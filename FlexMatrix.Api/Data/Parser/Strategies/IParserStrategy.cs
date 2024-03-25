namespace FlexMatrix.Api.Data.Parser.Strategies
{
    public interface IParserStrategy
    {
        string StrategyName { get; }

        object? Parse(string valueType, object? value);
    }
}
