using FlexMatrix.Api.Data.Models;

namespace FlexMatrix.Api.Data.Repositories.StructureRepository
{
    public interface IStructureRepository
    {
        Task<bool> CreateTableStructure(TableStructureDto tableStructure);

        Task<bool> AddColumnStructure(ColumnStructureDto column, string tableName);



        Task<bool> RemoveColumn(string tableName, string columnName);

        Task<bool> DeleteTable(string tableName);

        Task<bool> RemoveRelations(string tableName);
    }
}
