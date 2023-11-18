using Microsoft.AspNetCore.Mvc;
using MartenApp.Events;
using Newtonsoft.Json;
using Marten;

namespace MartenApp.Controllers;

[ApiController]
[Route("self")]
public class SelfController : ControllerBase
{
	private readonly IDocumentSession ctx;
	private readonly Guid orderId = Guid.Parse("8a2c17e9-d56f-47e7-ad52-369550fc0c6a");
	//private readonly Guid orderId = Guid.Parse("8a3c17e9-d56f-47e7-ad52-369550fc0c6a");
	private readonly Guid userId = Guid.Parse("9b088d1a-97bc-467a-83fa-5e5c90cbc7cf");

	private readonly ILogger<SelfController> logger;

	public SelfController(IDocumentSession ctx, ILogger<SelfController> logger)
	{
		this.ctx = ctx;
		this.logger = logger;
	}



	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> Get()
	{
		logger.LogError(new Exception("Something Hanppened"), "KEKW");
		//var data = await ctx.Events.AggregateStreamAsync<Order>(orderId);
		var data = await ctx.Events.AggregateStreamAsync<OrderSummary>(orderId);
		//var data = await ctx.Events.AggregateStreamAsync<UserOrdersSummary>(orderId);

		return Ok(data);
	}

	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> Create()
	{
		var @event = new OrderCreatedEvent(orderId, userId, "Start", 10);
		//var @event = new OrderCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Start", 10);
		ctx.Events.StartStream(orderId, @event);
		//ctx.Events.StartStream(@event.OrderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}

	[HttpGet]
	[Route("update")]
	public async Task<IActionResult> Update()
	{
		var @event = new OrderUpdatedEvent(orderId, userId, 5);
		await ctx.Events.AppendOptimistic(orderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}

	[HttpGet]
	[Route("complete")]
	public async Task<IActionResult> Complete()
	{
		var @event = new OrderCompletedEvent(orderId, userId, "Completed");
		await ctx.Events.AppendOptimistic(orderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}

	[HttpGet]
	[Route("get-domain-events")]
	public async Task<IActionResult> GetDomainEvents()
	{
		// imagine this is a back task or smth
		var events = ctx.Query<MessageOutbox>().Take(10).ToList();

		// or just while(true) effectively with "await Task.Delay(ms, ct);"
		if (events.Count <= 0)
			await Task.CompletedTask;

		var converted = new List<IDomainEvent>();

		var options = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
		events.ForEach(x =>
		{
			var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(x.Content, options);
			converted.Add(domainEvent);
		});

		// publish events
		// IMPORTANT
		// before publishing - specify property/headers messageId = outbox.Id
		// for potential idempotency check on consumer side

		//ctx.DeleteObjects(events);
		//await ctx.SaveChangesAsync();

		return Ok(converted);
	}
}
