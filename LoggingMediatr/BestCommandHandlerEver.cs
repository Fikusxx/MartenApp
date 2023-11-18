using MediatR;

namespace MartenApp.LoggingMediatr;

internal sealed class BestCommandHandlerEver : IRequestHandler<BestCommandEver>
{
	private readonly ILogger<BestCommandHandlerEver> logger;

	public BestCommandHandlerEver(ILogger<BestCommandHandlerEver> logger)
	{
		this.logger = logger;
	}

	public Task Handle(BestCommandEver request, CancellationToken cancellationToken)
	{
		//logger.LogWarning("Holy shit this is awesome : {Subject}", "Yeah, I guess");
		//var json = new JsonTest() { Name = "Fikus", Age = 25 };
		//logger.LogWarning("Regular template : {Me}", json);
		//logger.LogWarning("Json template : {@Me}", json);

		//return Task.FromResult(Unit.Value);
		//return Task.FromResult(777);
		return Task.CompletedTask;
	}
}

public class JsonTest
{
	public required string Name { get; set; }
	public int Age { get; set; }
}