using System.Collections.Generic;

namespace AtlasBank.Core.Application.Common;

/// <summary>
/// Standardized API response envelope for all endpoints
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Errors = null
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null, string traceId = "")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }

    public static ApiResponse<T> ErrorResponse(List<string> errors, string traceId = "")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = "Validation failed",
            Errors = errors,
            TraceId = traceId
        };
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public static ApiResponse SuccessResponse(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Errors = null
        };
    }

    public static ApiResponse ErrorResponse(string message, List<string>? errors = null, string traceId = "")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }

    public static ApiResponse ErrorResponse(List<string> errors, string traceId = "")
    {
        return new ApiResponse
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors,
            TraceId = traceId
        };
    }
}
