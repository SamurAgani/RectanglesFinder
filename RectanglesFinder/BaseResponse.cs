namespace RectanglesFinder
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public bool IsSuccessful { get; set; }
        public List<string> Errors { get; set; }
        public int StatusCode { get; set; }

        public static BaseResponse<T> Success(T data, int statusCode = 200)
        {
            BaseResponse<T> response = new BaseResponse<T>();
            response.Data = data;
            response.IsSuccessful = true;
            response.StatusCode = statusCode;
            return response;
        }

        public static BaseResponse<T> Fail(T data, List<string> error, int statusCode = 500)
        {
            BaseResponse<T> response = new BaseResponse<T>();
            response.Data = data;
            response.IsSuccessful = false;
            response.Errors = error;
            response.StatusCode = statusCode;
            return response;
        }
        public static BaseResponse<T> Fail(T data, string error, int statusCode = 500)
        {
            BaseResponse<T> response = new BaseResponse<T>();
            response.Data = data;
            response.IsSuccessful = false;
            response.Errors = [error];
            response.StatusCode = statusCode;
            return response;
        }
    }
}
