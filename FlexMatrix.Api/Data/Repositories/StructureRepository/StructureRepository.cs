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


        public async Task<bool> CreateTable(TableStructureDto tableStructure)
        {
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

        public async Task<bool> AddColumn(string tableName, string columnName, string dataType)
        {
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
    }
}
