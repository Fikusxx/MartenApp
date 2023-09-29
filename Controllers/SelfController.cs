using Microsoft.AspNetCore.Mvc;
using MartenApp.Events;
using Marten;

namespace MartenApp.Controllers;

[ApiController]
[Route("self")]
public class SelfController : ControllerBase
{
	private readonly IDocumentSession ctx;
	private readonly Guid orderId = Guid.Parse("8a2c17e9-d56f-47e7-ad52-369550fc0c6a");
	private readonly Guid userId = Guid.Parse("9b088d1a-97bc-467a-83fa-5e5c90cbc7cf");

	public SelfController(IDocumentSession ctx)
	{
		this.ctx = ctx;
	}

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> Get()
	{
		var data = await ctx.Events.AggregateStreamAsync<Order>(orderId);

		return Ok(data);
	}

	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> Create()
	{
		var @event = new OrderCreatedEvent(Guid.NewGuid(), orderId, userId, "Start", 10);
		ctx.Events.StartStream(orderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}

	[HttpGet]
	[Route("update")]
	public async Task<IActionResult> Update()
	{
		var @event = new OrderUpdatedEvent(Guid.NewGuid(), orderId, userId, 5);
		await ctx.Events.AppendOptimistic(orderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}

	[HttpGet]
	[Route("complete")]
	public async Task<IActionResult> Complete()
	{
		var @event = new OrderCompletedEvent(Guid.NewGuid(), orderId, userId, "Completed");
		await ctx.Events.AppendOptimistic(orderId, @event);
		await ctx.SaveChangesAsync();

		return Ok();
	}
}
