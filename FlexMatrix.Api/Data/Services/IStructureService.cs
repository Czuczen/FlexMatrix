using FlexMatrix.Api.Data.Models;

namespace FlexMatrix.Api.Data.Services
{
    public interface IStructureService
    {
        Task<bool> CreateTableStructure(TableStructureDto tableStructure);
    }
}