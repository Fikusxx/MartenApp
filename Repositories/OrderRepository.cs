using MartenApp.Events;
using Marten;

namespace MartenApp.Repositories;


public interface IOrderRepository : IAggregateRootRepository<Order> { }

public class OrderRepository : AggregateRepository<Order>, IOrderRepository
{
	public OrderRepository(IDocumentSession ctx) : base(ctx)
	{ }
}

public abstract class AggregateRepository<T> : IAggregateRootRepository<T> where T: class, IAggregateRoot
{
	protected readonly IDocumentSession ctx;

	public AggregateRepository(IDocumentSession ctx)
	{
		this.ctx = ctx;
	}

	public async Task StoreAsync(T aggregate, CancellationToken ct = default)
	{
		await Task.CompletedTask;
	}

	public async Task<T?> AggregateAsync(Guid id, int? version = null, CancellationToken ct = default)
	{
		var aggregate = await ctx.Events.AggregateStreamAsync<T>(id, version ?? 0, token: ct);

		return aggregate;
	}

	public async Task<T?> LoadAsync(Guid id, CancellationToken ct = default) 
	{
		var snapshot = await ctx.LoadAsync<T>(id, ct);

		return snapshot;
	}
}
