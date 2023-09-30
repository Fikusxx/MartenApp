using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MartenApp.Registries;
using MartenApp.Events;
using Marten;
using JasperFx.Core;
using Marten.Events.Daemon;
using Marten.Exceptions;
using Npgsql;
using MartenApp.MartenExtensions;
using MartenApp.Repositories;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Default Marten registration
//services.AddMarten(options =>
//{
//	var connection = configuration.GetConnectionString("Marten");
//	options.Connection(connection!);

//	options.Events.AddEventType<OrderCreatedEvent>();
//	options.Events.AddEventType<OrderUpdatedEvent>();
//	options.Events.AddEventType<OrderCompletedEvent>();

//	options.Schema.Include<OrderRegistry>();
//	options.Schema.Include<OrderSummaryRegistry>();
//	options.Schema.Include<UserOrdersSummaryRegistry>();

//	options.Projections.Add<OrderSingleProjector>(lifecycle: ProjectionLifecycle.Inline);
//	options.Projections.Add<OrderSummarySingleProjector>(lifecycle: ProjectionLifecycle.Inline);
//	options.Projections.Add<UserOrdersSummaryProjector>(lifecycle: ProjectionLifecycle.Inline);

//	options.Projections.OnException<EventFetcherException>()
//		.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
//		.Then.Pause(30.Seconds());

//	options.Projections.OnException<ShardStopException>().DoNothing();

//	options.Projections.OnException<ShardStartException>()
//		.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
//		.Then.DoNothing();

//	options.Projections.OnException<NpgsqlException>()
//		.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
//		.Then.Pause(30.Seconds());

//	options.Projections.OnException<MartenCommandException>()
//		.RetryLater(250.Milliseconds(), 500.Milliseconds(), 1.Seconds())
//		.Then.Pause(30.Seconds());

//	options.Projections.OnException<ProgressionProgressOutOfOrderException>().Pause(10.Seconds());
//})
//	.UseLightweightSessions()
//	.AddAsyncDaemon(DaemonMode.HotCold);
#endregion

services.RegisterMarten(configuration);

services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IOrderSummaryRepository, OrderSummaryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
