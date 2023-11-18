using MediatR;

namespace MartenApp.LoggingMediatr;

public sealed class BestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	//where TRequest : IRequest<TResponse>
{
	private readonly ILogger<BestLoggingBehavior<TRequest, TResponse>> logger;

	public BestLoggingBehavior(ILogger<BestLoggingBehavior<TRequest, TResponse>> logger)
	{
		this.logger = logger;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		logger.LogWarning("Starting request {@RequestName}, {@DateTime}",
			typeof(TRequest).Name, DateTimeOffset.UtcNow);

		//logger.LogWarning("Test count {Count}", 15);

		var result = await next();

		logger.LogWarning("Completed request {@RequestName}, {@DateTime}",
			typeof(TRequest).Name, DateTimeOffset.UtcNow);

		//logger.LogWarning("Test count {Count}", "privet"); // error

		return result;
	}
}
