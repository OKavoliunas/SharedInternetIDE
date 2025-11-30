namespace BlazorServerApp.Models
{
    public class Result<T> where T : class
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public string ErrorMessage { get; }

        private Result(T data, bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(data, true, null);
        }
        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(null, false, errorMessage);
        }
    }

}
