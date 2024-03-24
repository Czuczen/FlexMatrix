using FlexMatrix.Api.Data.DataBase;

namespace FlexMatrix.Api.Data.Repositories
{
    public abstract class Repository
    {
        protected readonly IUnitOfWork Context;

        protected string PrimaryKeyName { get; set; } = "Id";

        protected string TableSchema { get; set; } = "dbo";



        public Repository(IUnitOfWork context)
        {
            Context = context;
        }


        public async Task<bool> ColumnExist(string tableName, string columnName)
        {
            var sql = @"IF EXISTS(
                            SELECT 1 
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_SCHEMA = @TableSchema 
                            AND TABLE_NAME = @TableName 
                            AND COLUMN_NAME = @ColumnName
                        )
                        SELECT CAST(1 AS BIT);
                        ELSE
                        SELECT CAST(0 AS BIT);";

            var parameters = new Dictionary<string, object> { ["TableName"] = tableName, 
                ["ColumnName"] = columnName, ["TableSchema"] = TableSchema };

            var result = (bool) await Context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        public async Task<bool> TableExist(string tableName)
        {
            var sql = @"IF EXISTS(
                            SELECT 1 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_SCHEMA = @TableSchema
                            AND TABLE_NAME = @TableName
                        )
                        SELECT CAST(1 AS BIT);
                        ELSE
                        SELECT CAST(0 AS BIT);";

            var parameters = new Dictionary<string, object> { ["TableName"] = tableName, ["TableSchema"] = TableSchema };
            var result = (bool) await Context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        public async Task<IEnumerable<string>> GetTableNames()
        {
            var query = @"SELECT TABLE_NAME 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = @TableSchema;";

            var parameters = new Dictionary<string, object> { ["TableSchema"] = TableSchema };
            var result = await Context.ExecuteSingleQuery(query, parameters);

            var tableNames = result.Select(dict => dict["TABLE_NAME"].ToString());
            return tableNames;
        }

    }
}
