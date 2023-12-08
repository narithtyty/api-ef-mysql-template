using EntityFrameworkCore.MySQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiActionFilter : ActionFilterAttribute
{
    private readonly AppDbContext _appDbContext;

    public ApiActionFilter(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValue))
        {
            string token = authorizationHeaderValue.ToString();

            if (string.IsNullOrEmpty(token))
            {
                var customUnauthorizedResult = new ObjectResult(new
                {
                    Status = 401,
                    Message = "Unauthorized",
                })
                {
                    StatusCode = 401,
                };

                context.Result = customUnauthorizedResult;
                return;
                
            }
        }
        else
        {
            // Authorization header not found, return Unauthorized response
            var customUnauthorizedResult = new ObjectResult(new
            {
                Status = 401,
                Message = "Unauthorized",
            })
            {
                StatusCode = 401,
            };

            context.Result = customUnauthorizedResult;
            return;
        }

        // Continue with the action execution.
        base.OnActionExecuting(context);
    }
}