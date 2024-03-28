using FlexMatrix.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace FlexMatrix.Api.Data.Models
{
    public class TableStructureDto
    {
        [SafeForSql]
        public string TableSchema { get; set; } = "dbo";

        [SafeForSql]
        public string TableName { get; set; }

        [SafeForSql]
        public string PrimaryKeyName { get; set; } = "Id";

        [SafeForSql]
        public string PrimaryKeyType { get; set; }

        public IEnumerable<ColumnStructureDto> Columns { get; set; }
    }
}
