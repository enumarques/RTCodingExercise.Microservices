namespace Catalog.API.Models
{
    public class Result<T>
    {
        public readonly bool IsSuccess;
        public readonly T? Value;
        public readonly Exception? Exception;

        public Result(T value)
        {
            IsSuccess = value == null? false : true;
            Value = value;
        }

        public Result(Exception exception)
        {
            IsSuccess = false;
            Exception = exception;
        }
    }
}