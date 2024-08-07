using MartenApp.Auth;
using MartenApp.LoggingMediatr;
using MartenApp.MartenExtensions;
using MartenApp.Middlewares;
using MartenApp.Repositories;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Serilog;
using MartenApp.Controllers;

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

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
	// id to match security definition with a scheme
	var apiSecurityName = "ApiKey";

	// Authorize template top right corner
	options.AddSecurityDefinition(apiSecurityName, new OpenApiSecurityScheme()
	{
		Description = "Api key security",
		Type = SecuritySchemeType.ApiKey,
		Name = AuthConfiguration.ApiKeyHeader,
		In = ParameterLocation.Header,
		Scheme = "ApiKeyScheme"
	});

	var scheme = new OpenApiSecurityScheme()
	{
		Reference = new OpenApiReference()
		{
			Id = apiSecurityName,
			Type = ReferenceType.SecurityScheme
		},
		In = ParameterLocation.Header
	};

	var requirement = new OpenApiSecurityRequirement()
	{
		{scheme, new List<string>()}
	};

	options.AddSecurityRequirement(requirement);
});
#endregion

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

services.AddSingleton(TimeProvider.System);


#region Localization TBD
//builder.Services.AddLocalization(options => options.ResourcesPath = "Resources")
//	.AddRequestLocalization(options =>
//	{
//		var supported = new[] { "en", "es" };
//		options.SetDefaultCulture(supported[0])
//				.AddSupportedCultures(supported)
//				.AddSupportedUICultures(supported);
//	});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
	var supportedCultures = new[] { "en-US", "es-ES", "de-DE" };
	options.SetDefaultCulture(supportedCultures[0])
		.AddSupportedCultures(supportedCultures)
		.AddSupportedUICultures(supportedCultures);

	options.FallBackToParentUICultures = true;

	// browser wont override query params
	//var headerProvider = options.RequestCultureProviders.OfType<AcceptLanguageHeaderRequestCultureProvider>().FirstOrDefault();
	//options.RequestCultureProviders.Remove(headerProvider);

});
#endregion

var app = builder.Build();

app.UseRequestLocalization();

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

//app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ETagMiddleware>();
app.UseMiddleware<ExMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthorization();

app.MapControllers();

app.Run();
