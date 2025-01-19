namespace Order.API.Extension
{
    public sealed class Pagination
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public sealed class ApiResponse<T>
    {
        public ApiResponse(int statusCode, string message, T data, double executionTime = 0, Pagination? pagination = null)
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Data = data;
            this.TimeStamp = DateTime.UtcNow;
            this.ExcecutionTime = executionTime;
            this.Pagination = pagination;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public DateTime TimeStamp { get; set; }
        public double ExcecutionTime { get; set; }
        public Pagination? Pagination { get; set; }
    }


    public static class ApiResponseHelper
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Success", double executionTime = 0, Pagination pagination = null)
        {
            return new ApiResponse<T>(200, message, data, executionTime, pagination);
        }

        public static ApiResponse<T> Error<T>(string message, int statusCode = 500, double executionTime = 0)
        {
            return new ApiResponse<T>(statusCode, message, default, executionTime);
        }
    }
}
