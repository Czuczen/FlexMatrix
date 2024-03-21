using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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
            var sql = $"CREATE TABLE [dbo].[{tableStructure.TableName}] (";

            if (tableStructure.PrimaryKeyType == "UNIQUEIDENTIFIER")
                sql += "[Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),";
            else if (tableStructure.PrimaryKeyType == "INT")
                sql += "[Id] INT PRIMARY KEY IDENTITY(1,1),";
            // inne

            foreach (var column in tableStructure.Columns)
            {
                sql += $@"[{column.Name}] {column.Type},";
            }

            sql += ");";

            foreach (var column in tableStructure.Columns.Where(c => c.IsForeignKey))
            {
                sql += $@"ALTER TABLE [dbo].[{tableStructure.TableName}]
                            ADD CONSTRAINT FK_{tableStructure.TableName}_{column.Name}
                            FOREIGN KEY ([{column.Name}]) REFERENCES [dbo].[{column.ReferencesTableName}](Id);";
            }
            
            //var sql = $"CREATE TABLE {tableName} (Id INT PRIMARY KEY IDENTITY(1,1))";
            var parameters = new Dictionary<string, object> { ["@TableName"] = tableStructure.TableName };

            var result = await _context.ExecuteCommand(sql, parameters); // parameters potrzebne?
            return result;
        }

        public async Task<bool> AddColumn(string tableName, string columnName, string dataType)
        {
            var sql = $"ALTER TABLE {tableName} ADD {columnName} {dataType}";
            var parameters = new Dictionary<string, object> { ["@TableName"] = tableName };

            var result = await _context.ExecuteCommand(sql, parameters);
            return result;
        }

    }
}
