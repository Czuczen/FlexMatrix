using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text;

namespace FlexMatrix.Api.Data.Repositories.StructureRepository
{
    public class StructureRepository : IStructureRepository
    {
        private readonly IUnitOfWork _context;

        public StructureRepository(IUnitOfWork context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> GetTableStructure(string tableName)
        {
            if (!await TableExist(tableStructure.TableName)) return false;

            // zapytania:
            // kolumny
            // klucze główne i unikalne
            // klucze obce
            var query = @"SELECT c.name AS ColumnName, t.Name AS DataType
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
            var result = await _context.ExecuteMultiQuery(query, 3, parameters);
            return result;
        }

        public async Task<bool> CreateTableStructure(TableStructureDto tableStructure)
        {
            if (!await TableExist(tableStructure.TableName)) return false;

            var sqlBuilder = new StringBuilder($"CREATE TABLE [dbo].[{tableStructure.TableName}] (");

            if (tableStructure.PrimaryKeyType == "UNIQUEIDENTIFIER")
                sqlBuilder.Append("[Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
            else if (tableStructure.PrimaryKeyType == "INT")
                sqlBuilder.Append("[Id] INT PRIMARY KEY IDENTITY(1,1),");
            else if (tableStructure.PrimaryKeyType == "BIGINT")
                sqlBuilder.Append("[Id] BIGINT PRIMARY KEY IDENTITY(1,1),");
            else if (tableStructure.PrimaryKeyType == "GUID")
                sqlBuilder.Append("[Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
            else if (tableStructure.PrimaryKeyType == "VARCHAR")
                sqlBuilder.Append("[Id] VARCHAR(36) PRIMARY KEY,");

            foreach (var column in tableStructure.Columns)
                sqlBuilder.Append($"[{column.Name}] {column.Type},");

            // Remove a comma from the end of column definitions
            sqlBuilder.Length--;
            sqlBuilder.Append(");");

            foreach (var column in tableStructure.Columns.Where(c => c.IsForeignKey))
            {
                sqlBuilder.Append($@"ALTER TABLE [dbo].[{tableStructure.TableName}]
                    ADD CONSTRAINT FK_{tableStructure.TableName}_{column.Name}
                    FOREIGN KEY ([{column.Name}]) REFERENCES [dbo].[{column.ReferencesTableName}](Id)");

                switch (column.DeleteType)
                {
                    case "CASCADE":
                        sqlBuilder.Append(" ON DELETE CASCADE");
                        break;
                    case "SET NULL":
                        sqlBuilder.Append(" ON DELETE SET NULL");
                        break;
                    case "SET DEFAULT":
                        sqlBuilder.Append(" ON DELETE SET DEFAULT");
                        break;
                    case "NO ACTION":
                        sqlBuilder.Append(" ON DELETE NO ACTION");
                        break;
                    case "RESTRICT":
                        sqlBuilder.Append(" ON DELETE RESTRICT");
                        break;
                }

                switch (column.UpdateType)
                {
                    case "CASCADE":
                        sqlBuilder.Append(" ON UPDATE CASCADE");
                        break;
                    case "SET NULL":
                        sqlBuilder.Append(" ON UPDATE SET NULL");
                        break;
                    case "SET DEFAULT":
                        sqlBuilder.Append(" ON UPDATE SET DEFAULT");
                        break;
                    case "NO ACTION":
                        sqlBuilder.Append(" ON UPDATE NO ACTION");
                        break;
                    case "RESTRICT":
                        sqlBuilder.Append(" ON UPDATE RESTRICT");
                        break;
                }

                sqlBuilder.Append(";");
            }

            var result = await _context.ExecuteCommand(sqlBuilder.ToString());
            return result;
        }

        // ======================

        public async Task<bool> AddColumn(string tableName, string columnName, string dataType, string? defaultValue = null, bool isNullable = true, ForeignKeyConstraint? foreignKey = null)
        {
            if (await ColumnExist(tableName, columnName))
            {
                throw new InvalidOperationException($"Column '{columnName}' already exists in table '{tableName}'.");
            }

            var nullableSql = isNullable ? "NULL" : "NOT NULL";
            var sqlBuilder = new StringBuilder($"ALTER TABLE [dbo].[{tableName}] ADD [{columnName}] {dataType} {nullableSql}");

            // Dodanie wartości domyślnej, jeśli jest określona
            if (!string.IsNullOrEmpty(defaultValue))
            {
                sqlBuilder.Append($" DEFAULT {defaultValue}");
            }

            sqlBuilder.Append(";");

            // Dodanie klucza obcego, jeśli został określony
            if (foreignKey != null)
            {
                sqlBuilder.Append($@"
            ALTER TABLE [dbo].[{tableName}]
            ADD CONSTRAINT FK_{tableName}_{columnName}
            FOREIGN KEY ([{columnName}]) REFERENCES [dbo].[{foreignKey.ReferencesTableName}](Id)");

                // Obsługa różnych strategii usuwania
                if (!string.IsNullOrEmpty(foreignKey.DeleteAction))
                {
                    sqlBuilder.Append($" ON DELETE {foreignKey.DeleteAction}");
                }

                // Obsługa różnych strategii aktualizacji
                if (!string.IsNullOrEmpty(foreignKey.UpdateAction))
                {
                    sqlBuilder.Append($" ON UPDATE {foreignKey.UpdateAction}");
                }

                sqlBuilder.Append(";");
            }

            var result = await _context.ExecuteCommand(sqlBuilder.ToString());
            return result;
        }

        // Klasa pomocnicza dla ograniczenia klucza obcego
        public class ForeignKeyConstraint
        {
            public string ReferencesTableName { get; set; }
            public string DeleteAction { get; set; }
            public string UpdateAction { get; set; }
        }

        // ======================


        public async Task<bool> AddColumn(string tableName, string columnName, string dataType)
        {
            // check column existance

            var sql = $"ALTER TABLE {tableName} ADD {columnName} {dataType}";

            var result = await _context.ExecuteCommand(sql);
            return result;
        }

        public async Task<bool> RemoveColumn()
        {
            var sql = $"";


            // jeśli kolumna relacyjna to usunąć relację?
            var result = await _context.ExecuteCommand(sql);
            return result;
        }

        public async Task<bool> DeleteTable()
        {
            var sql = $"";

            // alter table relayjne też usunąć?
            var result = await _context.ExecuteCommand(sql);
            return result;
        }

        public async Task<bool> RemoveRelations()
        {
            var sql = $"";

            var result = await _context.ExecuteCommand(sql);
            return result;
        }



        // ============================================================================================================
        // ============================================================================================================
        // ============================================================================================================


        private async Task<bool> ColumnExist(string tableName, string columnName)
        {
            var sql = @"IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName) SELECT 1 ELSE SELECT 0";
            var parameters = new Dictionary<string, object> { ["TableName"] = tableName, ["ColumnName"] = columnName };

            var result = await _context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        private async Task<bool> TableExist(string tableName)
        {
            var sql = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
            var parameters = new Dictionary<string, object> { ["TableName"] = tableName };

            var result = await _context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetAllTables()
        {
            var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";

            var result = await _context.ExecuteSingleQuery(query);
            return result;
        }
    }
}
