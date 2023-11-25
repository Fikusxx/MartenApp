using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MartenApp.Auth;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		if (IsValidApiKey(context) == false)
		{
			var message = new { Message = "idi nahui plz" };
			context.Result = new UnauthorizedObjectResult(message);
		}

		return Task.CompletedTask;
	}

	private static bool IsValidApiKey(AuthorizationFilterContext ctx)
	{
		var providedKey = ctx.HttpContext.Request.Headers[AuthConfiguration.ApiKeyHeader];

		if (string.IsNullOrWhiteSpace(providedKey))
			return false;

		var cfg = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
		var actualKey = cfg.GetValue<string>(AuthConfiguration.AuthSection);

		return string.Equals(providedKey, actualKey, StringComparison.OrdinalIgnoreCase);
	}
}
