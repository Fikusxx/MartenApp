using Newtonsoft.Json;
using System.Net.Mime;

namespace MartenApp.Auth;

public sealed class ApiKeyMiddleware
{
	private readonly RequestDelegate next;
	private readonly IConfiguration cfg; // replace with IOptions<T> if needed

	public ApiKeyMiddleware(RequestDelegate next, IConfiguration cfg)
	{
		this.next = next;
		this.cfg = cfg;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var providedKey = context.Request.Headers[AuthConfiguration.ApiKeyHeader].FirstOrDefault();

		var isValid = IsValidApiKey(providedKey);

		if (isValid == false)
		{
			context.Response.ContentType = MediaTypeNames.Application.Json;
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			var message = JsonConvert.SerializeObject(new { Message = "poshel nahui noname" });
			await context.Response.WriteAsync(message);
			return; // intercept the request
		}

		await next.Invoke(context);
	}

	private bool IsValidApiKey(string? providedApiKey)
	{
		if (string.IsNullOrWhiteSpace(providedApiKey))
			return false;

		var validApiKey = cfg.GetValue<string>(AuthConfiguration.AuthSection);

		return string.Equals(validApiKey, providedApiKey, StringComparison.OrdinalIgnoreCase);
	}
}
