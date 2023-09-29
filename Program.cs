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

	options.Schema.For<Order>().Identity(x => x.Id);
	options.Schema.For<OrderSummary>().Identity(x => x.OrderId);
	options.Schema.For<UserOrdersSummary>().Identity(x => x.UserId);

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
