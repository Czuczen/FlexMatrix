using System.Data.Common;

namespace FlexMatrix.Api.Data.DataBase
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<IEnumerable<Dictionary<string, object>>> ExecuteSingleQuery(string query, Dictionary<string, object>? parameters = null);

        Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> ExecuteMultiQuery(string query, int queriesNumber, Dictionary<string, object>? parameters = null);

        Task<bool> ExecuteCommand(string sql, Dictionary<string, object>? parameters = null);

        Task<object?> ExecuteScalarCommand(string sql, Dictionary<string, object>? parameters = null);

        Task BeginTransaction();

        Task SaveTransaction();

        Task RollbackTransaction();
    }
}