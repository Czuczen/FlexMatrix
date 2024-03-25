namespace FlexMatrix.Api.Data.Parser
{
    public interface IParser
    {
        object? Parse(string strategyName, string valueType, object value);
    }
}