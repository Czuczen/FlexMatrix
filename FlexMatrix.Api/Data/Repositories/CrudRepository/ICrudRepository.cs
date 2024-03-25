namespace FlexMatrix.Api.Data.Repositories.CrudRepository
{
    public interface ICrudRepository
    {
        Task<Dictionary<string, object>> GetById(string tableName, object id);

        Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName);

        Task<bool> Create(string tableName, Dictionary<string, object> obj);

        Task<bool> Update(string tableName, Dictionary<string, object> obj);

        Task<bool> Delete(string tableName, int id);
    }
}
