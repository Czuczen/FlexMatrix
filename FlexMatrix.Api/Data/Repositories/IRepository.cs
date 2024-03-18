namespace FlexMatrix.Api.Data.Repositories
{
    public interface IRepository
    {
        Task<bool> CreateTableAsync(string tableName);

        Task<bool> AddColumnToTableAsync(string tableName, string columnName, string dataType);

        Task<IEnumerable<dynamic>> GetAllTablesAsync();
    }
}