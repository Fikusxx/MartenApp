using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Mime;

namespace MartenApp.Middlewares;

// add 
// services.AddExceptionHandler();
// app.UseExceptionHandler();
public sealed class ExHandler : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		// switch that ex

		var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
		var trace1 = Activity.Current?.Id;
		var trace2 = httpContext.TraceIdentifier;

		var details = new ExceptionDetails()
		{
			Message = exception.Message,
			Type = exception.GetType().Name,
			TraceId = traceId
		};

		// log that shit

		httpContext.Response.StatusCode = 400;
		httpContext.Response.ContentType = MediaTypeNames.Application.Json;
		var result = JsonConvert.SerializeObject(details);
		await httpContext.Response.WriteAsync(result, cancellationToken);

		return true;

		#region Or

		await Results.Problem(
			title: "Some really bad things happened :(",
			statusCode: StatusCodes.Status500InternalServerError,
			detail: "OMG RUN",
			extensions: new Dictionary<string, object?>
			{
				{"traceId", traceId}
			}
			).ExecuteAsync(httpContext);

		return true;

		#endregion
	}
}

public class ExceptionDetails
{
	public string Type { get; set; }
	public string Message { get; set; }
	public string TraceId { get; set; }
}
