namespace FlexMatrix.Api.Data.Models
{
    public class ColumnStructureDto
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsNullable { get; set; }

        public string DefaultValue { get; set; }

        public int? Length { get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }

        public bool IsForeignKey { get; set; }

        public string? ReferencesTableName { get; set; }

        public string? DeleteType {  get; set; }

        public string? UpdateType { get; set; }
    }
}
