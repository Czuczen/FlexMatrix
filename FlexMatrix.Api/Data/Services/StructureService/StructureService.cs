using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Repositories.StructureRepository;

namespace FlexMatrix.Api.Data.Services.StructureService
{
    public class StructureService : IStructureService
    {
        private readonly IStructureRepository _repository;

        public StructureService(IStructureRepository repository)
        {
            _repository = repository;
        }


        public async Task<bool> CreateTableStructure(TableStructureDto tableStructure)
        {
            var result = await _repository.CreateTableStructure(tableStructure);
            return result;
        }
    }
}
