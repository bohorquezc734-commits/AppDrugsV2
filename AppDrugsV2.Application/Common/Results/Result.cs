namespace AppDrugsV2.Application.Common.Results
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }  // ← Agregar ? para aceptar null
        public string? Error { get; }  // ← Agregar ? para aceptar null
        public bool IsFailure => !IsSuccess;

        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default, error);
        }
    }
}