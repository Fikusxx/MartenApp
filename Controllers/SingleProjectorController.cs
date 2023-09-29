using Microsoft.AspNetCore.Mvc;
using MartenApp.Events;
using Marten;

namespace MartenApp.Controllers;

[ApiController]
[Route("single")]
public class SingleProjectorController : ControllerBase
{
	private readonly IDocumentSession ctx;
	private readonly IQuerySession query;
	private readonly Guid orderId = Guid.Parse("8a2c17e9-d56f-47e7-ad52-369550fc0c6a");
	private readonly Guid userId = Guid.Parse("9b088d1a-97bc-467a-83fa-5e5c90cbc7cf");

	public SingleProjectorController(IDocumentSession ctx, IQuerySession query)
	{
		this.ctx = ctx;
		this.query = query;
	}

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> Get()
	{
		var order = await ctx.LoadAsync<Order>(orderId);
		var orderSummary = await ctx.LoadAsync<OrderSummary>(orderId);
		var userOrdersSummary = await ctx.LoadAsync<UserOrdersSummary>(userId);
		//var data = await ctx.Events.QueryAllRawEvents().AggregateToAsync<List<Order>>();
		//var data = await ctx.Events.FetchStreamAsync(id);
		//var data = ctx.Events.LoadAsync(id);
		//var data = ctx.Events.QueryAllRawEvents()
		//	.Where(x => x.StreamId == id)
		//	.Where(x => x.DotNetTypeName.Contains(nameof(OrderCreatedEvent)))
		//	.ToList();
		//var data = await ctx.Events.AggregateStreamAsync<Order>(id, version: 2);
		//var data = await ctx.Events.AggregateStreamAsync<Order>(id, timestamp: DateTime.UtcNow.AddDays(-1));

		return Ok(order);
	}

	[HttpGet]
	[Route("daemon")]
	public async Task<IActionResult> Daemon([FromServices] IDocumentStore store)
	{
		var daemon = await store.BuildProjectionDaemonAsync();
		await daemon.RebuildProjection<OrderSingleProjector>(CancellationToken.None);
		 
		return Ok();
	}
}
