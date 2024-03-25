using FlexMatrix.Api.Consts;
using FlexMatrix.Api.Data.DataBase;

namespace FlexMatrix.Api.Data.Repositories
{
    public abstract class Repository
    {
        protected readonly IUnitOfWork Context;

        protected string PrimaryKeyName { get; set; } = "Id";

        protected string TableSchema { get; set; } = "dbo";



        protected Repository(IUnitOfWork context)
        {
            Context = context;
        }

        protected async Task<IEnumerable<Tuple<string, string, object>>> GenerateParameters(string tableName, Dictionary<string, object> dictParameters)
        {
            var ret = new List<Tuple<string, string, object>>();

            var columnsInfo = await GetColumnNamesWithTypes(tableName);

            foreach (var param in dictParameters)
                ret.Add(new Tuple<string, string, object>(param.Key, columnsInfo.Single(c =>
                    c["ColumnName"].ToString() == param.Key)["DataType"].ToString(), param.Value));

            return ret; ;
        }

        protected async Task<bool> ColumnExist(string tableName, string columnName)
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

            var parameters = new List<Tuple<string, string, object>>
            {
                new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName),
                new Tuple<string, string, object>("ColumnName", SqlTypes.VarChar, columnName),
                new Tuple<string, string, object>("TableSchema", SqlTypes.VarChar, TableSchema)
            };

            var result = (bool) await Context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        protected async Task<bool> TableExist(string tableName)
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

            var parameters = new List<Tuple<string, string, object>>
            {
                new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName),
                new Tuple<string, string, object>("TableSchema", SqlTypes.VarChar, TableSchema)
            };

            var result = (bool) await Context.ExecuteScalarCommand(sql, parameters);
            return result;
        }

        protected async Task<IEnumerable<string>> GetTableNames()
        {
            var query = @"SELECT TABLE_NAME 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = @TableSchema;";

            var parameters = new List<Tuple<string, string, object>>{ new Tuple<string, string, object>("TableSchema", SqlTypes.VarChar, TableSchema) };
            var result = await Context.ExecuteSingleQuery(query, parameters);

            var tableNames = result.Select(dict => dict["TABLE_NAME"].ToString());
            return tableNames;
        }

        protected async Task<IEnumerable<Dictionary<string, object>>> GetColumnNamesWithTypes(string tableName)
        {
            var query = @"SELECT COLUMN_NAME AS 'ColumnName', DATA_TYPE AS 'DataType'
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = @TableName
                            ORDER BY ORDINAL_POSITION;";

            var parameters = new List<Tuple<string, string, object>>{ new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName) };
            var columns = await Context.ExecuteSingleQuery(query, parameters);
            return columns;
        }

        protected async Task<IEnumerable<Dictionary<string, object>>> GetIndexes(string tableName)
        {
            var query = @"SELECT 
                            tc.CONSTRAINT_NAME AS 'IndexName', 
                            kcu.COLUMN_NAME AS 'ColumnName', 
                            CASE WHEN tc.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS 'IsPrimaryKey', 
                            CASE WHEN tc.CONSTRAINT_TYPE = 'UNIQUE' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS 'IsUnique'
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                        WHERE tc.TABLE_NAME = @TableName;";

            var parameters = new List<Tuple<string, string, object>>{ new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName) };
            var indexes = await Context.ExecuteSingleQuery(query, parameters);
            return indexes;
        }

        protected async Task<IEnumerable<Dictionary<string, object>>> GetForeignKeys(string tableName)
        {
            var query = @"SELECT 
                            fk.CONSTRAINT_NAME AS 'ForeignKey', 
                            fk.TABLE_NAME AS 'ParentTable', 
                            fk.REFERENCED_TABLE_NAME AS 'ReferenceTable', 
                            kcu.COLUMN_NAME AS 'ParentColumn', 
                            kcu.REFERENCED_COLUMN_NAME AS 'ReferenceColumn'
                        FROM 
                            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS fk
                        INNER JOIN 
                            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu ON fk.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                        WHERE 
                            fk.TABLE_NAME = @TableName;";

            var parameters = new List<Tuple<string, string, object>>{ new Tuple<string, string, object>("TableName", SqlTypes.VarChar, tableName) };
            var foreignKeys = await Context.ExecuteSingleQuery(query, parameters);
            return foreignKeys;
        }
    }
}
