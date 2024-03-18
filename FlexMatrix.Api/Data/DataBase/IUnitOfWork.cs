using System.Data.Common;

namespace FlexMatrix.Api.Data.DataBase
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransaction();

        Task SaveTransaction();

        Task RollbackTransaction();

        Task<bool> ExecuteCommand(string sql, string tableName);

        Task<IEnumerable<Dictionary<string, object>>> GetData(string query);
    }
}