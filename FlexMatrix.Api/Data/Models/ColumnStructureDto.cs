using FlexMatrix.Api.Attributes.Validation;

namespace FlexMatrix.Api.Data.Models
{
    public class ColumnStructureDto
    {
        [SafeForSql]
        public string Name { get; set; }

        [SafeForSql]
        public string Type { get; set; }

        public bool IsNullable { get; set; }

        //[SafeForSql]
        //public object? DefaultValue { get; set; }
        public string? DefaultValue { get; set; }

        public int? Length { get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }



        public bool IsForeignKey { get; set; }

        [SafeForSql]
        public string TableSchema { get; set; } = "dbo";

        [SafeForSql]
        public string? ReferencesTableName { get; set; }

        [SafeForSql]
        public string? ReferencesTablePrimaryKeyName { get; set; } = "Id";

        [SafeForSql]
        public string? DeleteType {  get; set; }

        [SafeForSql]
        public string? UpdateType { get; set; }
    }
}
