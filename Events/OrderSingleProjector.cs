using Marten.Events.Aggregation;

namespace MartenApp.Events;

public class OrderSingleProjector : SingleStreamProjection<Order>
{
    public OrderSingleProjector()
    {
		DeleteEvent<OrderCancelledEvent>();
		DeleteEvent<OrderRefundedEvent>(x => x.Reason == "KEKW");

		IncludeType<OrderCreatedEvent>();
		IncludeType<OrderUpdatedEvent>();
		IncludeType<OrderCompletedEvent>();
    }

    public Order Create(OrderCreatedEvent e)
	{
		var data = new Order() { Id = e.OrderId, Name = e.Name, Qty = e.Qty };

		return data;
	}

	public void Apply(Order snapshot, OrderUpdatedEvent e)
	{
		snapshot.Qty += e.Qty;
	}

	public void Apply(Order snapshot, OrderCompletedEvent e)
	{
		snapshot.Name = e.Name;
	}
}
