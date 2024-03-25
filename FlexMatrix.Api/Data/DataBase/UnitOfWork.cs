using FlexMatrix.Api.Consts;
using FlexMatrix.Api.Data.Parser;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace FlexMatrix.Api.Data.DataBase;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IParser _parser;
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;



    public UnitOfWork(ILogger<UnitOfWork> logger, IParser parser, string connectionString)
    {
        _logger = logger;
        _parser = parser;
        _connection = new SqlConnection(connectionString);
    }


    public async Task<IEnumerable<Dictionary<string, object>>> ExecuteSingleQuery(string query, IEnumerable<Tuple<string, string, object>>? parameters = null)
    {
        _logger.LogDebug("Executing single query. \nQuery: " + query);

        var results = new List<Dictionary<string, object>>();

        using (var command = _connection.CreateCommand())
        {
            if (_transaction == null)
                throw new InvalidOperationException("The transaction has not been opened.");

            command.Transaction = _transaction;
            command.CommandText = query;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{param.Item1}";
                    parameter.Value = param.Item3 != null ? _parser.Parse(ParseStrategies.ToDb, param.Item2, param.Item3) : DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

                results.Add(row);
            }
        }

        _logger.LogDebug($"Single query executed. Result: {results.Count} \nQuery: {query}");

        return results;
    }

    public async Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> ExecuteMultiQuery(string query, int queriesNumber,
        IEnumerable<Tuple<string, string, object>>? parameters = null)
    {
        _logger.LogDebug("Executing multi query. \nQuery: " + query);

        var results = new List<List<Dictionary<string, object>>>();

        using (var command = _connection.CreateCommand())
        {
            if (_transaction == null)
                throw new InvalidOperationException("The transaction has not been opened.");

            command.Transaction = _transaction;
            command.CommandText = query;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{param.Item1}";
                    parameter.Value = param.Item3 != null ? _parser.Parse(ParseStrategies.ToDb, param.Item2, param.Item3) : DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            using var reader = await command.ExecuteReaderAsync();

            while (queriesNumber > 0)
            {
                var objectsList = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

                    objectsList.Add(row);
                }

                results.Add(objectsList);
                queriesNumber--;

                if (queriesNumber > 0)
                    await reader.NextResultAsync();
            }
        }

        _logger.LogDebug($"Multi query executed. Result: {results.Count} \nQuery: {query}");

        return results;
    }

    public async Task<bool> ExecuteCommand(string sql, IEnumerable<Tuple<string, string, object>>? parameters = null)
    {
        _logger.LogDebug("Executing command. \nSql: " + sql);

        if (_transaction == null)
            throw new InvalidOperationException("The transaction has not been opened.");

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{param.Item1}";
                parameter.Value = param.Item3 != null ? _parser.Parse(ParseStrategies.ToDb, param.Item2, param.Item3) : DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        var result = await command.ExecuteNonQueryAsync();

        _logger.LogDebug($"Command executed. Result: {result} \nSql: {sql}");

        return result > 0;
    }

    public async Task<object?> ExecuteScalarCommand(string sql, IEnumerable<Tuple<string, string, object>>? parameters = null)
    {
        _logger.LogDebug("Executing scalar command. \nSql: " + sql);

        if (_transaction == null)
            throw new InvalidOperationException("The transaction has not been opened.");

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{param.Item1}";
                parameter.Value = param.Item3 != null ? _parser.Parse(ParseStrategies.ToDb, param.Item2, param.Item3) : DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        var result = await command.ExecuteScalarAsync();

        _logger.LogDebug($"Scalar command executed. Result: {result} \nSql: {sql}");

        return result;
    }

    private async Task OpenConnection()
    {
        _logger.LogDebug("Opening database connection");

        if (_connection.State == ConnectionState.Broken) // obsługa innych statusów?
            throw new InvalidOperationException("The connection is broken.");

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
            _logger.LogDebug("Database connection opened");
        }
        else
            _logger.LogDebug("Database connection already open");
    }

    public async Task BeginTransaction()
    {
        _logger.LogDebug("Starting transaction");
        if (_transaction != null)
            throw new InvalidOperationException("A new transaction cannot be started because the previous transaction has not yet been completed.");

        await OpenConnection();
        _transaction = await _connection.BeginTransactionAsync();
        _logger.LogDebug("Transaction started");
    }

    public async Task SaveTransaction()
    {
        try
        {
            _logger.LogDebug("Saving transaction");
            await _transaction.CommitAsync();
            _logger.LogDebug("Transaction saved");
            _logger.LogDebug("Clearing transaction");
            await _transaction.DisposeAsync();
            _transaction = null;
            _logger.LogDebug("Transaction cleared");
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
            _logger.LogDebug("Rollbacking transaction");
            await _transaction.RollbackAsync();
            _logger.LogDebug("Transaction rollbacked");
            _logger.LogDebug("Clearing transaction");
            await _transaction.DisposeAsync();
            _transaction = null;
            _logger.LogDebug("Transaction cleared");
        }
        else
            _logger.LogError("Rollback transaction failed. Transaction is null.");
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Unit of work start clearing resources");
        if (_transaction != null)
        {
            try
            {
                await _transaction.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't dispose transaction", ex);
            }
        }
            
        if (_connection != null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("The connection could not be closed", ex);
            }

            try
            {
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't dispose connection", ex);
            }
        }

        _logger.LogDebug("Unit of work complete clearing resources");
    }
}
