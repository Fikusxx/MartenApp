using Marten.Events.Projections;

namespace MartenApp.Events;

public class UserOrdersSummaryProjector : MultiStreamProjection<UserOrdersSummary, Guid>
{
    public UserOrdersSummaryProjector()
    {
        Identity<IHasUserId>(x => x.UserId);

		IncludeType<OrderCreatedEvent>();
		IncludeType<OrderUpdatedEvent>();
		IncludeType<OrderCompletedEvent>();
	}

    public void Apply(UserOrdersSummary snapshot, OrderCreatedEvent e)
    {
		snapshot.Qty += e.Qty;
		snapshot.OrderIds.Add(e.OrderId);
    }

	public void Apply(UserOrdersSummary snapshot, OrderUpdatedEvent e)
	{
		snapshot.Qty += e.Qty;
	}

	public void Apply(UserOrdersSummary snapshot, OrderCompletedEvent e)
	{
        Console.WriteLine("Completed :)");
    }
}
