namespace FlexMatrix.Api.Data.Validators
{
    public interface IValidator<T>
    {
        void Validate(T instance);
    }
}
