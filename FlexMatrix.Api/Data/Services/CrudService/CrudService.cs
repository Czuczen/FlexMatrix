using FlexMatrix.Api.Data.Repositories.CrudRepository;

namespace FlexMatrix.Api.Data.Services.CrudService
{
    public class CrudService : ICrudService
    {
        private readonly ICrudRepository _crudRepository;


        public CrudService(ICrudRepository crudRepository)
        {
            _crudRepository = crudRepository;
        }


        public async Task<Dictionary<string, object>> GetById(string tableName, int id)
        {
            return await _crudRepository.GetById(tableName, id);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAll(string tableName)
        {
            return await _crudRepository.GetAll(tableName);
        }

        public async Task<bool> Create(string tableName, Dictionary<string, object> columnValues)
        {
            return await _crudRepository.Create(tableName, columnValues);
        }

        public async Task<bool> Update(string tableName, Dictionary<string, object> columnValues)
        {
            return await _crudRepository.Update(tableName, columnValues);
        }

        public async Task<bool> Delete(string tableName, int id)
        {
            return await _crudRepository.Delete(tableName, id);
        }
    }
}
