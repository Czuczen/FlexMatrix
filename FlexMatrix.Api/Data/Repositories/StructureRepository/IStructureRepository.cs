using FlexMatrix.Api.Data.Models;

namespace FlexMatrix.Api.Data.Repositories.StructureRepository
{
    public interface IStructureRepository
    {
        Task<bool> CreateTable(TableStructureDto tableStructure);

        Task<bool> AddColumn(string tableName, string columnName, string dataType);
    }
}