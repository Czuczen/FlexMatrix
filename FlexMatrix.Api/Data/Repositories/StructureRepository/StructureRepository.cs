using FlexMatrix.Api.Consts;
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

        public async Task<bool> CreateTableStructure(TableStructureDto tableStructure)
        {
            if (await TableExist(tableStructure.TableName)) return false;

            var sqlBuilder = new StringBuilder($"CREATE TABLE [{tableStructure.TableSchema}].[{tableStructure.TableName}] (");
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
                sqlBuilder.Append($@"ALTER TABLE [{tableStructure.TableSchema}].[{tableStructure.TableName}]
                    ADD CONSTRAINT FK_{tableStructure.TableName}_{column.Name}
                    FOREIGN KEY ([{column.Name}]) REFERENCES [{column.TableSchema}].[{column.ReferencesTableName}]({column.ReferencesTablePrimaryKeyName})");

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
            var sqlBuilder = new StringBuilder($"ALTER TABLE [{await GetTableSchema(tableName)}].[{tableName}] ADD [{column.Name}] {column.Type} {nullableSql}");

            if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                sqlBuilder.Append($" DEFAULT {column.DefaultValue}");

            sqlBuilder.Append(";");

            if (column.IsForeignKey)
            {
                sqlBuilder.Append($@"ALTER TABLE [{await GetTableSchema(tableName)}].[{tableName}]
                    ADD CONSTRAINT FK_{tableName}_{column.Name}
                    FOREIGN KEY ([{column.Name}]) REFERENCES [{column.TableSchema}].[{column.ReferencesTableName}]({column.ReferencesTablePrimaryKeyName})");

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
            var removeFKSql = $@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName)
                BEGIN
                    DECLARE @ConstraintName nvarchar(200);
                    SELECT @ConstraintName = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName;
                    EXEC('ALTER TABLE [{await GetTableSchema(tableName)}].[@TableName] DROP CONSTRAINT ' + @ConstraintName);
                END";

            var parameters = new List<Tuple<string, string, object>> { 
                new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName),
                new Tuple<string, string, object>("ColumnName", SqlTypes.VarChar, columnName)};

            await Context.ExecuteCommand(removeFKSql, parameters);

            var sql = $"ALTER TABLE [{await GetTableSchema(tableName)}].[{tableName}] DROP COLUMN [{columnName}];";
            var result = await Context.ExecuteCommand(sql);

            return result;
        }

        public async Task<bool> DeleteTable(string tableName)
        {
            // Opcjonalnie: Usuń wszystkie klucze obce powiązane z tą tabelą
            await RemoveRelations(tableName);

            // Usuń tabelę
            var sql = $"DROP TABLE IF EXISTS [{await GetTableSchema(tableName)}].[{tableName}];";
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
                AND TABLE_SCHEMA = '{await GetTableSchema(tableName)}';

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
