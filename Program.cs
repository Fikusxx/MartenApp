using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MartenApp.Registries;
using MartenApp.Events;
using Marten;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

services.AddMarten(options =>
{
	var connection = configuration.GetConnectionString("Marten");
	options.Connection(connection!);

	options.Events.AddEventType<OrderCreatedEvent>();
	options.Events.AddEventType<OrderUpdatedEvent>();
	options.Events.AddEventType<OrderCompletedEvent>();

	options.Schema.Include<OrderRegistry>();
	options.Schema.Include<OrderSummaryRegistry>();
	options.Schema.Include<UserOrdersSummaryRegistry>();

	options.Projections.Add<OrderSingleProjector>(lifecycle: ProjectionLifecycle.Inline);
	options.Projections.Add<OrderSummarySingleProjector>(lifecycle: ProjectionLifecycle.Inline);
	options.Projections.Add<UserOrdersSummaryProjector>(lifecycle: ProjectionLifecycle.Inline);
})
	.UseLightweightSessions()
	.AddAsyncDaemon(DaemonMode.Solo);

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
