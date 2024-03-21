using System.Data.Common;

namespace FlexMatrix.Api.Data.DataBase
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransaction();

        Task SaveTransaction();

        Task RollbackTransaction();

        Task<IEnumerable<Dictionary<string, object>>> GetData(string query, Dictionary<string, object> parameters);

        Task<bool> ExecuteScalarCommand(string sql, Dictionary<string, object> parameters);

        Task<bool> ExecuteCommand(string sql, Dictionary<string, object> parameters);
    }
}