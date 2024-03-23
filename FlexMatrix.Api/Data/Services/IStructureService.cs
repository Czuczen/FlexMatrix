using FlexMatrix.Api.Data.Models;

namespace FlexMatrix.Api.Data.Services
{
    public interface IStructureService
    {
        Task<IEnumerable<IEnumerable<Dictionary<string, object>>>> GetTableStructure(string tableName);

        Task<bool> CreateTableStructure(TableStructureDto tableStructure);
    }
}