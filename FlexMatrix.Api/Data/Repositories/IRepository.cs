namespace FlexMatrix.Api.Data.Repositories
{
    public interface IRepository
    {
        Task<bool> ColumnExist(string tableName, string columnName);

        Task<bool> TableExist(string tableName);

        Task<IEnumerable<string>> GetTableNames();
    }
}
