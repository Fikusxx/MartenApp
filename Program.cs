using MartenApp.LoggingMediatr;
using MartenApp.MartenExtensions;
using MartenApp.Middlewares;
using MartenApp.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;


builder.Host.UseSerilog((ctx, cfg) =>
{
	// take settings from app settings
	//cfg.ReadFrom.Configuration(ctx.Configuration);

	var cs = "host=localhost;port=5432;database=postgres;password=wc3alive;username=postgres";
	cfg.WriteTo.PostgreSQL(connectionString: cs, tableName: "Logs", needAutoCreateTable: true)
			.MinimumLevel.Warning();
});

builder.Services.RegisterBestServicesEverCreated();

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

//services.AddProblemDetails();
//services.AddExceptionHandler<ExHandler>();

var app = builder.Build();

app.UseCors(builder =>
		 builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseExceptionHandler();
app.UseMiddleware<ETagMiddleware>();
app.UseMiddleware<ExMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthorization();

app.MapControllers();

app.Run();
