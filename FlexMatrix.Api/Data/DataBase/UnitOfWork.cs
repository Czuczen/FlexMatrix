using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace FlexMatrix.Api.Data.DataBase
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly DbConnection _connection;
        private DbTransaction? _transaction;


        public UnitOfWork(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

     
        private async Task OpenConnection()
        {
            if (_connection.State == ConnectionState.Broken)
                throw new InvalidOperationException("The connection is broken.");

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }


        public async Task BeginTransaction()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A new transaction cannot be started because the previous transaction has not yet been completed.");

            await OpenConnection();
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task SaveTransaction()
        {
            try
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            catch
            {
                await RollbackTransaction();
                throw;
            }
        }

        public async Task RollbackTransaction()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }

        //================================================================================
        //================================================================================
        //================================================================================

        public async Task<bool> CheckIfTableExists(string tableName)
        {
            if (_transaction == null)
                throw new InvalidOperationException("The transaction has not been opened.");

            // Używamy parametryzowanego zapytania, aby uniknąć SQL Injection
            var sql = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";

            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = _transaction;

            var tableNameParam = command.CreateParameter();
            tableNameParam.ParameterName = "@TableName";
            tableNameParam.Value = tableName;
            command.Parameters.Add(tableNameParam);

            // Jeśli znajdziemy tabelę, ExecuteScalar zwróci 1, w przeciwnym razie null
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<bool> ExecuteCommand(string sql, string tableName)
        {
            if (_transaction == null)
                throw new InvalidOperationException("The transaction has not been opened.");

            using var command = _connection.CreateCommand(); // jak bym zwracał command ? kiedy wykona się dispose
            command.Transaction = _transaction;
            command.CommandText = sql;

            var response = await command.ExecuteNonQueryAsync();
            var ret = await CheckIfTableExists(tableName);

            return ret;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetData(string query)
        {
            var results = new List<Dictionary<string, object>>();

            using (var command = _connection.CreateCommand())
            {
                if (_transaction == null)
                    throw new InvalidOperationException("The transaction has not been opened.");

                command.Transaction = _transaction;
                command.CommandText = query; // parametryzowane ?? 

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

                    results.Add(row);
                }
            }

            return results;
        }

    }
}
