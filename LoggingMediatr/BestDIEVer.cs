using MediatR;

namespace MartenApp.LoggingMediatr;

public static class BestDIEVer
{
	public static IServiceCollection RegisterBestServicesEverCreated(this IServiceCollection services)
	{
		services.AddMediatR(builder => builder.RegisterServicesFromAssemblyContaining(typeof(BestDIEVer)));
		services.AddScoped(typeof(IPipelineBehavior<,>), typeof(BestLoggingBehavior<,>));

		return services;
	}
}
