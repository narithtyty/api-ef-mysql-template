using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ApiActionFilter : ActionFilterAttribute
{
    public ScopeEnum Scope { get; set; }
    public string Role { get; set; }

    public ApiActionFilter(ScopeEnum Scope = ScopeEnum.None, string Role = null)
    {
        this.Scope = Scope;
        this.Role = Role;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Retrieve the TokenService from HttpContext.RequestServices
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<TokenService>();
       
        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValue))
        {
            string token = authorizationHeaderValue.ToString().Replace("Bearer ","");

            if (string.IsNullOrEmpty(token))
            {
                SetUnauthorizedResult(context);
                return;
            }

            try
            {
                // Validate and decode the JWT token
                var claimsPrincipal = tokenService.ValidateToken(token);

                // Check expiration
                if (claimsPrincipal == null || claimsPrincipal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Exp && DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(long.Parse(c.Value))))
                {
                    SetUnauthorizedResult(context, "Token is expired.");
                    return;
                }

                // Check scope
                if (Scope != ScopeEnum.None && !claimsPrincipal.HasClaim(c => c.Type == "scope" && Enum.GetName(typeof(ScopeEnum), Scope) == c.Value))
                {
                    SetUnauthorizedResult(context, "Insufficient scope.");
                    return;
                }

                // Check role 
                if (!string.IsNullOrEmpty(Role) && !claimsPrincipal.IsInRole(Role))
                {
                    SetUnauthorizedResult(context, "Insufficient role.");
                    return;
                }
            }
            catch (SecurityTokenException)
            {
                SetUnauthorizedResult(context, "Invalid token.");
                return;
            }
        }
        else
        {
            SetUnauthorizedResult(context);
            return;
        }

        // Continue with the action execution.
        base.OnActionExecuting(context);
    }

    private void SetUnauthorizedResult(ActionExecutingContext context, string message = "Unauthorized")
    {
        var customUnauthorizedResult = new ObjectResult(new
        {
            Status = 401,
            Message = message,
        })
        {
            StatusCode = 401,
        };

        context.Result = customUnauthorizedResult;
    }
}

public enum ScopeEnum
{
    None,
    API,
    Internal,
    External
}
