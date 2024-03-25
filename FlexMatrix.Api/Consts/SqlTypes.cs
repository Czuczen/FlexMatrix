namespace FlexMatrix.Api.Consts
{
    public static class SqlTypes
    {
        // Numeric types
        public const string Int = "INT";
        public const string SmallInt = "SMALLINT";
        public const string TinyInt = "TINYINT";
        public const string BigInt = "BIGINT";
        public const string Decimal = "DECIMAL";
        public const string Numeric = "NUMERIC";
        public const string Real = "REAL";
        public const string Float = "FLOAT";
        public const string Bit = "BIT";
        public const string Money = "MONEY";
        public const string SmallMoney = "SMALLMONEY";

        // Date and time types
        public const string Date = "DATE";
        public const string Time = "TIME";
        public const string DateTime = "DATETIME";
        public const string DateTime2 = "DATETIME2";
        public const string SmallDateTime = "SMALLDATETIME";
        public const string DateTimeOffset = "DATETIMEOFFSET";
        public const string TimeStamp = "TIMESTAMP";

        // Character strings
        public const string Char = "CHAR";
        public const string VarChar = "VARCHAR";
        public const string Text = "TEXT";

        // Unicode character strings
        public const string NChar = "NCHAR";
        public const string NVarChar = "NVARCHAR";
        public const string NText = "NTEXT";

        // Binary strings
        public const string Binary = "BINARY";
        public const string VarBinary = "VARBINARY";
        public const string Image = "IMAGE";

        // Other data types
        public const string UniqueIdentifier = "UNIQUEIDENTIFIER"; // GUID
        public const string Xml = "XML";
        public const string Json = "JSON";
        public const string SqlVariant = "SQL_VARIANT";

        // Spatial Data Types
        public const string Geography = "GEOGRAPHY";
        public const string Geometry = "GEOMETRY";

        // No direct equivalent in SQL for Enum, Uri - usually represented as VARCHAR or INT
        public const string Enum = "INT";
        public const string Uri = "VARCHAR";
    }
}
