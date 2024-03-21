using FlexMatrix.Api.Data.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace FlexMatrix.Api.Data.Repositories
{
    public class Repository : IRepository
    {
        private readonly IUnitOfWork _context;


        public Repository(IUnitOfWork context)
        {
            _context = context;
        }


        public async Task<Dictionary<string, object>> GetById(string tableName, int id)
        {
            var query = $"SELECT * FROM {tableName} WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { ["@Id"] = id };

            var results = await _context.GetData(query, parameters);
            return results.Single();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName)
        {
            var query = $"SELECT * FROM {tableName}";
            var parameters = new Dictionary<string, object> { ["@Id"] = tableName };

            var results = await _context.GetData(query, parameters);
            return results;
        }

        public async Task<bool> Create(string tableName, Dictionary<string, object> columnValues)
        {
            var columns = string.Join(", ", columnValues.Keys);
            var parameters = string.Join(", ", columnValues.Keys.Select(k => "@" + k));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            var result = await _context.ExecuteCommand(sql, columnValues);
            return result;
        }

        public async Task<bool> Update(string tableName, int id, Dictionary<string, object> columnValues)
        {
            var setClause = string.Join(", ", columnValues.Keys.Select(k => $"{k} = @{k}"));
            var sql = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";

            var results = await _context.ExecuteCommand(sql, columnValues);
            return results;
        }

        public async Task<bool> Delete(string tableName, int id)
        {
            var sql = $"DELETE FROM {tableName} WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { ["@Id"] = id };

            var result = await _context.ExecuteCommand(sql, parameters);
            return result;
        }

        // ====================================================================================================

        public async Task<bool> TableExist(string tableName)
        {
            var sql = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
            var parameters = new Dictionary<string, object> { ["@TableName"] = tableName };

            var result = await _context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAllTables()
        {
            var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
            var parameters = new Dictionary<string, object> { ["@TableName"] = "" };

            var result = await _context.GetData(query, parameters);
            return result;
        }
    }
}
