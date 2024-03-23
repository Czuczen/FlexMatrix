using FlexMatrix.Api.Data.Models;

namespace FlexMatrix.Api.Data.Repositories.StructureRepository
{
    public interface IStructureRepository
    {
        Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> GetTableStructure(string tableName);

        Task<bool> CreateTableStructure(TableStructureDto tableStructure);

        Task<bool> AddColumn(string tableName, string columnName, string dataType);
    }
}