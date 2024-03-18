using FlexMatrix.Api.Data.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FlexMatrix.Api.Data.Repositories
{
    public class Repository : IRepository
    {
        private readonly IUnitOfWork _context;

        public Repository(IUnitOfWork context)
        {
            _context = context;
        }


        public async Task<bool> CreateTableAsync(string tableName)
        {
            var sdfsdf = await GetAllTablesAsync();

            var sql = $"CREATE TABLE {tableName} (Id INT PRIMARY KEY IDENTITY(1,1))";
            var result = await _context.ExecuteCommand(sql, tableName);
            //await _context.CommitAsync();
            return result;
        }

        public async Task<bool> AddColumnToTableAsync(string tableName, string columnName, string dataType)
        {
            var sql = $"ALTER TABLE {tableName} ADD {columnName} {dataType}";
            var result = await _context.ExecuteCommand(sql, tableName);
            return result;
        }

        public async Task<IEnumerable<dynamic>> GetAllTablesAsync()
        {
            return await _context.GetData("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
        }
    }
}
