using FlexMatrix.Api.Data.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace FlexMatrix.Api.Data.Repositories.CrudRepository
{
    public class CrudRepository : Repository, ICrudRepository
    {
        public CrudRepository(IUnitOfWork context) : base(context)
        {
        }


        public async Task<Dictionary<string, object>> GetById(string tableName, int id)
        {
            var query = $"SELECT * FROM [{TableSchema}].[{tableName}] WHERE {PrimaryKeyName} = @Id";
            var parameters = new Dictionary<string, object> { ["Id"] = id };

            var results = await Context.ExecuteSingleQuery(query, parameters);
            return results.Single();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName)
        {
            var query = $"SELECT * FROM [{TableSchema}].[{tableName}]";

            var results = await Context.ExecuteSingleQuery(query);
            return results;
        }

        public async Task<bool> Create(string tableName, Dictionary<string, object> columnValues)
        {
            var columns = string.Join(", ", columnValues.Keys.Select(k => $"[{k}]"));
            var parameters = string.Join(", ", columnValues.Keys.Select(k => "@" + k));
            var sql = $"INSERT INTO [{TableSchema}].[{tableName}] ({columns}) VALUES ({parameters})";

            var result = await Context.ExecuteCommand(sql, columnValues);
            return result;
        }

        public async Task<bool> Update(string tableName, Dictionary<string, object> columnValues)
        {
            var setClause = string.Join(", ", columnValues.Keys.Select(k => $"[{k}] = @{k}"));
            var sql = $"UPDATE [{TableSchema}].[{tableName}] SET {setClause} WHERE {PrimaryKeyName} = @Id";
            
            var results = await Context.ExecuteCommand(sql, columnValues);
            return results;
        }

        public async Task<bool> Delete(string tableName, int id)
        {
            var sql = $"DELETE FROM [{TableSchema}].[{tableName}] WHERE {PrimaryKeyName} = @Id";
            var parameters = new Dictionary<string, object> { ["Id"] = id };

            var result = await Context.ExecuteCommand(sql, parameters);
            return result;
        }
    }
}
