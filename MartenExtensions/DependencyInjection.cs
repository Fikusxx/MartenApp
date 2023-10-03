using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MartenApp.Interceptors;
using MartenApp.Controllers;
using MartenApp.Registries;
using Marten.Events.Daemon;
using Marten.Exceptions;
using MartenApp.Events;
using JasperFx.Core;
using Marten;
using Npgsql;

namespace MartenApp.MartenExtensions;

public static class DependencyInjection
{
	public static IServiceCollection RegisterMarten(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMarten(options =>
		{
			var connection = configuration.GetConnectionString("Marten");
			options.Connection(connection!);

			options.RetryPolicy(DefaultRetryPolicy.Times(3));

			#region Projection exception retry policies
			options.Projections.OnException<EventFetcherException>()
				.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
				.Then.Pause(30.Seconds());

			options.Projections.OnException<ShardStopException>().DoNothing();

			options.Projections.OnException<ShardStartException>()
				.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
				.Then.DoNothing();

			options.Projections.OnException<NpgsqlException>()
				.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
				.Then.Pause(30.Seconds());

			options.Projections.OnException<MartenCommandException>()
				.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
				.Then.Pause(30.Seconds());

			options.Projections.OnException<ProgressionProgressOutOfOrderException>().Pause(10.Seconds());
			#endregion
		})
			.UseLightweightSessions()
			//.OptimizeArtifactWorkflow()
			.ApplyAllDatabaseChangesOnStartup()
			.AddAsyncDaemon(DaemonMode.HotCold);


		services.AddOrderModule();

		return services;
	}

	private static IServiceCollection AddOrderModule(this IServiceCollection services)
	{
		services.ConfigureMarten(options =>
		{
			options.Events.AddEventType<OrderCreatedEvent>();
			options.Events.AddEventType<OrderUpdatedEvent>();
			options.Events.AddEventType<OrderCompletedEvent>();

			options.RegisterDocumentType<Order>();
			options.RegisterDocumentType<OrderSummary>();
			options.RegisterDocumentType<UserOrdersSummary>();

			options.RegisterDocumentType<MessageOutbox>();

			options.Schema.Include<OrderRegistry>();
			options.Schema.Include<OrderSummaryRegistry>();
			options.Schema.Include<UserOrdersSummaryRegistry>();

			options.Schema.For<BaseClass>()
				.AddSubClass<FirstSubClass>()
				.AddSubClass<SecondSubClass>();

			options.RetryPolicy();

			options.Listeners.Add(new Interceptor());

			options.Projections.Add<OrderSingleProjector>(lifecycle: ProjectionLifecycle.Inline);
			options.Projections.Add<OrderSummarySingleProjector>(lifecycle: ProjectionLifecycle.Async);
			options.Projections.Add<UserOrdersSummaryProjector>(lifecycle: ProjectionLifecycle.Async);
		});

		return services;
	}
}
