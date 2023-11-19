using Newtonsoft.Json;
using System.Net.Mime;
using System.Net;

namespace MartenApp.Middlewares;

public sealed class ExMiddleware
{
	private readonly RequestDelegate next;

	public ExMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{

			await HandleExceptionAsync(context, ex);
		}
	}

	private Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = MediaTypeNames.Application.Json;
		HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
		var traceId = context.TraceIdentifier;

		var exDetails = new ExceptionDetails()
		{
			Message = exception.Message,
			Type = exception.GetType().Name,
			TraceId = traceId
		};

		string result = JsonConvert.SerializeObject(exDetails);

		switch (exception)
		{
			default:
				statusCode = HttpStatusCode.BadRequest;
				break;
		}

		context.Response.StatusCode = (int)statusCode;

		return context.Response.WriteAsync(result);
	}
}
