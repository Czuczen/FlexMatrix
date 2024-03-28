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

        public async Task<bool> AddColumnStructure(ColumnStructureDto column, string tableName)
        {
            var result = await _repository.AddColumnStructure(column, tableName);
            return result;
        }

        public async Task<bool> DeleteTable(string tableName)
        {
            var result = await _repository.DeleteTable(tableName);
            return result;
        }

        public async Task<bool> RemoveColumn(string tableName, string columnName)
        {
            var result = await _repository.RemoveColumn(tableName, columnName);
            return result;
        }

        public async Task<bool> RemoveRelations(string tableName)
        {
            var result = await _repository.RemoveRelations(tableName);
            return result;
        }
    }
}
