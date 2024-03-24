using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text;

namespace FlexMatrix.Api.Data.Repositories.StructureRepository
{
    public class StructureRepository : Repository, IStructureRepository
    {
        public StructureRepository(IUnitOfWork context) : base(context)
        {
        }

        public async Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> GetTableStructure(string tableName)
        {
            if (!await TableExist(tableName)) return null;

            // do przerobienia bo  OBJECT_ID(@TableName) to chyba typowo dla mssql
            // zapytania:
            // kolumny
            // klucze główne i unikalne
            // klucze obce
            var query = @"
                SELECT c.name AS ColumnName, t.Name AS DataType
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                WHERE c.object_id = OBJECT_ID(@TableName)
                ORDER BY c.column_id; 

                SELECT i.name AS IndexName, COL_NAME(ic.object_id, ic.column_id) AS ColumnName, i.is_primary_key, i.is_unique
                FROM sys.indexes AS i
                INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                WHERE i.object_id = OBJECT_ID(@TableName);

                SELECT 
                    fk.name AS ForeignKey,
                    tp.name AS ParentTable,
                    tr.name AS ReferenceTable,
                    cp.name AS ParentColumn,
                    cr.name AS ReferenceColumn
                FROM 
                    sys.foreign_keys AS fk
                INNER JOIN 
                    sys.tables AS tp ON fk.parent_object_id = tp.object_id
                INNER JOIN 
                    sys.tables AS tr ON fk.referenced_object_id = tr.object_id
                INNER JOIN 
                    sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN 
                    sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
                INNER JOIN 
                    sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
                WHERE 
                    tp.name = @TableName;";

            var parameters = new Dictionary<string, object> { ["TableName"] = tableName };
            var result = await Context.ExecuteMultiQuery(query, 3, parameters);
            return result;
        }

        public async Task<bool> CreateTableStructure(TableStructureDto tableStructure)
        {
            if (!await TableExist(tableStructure.TableName)) return false;

            var sqlBuilder = new StringBuilder($"CREATE TABLE [{TableSchema}].[{tableStructure.TableName}] (");
            AppendPrimaryKey(sqlBuilder, tableStructure);

            foreach (var column in tableStructure.Columns)
            {
                var nullableSql = column.IsNullable ? "NULL" : "NOT NULL";
                sqlBuilder.Append($"[{column.Name}] {column.Type} {nullableSql}");

                if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                    sqlBuilder.Append($" DEFAULT {column.DefaultValue}");

                sqlBuilder.Append(",");
            }

            sqlBuilder.Length--; // Remove a comma from the end of column definitions
            sqlBuilder.Append(");");

            foreach (var column in tableStructure.Columns.Where(c => c.IsForeignKey))
            {
                sqlBuilder.Append($@"ALTER TABLE [{TableSchema}].[{tableStructure.TableName}]
                    ADD CONSTRAINT FK_{tableStructure.TableName}_{column.Name}
                    FOREIGN KEY ([{column.Name}]) REFERENCES [{TableSchema}].[{column.ReferencesTableName}]({column.ReferencesTablePrimaryKeyName})");

                if (!string.IsNullOrWhiteSpace(column.DeleteType))
                    sqlBuilder.Append($" ON DELETE {column.DeleteType}");

                if (!string.IsNullOrWhiteSpace(column.UpdateType))
                    sqlBuilder.Append($" ON UPDATE {column.UpdateType}");

                sqlBuilder.Append(";");
            }

            var result = await Context.ExecuteCommand(sqlBuilder.ToString());
            return result;
        }

        public async Task<bool> AddColumnStructure(ColumnStructureDto column, string tableName)
        {
            if (await ColumnExist(tableName, column.Name))
                throw new InvalidOperationException($"Column '{column.Name}' already exists in table '{tableName}'.");

            var nullableSql = column.IsNullable ? "NULL" : "NOT NULL";
            var sqlBuilder = new StringBuilder($"ALTER TABLE [{TableSchema}].[{tableName}] ADD [{column.Name}] {column.Type} {nullableSql}");

            if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                sqlBuilder.Append($" DEFAULT {column.DefaultValue}");

            sqlBuilder.Append(";");

            if (column.IsForeignKey)
            {
                sqlBuilder.Append($@"ALTER TABLE [{TableSchema}].[{tableName}]
                    ADD CONSTRAINT FK_{tableName}_{column.Name}
                    FOREIGN KEY ([{column.Name}]) REFERENCES [{TableSchema}].[{column.ReferencesTableName}]({column.ReferencesTablePrimaryKeyName})");

                if (!string.IsNullOrWhiteSpace(column.DeleteType))
                    sqlBuilder.Append($" ON DELETE {column.DeleteType}");

                if (!string.IsNullOrWhiteSpace(column.UpdateType))
                    sqlBuilder.Append($" ON UPDATE {column.UpdateType}");

                sqlBuilder.Append(";");
            }

            var result = await Context.ExecuteCommand(sqlBuilder.ToString());
            return result;
        }

        public async Task<bool> RemoveColumn(string tableName, string columnName)
        {
            // Najpierw usuń relacje dla tej kolumny, jeśli istnieją
            var removeFKSql = $@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}')
                BEGIN
                    DECLARE @ConstraintName nvarchar(200);
                    SELECT @ConstraintName = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}';
                    EXEC('ALTER TABLE [{TableSchema}].[' + {tableName} + '] DROP CONSTRAINT ' + @ConstraintName);
                END";

            await Context.ExecuteCommand(removeFKSql);

            // Następnie usuń kolumnę
            var sql = $"ALTER TABLE [{TableSchema}].[{tableName}] DROP COLUMN [{columnName}];";
            var result = await Context.ExecuteCommand(sql);
            return result;
        }

        public async Task<bool> DeleteTable(string tableName)
        {
            // Opcjonalnie: Usuń wszystkie klucze obce powiązane z tą tabelą
            await RemoveRelations(tableName);

            // Usuń tabelę
            var sql = $"DROP TABLE IF EXISTS [{TableSchema}].[{tableName}];";
            var result = await Context.ExecuteCommand(sql);
            return result;
        }

        public async Task<bool> RemoveRelations(string tableName)
        {
            var sql = $@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) 
                             + ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + '; '
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                WHERE CONSTRAINT_TYPE = 'FOREIGN KEY' 
                AND TABLE_NAME = '{tableName}'
                AND TABLE_SCHEMA = '{TableSchema}';

                EXEC sp_executesql @sql;
                ";

            var result = await Context.ExecuteCommand(sql);
            return result;
        }


        // =================================================
        // ==================== HELPERS ====================
        // =================================================


        private static void AppendPrimaryKey(StringBuilder sqlBuilder, TableStructureDto tableStructure)
        {
            var primaryKeyName = tableStructure.PrimaryKeyName;
            switch (tableStructure.PrimaryKeyType.ToUpper())
            {
                case "UNIQUEIDENTIFIER":
                    sqlBuilder.Append($"[{primaryKeyName}] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
                    break;
                case "INT":
                    sqlBuilder.Append($"[{primaryKeyName}] INT PRIMARY KEY IDENTITY(1,1),");
                    break;
                case "BIGINT":
                    sqlBuilder.Append($"[{primaryKeyName}] BIGINT PRIMARY KEY IDENTITY(1,1),");
                    break;
                case "GUID":
                    sqlBuilder.Append($"[{primaryKeyName}] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
                    break;
                case "VARCHAR":
                    sqlBuilder.Append($"[{primaryKeyName}] VARCHAR(36) PRIMARY KEY,");
                    break;
                default:
                    throw new NotImplementedException("Primary key type not implemented");
            }
        }
    }
}
