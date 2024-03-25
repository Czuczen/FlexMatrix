using System.Data.Common;

namespace FlexMatrix.Api.Data.DataBase
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<IEnumerable<Dictionary<string, object>>> ExecuteSingleQuery(string query, IEnumerable<Tuple<string, string, object>>? parameters = null);

        Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> ExecuteMultiQuery(string query, int queriesNumber, IEnumerable<Tuple<string, string, object>>? parameters = null);

        Task<bool> ExecuteCommand(string sql, IEnumerable<Tuple<string, string, object>>? parameters = null);

        Task<object?> ExecuteScalarCommand(string sql, IEnumerable<Tuple<string, string, object>>? parameters = null);

        Task BeginTransaction();

        Task SaveTransaction();

        Task RollbackTransaction();
    }
}