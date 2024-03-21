﻿using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace FlexMatrix.Api.Data.DataBase;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;


    public UnitOfWork(ILogger<UnitOfWork> logger, string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _logger = logger;
    }


    private async Task OpenConnection()
    {
        _logger.LogDebug("Opening database connection");

        if (_connection.State == ConnectionState.Broken) // obsługa innych statusów?
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
    }

    // ===========================================================================================
    // ===========================================================================================
    // ===========================================================================================

    public async Task<IEnumerable<Dictionary<string, object>>> GetData(string query, Dictionary<string, object> parameters)
    {
        var results = new List<Dictionary<string, object>>();

        using (var command = _connection.CreateCommand())
        {
            if (_transaction == null)
                throw new InvalidOperationException("The transaction has not been opened.");

            command.Transaction = _transaction;
            command.CommandText = query;

            foreach (var item in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{item.Key}";
                parameter.Value = item.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
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

        return results;
    }

    public async Task<bool> ExecuteScalarCommand(string sql, Dictionary<string, object> parameters)
    {
        if (_transaction == null)
            throw new InvalidOperationException("The transaction has not been opened.");

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        foreach (var item in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{item.Key}";
            parameter.Value = item.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        // Jeśli znajdziemy tabelę, ExecuteScalar zwróci 1, w przeciwnym razie null
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> ExecuteCommand(string sql, Dictionary<string, object> parameters)
    {
        if (_transaction == null)
            throw new InvalidOperationException("The transaction has not been opened.");

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        foreach (var item in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{item.Key}";
            parameter.Value = item.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }
}