﻿using FlexMatrix.Api.Data.Parser.Strategies;
using System;
using System.Reflection;

namespace FlexMatrix.Api.Data.Parser
{
    public class Parser : IParser
    {
        private readonly IEnumerable<IParserStrategy> _parseStrategies;

        public Parser(IEnumerable<IParserStrategy> parseStrategies)
        {
            _parseStrategies = parseStrategies;
        }

        public object Parse(string strategyName, string valueType, object value)
        {
            _parseStrategies.Single(s => s.StrategyName == strategyName).Parse(valueType, value);

            return value;
        }
    }
}
