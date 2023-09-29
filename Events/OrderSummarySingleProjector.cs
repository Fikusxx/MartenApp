using Marten.Events.Aggregation;

namespace MartenApp.Events;

public class OrderSummarySingleProjector : SingleStreamProjection<OrderSummary>
{
	public OrderSummary Create(OrderCreatedEvent e)
	{
		var data = new OrderSummary();
		data.Qty += e.Qty;
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
