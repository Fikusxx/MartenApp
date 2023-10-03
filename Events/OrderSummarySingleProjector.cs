using Marten.Events.Aggregation;

namespace MartenApp.Events;

public class OrderSummarySingleProjector : SingleStreamProjection<OrderSummary>
{
    public OrderSummarySingleProjector()
    {
		IncludeType<OrderCreatedEvent>();
		IncludeType<OrderUpdatedEvent>();
		IncludeType<OrderCompletedEvent>();
	}

    public OrderSummary Create(OrderCreatedEvent e)
	{
		var data = new OrderSummary() 
		{ 
			OrderId = e.OrderId,
			UserId = e.UserId,
			Qty = e.Qty
		};

		data.Names.Add(e.Name);

		return data;
	}

	public void Apply(OrderSummary snapshot, OrderUpdatedEvent e)
	{
		snapshot.Qty += e.Qty;
	}

	public void Apply(OrderSummary snapshot, OrderCompletedEvent e)
	{
		snapshot.Names.Add(e.Name);
	}
}
