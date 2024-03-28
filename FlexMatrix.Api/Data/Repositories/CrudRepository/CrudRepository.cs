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


        public async Task<Dictionary<string, object>> GetById(string tableName, object id)
        {
            var query = $"SELECT * FROM [{await GetTableSchema(tableName)}].[{tableName}] WHERE {await GetPrimaryKeyName(tableName)} = @Id";

            var parameters = await GenerateParameters(tableName, new Dictionary<string, object> { ["Id"] = id });

            var results = await Context.ExecuteSingleQuery(query, parameters);
            return results.Single();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName)
        {
            var query = $"SELECT * FROM [{await GetTableSchema(tableName)}].[{tableName}]";

            var results = await Context.ExecuteSingleQuery(query);
            return results;
        }

        public async Task<bool> Create(string tableName, Dictionary<string, object> obj)
        {
            var columns = string.Join(", ", obj.Keys.Select(k => $"[{k}]"));
            var sqlParameters = string.Join(", ", obj.Keys.Select(k => "@" + k));
            var sql = $"INSERT INTO [{await GetTableSchema(tableName)}].[{tableName}] ({columns}) VALUES ({sqlParameters})";
            var parameters = await GenerateParameters(tableName, obj);

            var result = await Context.ExecuteCommand(sql, parameters);
            return result;
        }

        public async Task<bool> Update(string tableName, Dictionary<string, object> obj)
        {
            var setClause = string.Join(", ", obj.Keys.Select(k => $"[{k}] = @{k}"));
            var sql = $"UPDATE [{await GetTableSchema(tableName)}].[{tableName}] SET {setClause} WHERE {await GetPrimaryKeyName(tableName)} = @Id";
            var parameters = await GenerateParameters(tableName, obj);

            var result = await Context.ExecuteCommand(sql, parameters);
            return result;
        }

        public async Task<bool> Delete(string tableName, int id)
        {
            var sql = $"DELETE FROM [{await GetTableSchema(tableName)}].[{tableName}] WHERE {await GetPrimaryKeyName(tableName)} = @Id";
            var parameters = await GenerateParameters(tableName, new Dictionary<string, object> { ["Id"] = id });

            var result = await Context.ExecuteCommand(sql, parameters);
            return result;
        }
    }
}
