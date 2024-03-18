using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Repositories;

namespace FlexMatrix.Api.Data.Services
{
    public class StructureService : IStructureService
    {
        private readonly IRepository _repository;

        public StructureService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> CreateTableStructure(TableStructureDto tableStructure)
        {
            var result = await _repository.CreateTableAsync(tableStructure.TableName);

            return result;
        }
    }
}
