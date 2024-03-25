using FlexMatrix.Api.Consts;
using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FlexMatrix.Api.Data.Parser.Strategies
{
    public class ToDbParser : IParserStrategy
    {
        public string StrategyName => ParseStrategies.ToDb;



        public object? Parse(string valueType, object? value)
        {
            if (value == null) return DBNull.Value;

            var cultureInfo = CultureInfo.InvariantCulture;

            switch (valueType.ToUpper())
            {
                case SqlTypes.Int:
                case SqlTypes.SmallInt:
                case SqlTypes.TinyInt:
                case SqlTypes.BigInt:
                    return Convert.ToInt64(value, cultureInfo);
                case SqlTypes.Float:
                case SqlTypes.Real:
                    return Convert.ToDouble(value, cultureInfo);
                case SqlTypes.Decimal:
                case SqlTypes.Numeric:
                case SqlTypes.Money:
                case SqlTypes.SmallMoney:
                    return Convert.ToDecimal(value, cultureInfo);
                case SqlTypes.Bit:
                    return Convert.ToBoolean(value, cultureInfo);
                case SqlTypes.DateTime:
                case SqlTypes.Date:
                case SqlTypes.DateTime2:
                case SqlTypes.SmallDateTime:
                    return DateTime.Parse(value.ToString(), cultureInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                    //return Convert.ToDateTime(value, cultureInfo);
                case SqlTypes.Time:
                    return TimeSpan.Parse(value.ToString(), cultureInfo);
                case SqlTypes.UniqueIdentifier:
                    return Guid.Parse(value.ToString());
                case SqlTypes.Char:
                case SqlTypes.VarChar:
                case SqlTypes.Text:
                case SqlTypes.NChar:
                case SqlTypes.NVarChar:
                case SqlTypes.NText:
                    return value.ToString();
                case SqlTypes.VarBinary:
                case SqlTypes.Image:
                    return Convert.FromBase64String(value.ToString());
                    //return value is byte[] byteArrayValue ? byteArrayValue : null;
                case SqlTypes.DateTimeOffset:
                    return DateTimeOffset.Parse(value.ToString(), cultureInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                case SqlTypes.Xml:
                case SqlTypes.Json:
                    return value.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "This type is not supported.");
            }
        }
    }
}
