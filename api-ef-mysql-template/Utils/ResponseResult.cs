using Microsoft.AspNetCore.Mvc;
using System;

public class ResponseResult : ObjectResult
{
    public ResponseResult(int statusCode, object value)
        : base(new ResponseModel { Status = statusCode, Data = value })
    {
        StatusCode = statusCode;
    }

    public static ResponseResult Success(int statusCode, object data)
    {
        return new ResponseResult(statusCode, data);
    }
    public static ResponseResult Success(object data)
    {
        return new ResponseResult(200, data);
    }

    public static ResponseResult Error(int statusCode, string errorMessage)
    {
        return new ResponseResult(statusCode, new ErrorResponse { Message = errorMessage });
    }

    public static ResponseResult Error(string errorMessage)
    {
        return new ResponseResult(500, new ErrorResponse { Message = errorMessage });
    }

    public static ResponseResult BadRequest(string errorMessage)
    {
        return Error(400, errorMessage);
    }

    public static ResponseResult Unauthorized(string errorMessage)
    {
        return Error(401, errorMessage);
    }

    public static ResponseResult NotFound(string errorMessage)
    {
        return Error(404, errorMessage);
    }
}

public class ResponseModel
{
    public int Status { get; set; }
    public object Data { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; }
}
