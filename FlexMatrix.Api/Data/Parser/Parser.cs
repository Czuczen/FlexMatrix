using FlexMatrix.Api.Data.Parser.Strategies;
using System;
using System.Reflection;
using System.Text.Json;

namespace FlexMatrix.Api.Data.Parser
{
    public class Parser : IParser
    {
        private readonly IEnumerable<IParserStrategy> _parseStrategies;

        public Parser(IEnumerable<IParserStrategy> parseStrategies)
        {
            _parseStrategies = parseStrategies;
        }

        public object? Parse(string strategyName, string valueType, object value)
        {
            var parsedValue = _parseStrategies.Single(s => s.StrategyName == strategyName).Parse(valueType, value);

            return parsedValue;
        }

        public static T ConvertFromJsonElement<T>(JsonElement element)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
