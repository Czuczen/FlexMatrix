namespace FlexMatrix.Api.Data.Repositories.CrudRepository
{
    public interface ICrudRepository : IRepository
    {
        Task<Dictionary<string, object>> GetById(string tableName, int id);

        Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName);

        Task<bool> Create(string tableName, Dictionary<string, object> columnValues);

        Task<bool> Update(string tableName, int id, Dictionary<string, object> columnValues);

        Task<bool> Delete(string tableName, int id);
    }
}
