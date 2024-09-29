namespace Manager.WebApi
{
    /// <summary>
    /// 返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 数据主体
        /// </summary>
        public T Data { get; set; }

        public new static ApiResult<T> Error(string message)
        {
            return new ApiResult<T>
            {
                Success = false,
                Message = message,
                Code = 500
            };
        }

        public new static ApiResult<T> NotFound(string message = null)
        {
            return new ApiResult<T>
            {
                Code = 404,
                Success = false,
                Message = message ?? "not found"
            };
        }

        public new static ApiResult<T> NoContent(string message = null)
        {
            return new ApiResult<T>
            {
                Code = 204,
                Message = message ?? "no content",
                Success = true,
            };
        }
    }

    public class ApiResult
    {
        /// <summary>
        /// 状态值
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 请求是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        public static ApiResult Ok()
        {
            return new ApiResult
            {
                Code = 200,
                Success = true
            };
        }

        public static ApiResult<T> Ok<T>(T data)
        {
            return new ApiResult<T>
            {
                Code = 200,
                Success = true,
                Data = data
            };
        }

        public static ApiResult NoContent(string message = null)
        {
            return new ApiResult
            {
                Code = 204,
                Message = message ?? "no content",
                Success = true
            };
        }

        public static ApiResult NotFound(string message = null)
        {
            return new ApiResult
            {
                Code = 404,
                Success = false,
                Message = message ?? "not found"
            };
        }

        public static ApiResult Error(string message)
        {
            return new ApiResult
            {
                Code = 500,
                Success = false,
                Message = message
            };
        }
    }
}
