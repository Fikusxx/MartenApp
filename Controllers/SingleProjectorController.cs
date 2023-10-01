using Microsoft.AspNetCore.Mvc;
using MartenApp.Repositories;
using MartenApp.Events;
using Marten;
using JasperFx.CodeGeneration.Frames;

namespace MartenApp.Controllers;

[ApiController]
[Route("single")]
public class SingleProjectorController : ControllerBase
{
	private readonly IOrderRepository orderRepo;
	private readonly IOrderSummaryRepository orderSummaryRepo;

	private readonly IDocumentStore store;
	private readonly IDocumentSession ctx;
	private readonly IQuerySession query;
	private readonly Guid orderId = Guid.Parse("8a2c17e9-d56f-47e7-ad52-369550fc0c6a");
	private readonly Guid userId = Guid.Parse("9b088d1a-97bc-467a-83fa-5e5c90cbc7cf");

	public SingleProjectorController(IDocumentSession ctx, IQuerySession query,
		IOrderRepository orderRepo, IOrderSummaryRepository orderSummaryRepo)
	{
		this.ctx = ctx;
		this.query = query;
		this.orderRepo = orderRepo;
		this.orderSummaryRepo = orderSummaryRepo;
	}

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> Get()
	{
		var data = ctx.Query<Order>().Where(x => x.Id == orderId).ToList();
		var order = await ctx.LoadAsync<Order>(orderId);
		var orderSummary = await ctx.LoadAsync<OrderSummary>(orderId);
		var userOrdersSummary = await ctx.LoadAsync<UserOrdersSummary>(userId);

		var orderSnapshot = await orderRepo.LoadAsync(orderId);
		var orderStream = await orderRepo.AggregateAsync(orderId, 1);
		var orderSummarySnapshot = await orderSummaryRepo.LoadAsync(orderId);


		var user1 = new User(Guid.NewGuid());
		var user2 = new User(Guid.NewGuid());
		var issue1 = new Issue(user1.Id, "Issue #1");
		var issue2 = new Issue(user2.Id, "Issue #2");
		var issue3 = new Issue(user2.Id, "Issue #3");

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

public record User(Guid Id);
public record Issue(Guid UserId, string Title);