namespace FlexMatrix.Api.Data.Services.CrudService
{
    public interface ICrudService
    {
        Task<Dictionary<string, object>> GetById(string tableName, int id);

        Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName);

        Task<bool> Create(string tableName, Dictionary<string, object> columnValues);

        Task<bool> Update(string tableName, Dictionary<string, object> columnValues);

        Task<bool> Delete(string tableName, int id);
    }
}