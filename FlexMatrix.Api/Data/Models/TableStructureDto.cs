namespace FlexMatrix.Api.Data.Models
{
    public class TableStructureDto
    {
        public string TableName { get; set; }

        public string PrimaryKeyType { get; set; }

        public IEnumerable<ColumnStructureDto> Columns { get; set; }
    }
}
