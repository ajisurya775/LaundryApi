using System.Collections.Generic;

namespace LaundrySaas.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(T data, string? message = null)
    {
        Success = true;
        Message = message;
        Data = data;
        Errors = null;
    }

    public ApiResponse(string message, List<string>? errors = null)
    {
        Success = false;
        Message = message;
        Data = default;
        Errors = errors ?? new List<string>();
    }
    
    public ApiResponse(string message, string error)
    {
        Success = false;
        Message = message;
        Data = default;
        Errors = new List<string> { error };
    }

    public static ApiResponse<T> CreateSuccess(T data, string? message = null) => new(data, message);
    public static ApiResponse<T> CreateError(string message, List<string>? errors = null) => new(message, errors);
    public static ApiResponse<T> CreateError(string message, string error) => new(message, error);
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse()
    {
    }

    public ApiResponse(object data, string? message = null) : base(data, message)
    {
    }

    public ApiResponse(string message, List<string>? errors = null) : base(message, errors)
    {
    }

    public ApiResponse(string message, string error) : base(message, error)
    {
    }

    public static ApiResponse CreateSuccess(string? message = null) => new(new object(), message);
    public static new ApiResponse CreateSuccess(object data, string? message = null) => new(data, message);
    public static new ApiResponse CreateError(string message, List<string>? errors = null) => new(message, errors);
    public static new ApiResponse CreateError(string message, string error) => new(message, error);
}
