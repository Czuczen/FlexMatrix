namespace FlexMatrix.Api.Data.Models
{
    public class ColumnStructureDto
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsForeignKey { get; set; }

        public bool ReferencesTableName { get; set; }
    }
}
