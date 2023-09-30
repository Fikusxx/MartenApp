using MartenApp.Events;
using Marten;

namespace MartenApp.Repositories;


public interface IOrderSummaryRepository : IReadModelRepository<OrderSummary> { }

public class OrderSummaryRepository : ReadModelRepository<OrderSummary>, IOrderSummaryRepository
{
	public OrderSummaryRepository(IQuerySession ctx) : base(ctx)
	{ }
}


public abstract class ReadModelRepository<T> : IReadModelRepository<T> where T : class, IReadModel
{
	protected readonly IQuerySession ctx;

	public ReadModelRepository(IQuerySession ctx)
	{
		this.ctx = ctx;
	}

	public async Task<T?> LoadAsync(Guid id, CancellationToken ct = default)
	{
		var snapshot = await ctx.LoadAsync<T>(id, ct);

		return snapshot;
	}
}