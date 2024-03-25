using FlexMatrix.Api.Consts;
using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FlexMatrix.Api.Data.Parser.Strategies
{
    public class ToDbParser : IParserStrategy
    {
        public string StrategyName { get; set; } = ParseStrategies.ToDb;



        public object? Parse(string valueType, object? value)
        {
            if (value == null) return DBNull.Value;

            var cultureInfo = CultureInfo.InvariantCulture;

            switch (valueType)
            {
                case ParseTypes.Int:
                    return Convert.ToInt32(value, cultureInfo);
                case ParseTypes.Long:
                    return Convert.ToInt64(value, cultureInfo);
                case ParseTypes.String:
                    return value.ToString();
                case ParseTypes.Double:
                    return Convert.ToDouble(value, cultureInfo);
                case ParseTypes.Decimal:
                    return Convert.ToDecimal(value, cultureInfo);
                case ParseTypes.Boolean:
                    return Convert.ToBoolean(value, cultureInfo);
                case ParseTypes.DateTime:
                    return DateTime.Parse(value.ToString(), cultureInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                case ParseTypes.Guid:
                    return Guid.Parse(value.ToString());
                case ParseTypes.Byte:
                    return Convert.ToByte(value, cultureInfo);
                case ParseTypes.ByteArray:
                    return Convert.FromBase64String(value.ToString());
                    //return value is byte[] byteArrayValue ? byteArrayValue : DBNull.Value;
                case ParseTypes.TimeSpan:
                    return TimeSpan.Parse(value.ToString(), cultureInfo);
                case ParseTypes.Char:
                    return Convert.ToChar(value, cultureInfo);
                case ParseTypes.Enum:
                    return Convert.ToInt32(value, cultureInfo);
                    //return Enum.Parse(typeof(YourEnumTypeHere), value.ToString());
                case ParseTypes.DateTimeOffset:
                    return DateTimeOffset.Parse(value.ToString(), cultureInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                case ParseTypes.Uri:
                    return Uri.TryCreate(value.ToString(), UriKind.Absolute, out Uri uriValue) ? uriValue : DBNull.Value;
                case ParseTypes.Json:
                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                    return JsonSerializer.Serialize(value, options);
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "This type is not supported.");
            }
        }
    }
}
