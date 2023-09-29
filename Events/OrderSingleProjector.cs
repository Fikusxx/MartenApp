using Marten.Events.Aggregation;

namespace MartenApp.Events;

public class OrderSingleProjector : SingleStreamProjection<Order>
{
    public OrderSingleProjector()
    {
		DeleteEvent<OrderCancelledEvent>();
		DeleteEvent<OrderRefundedEvent>(x => x.Reason == "KEKW");
		
    }

    public Order Create(OrderCreatedEvent e)
	{
		return new Order() { Id = e.Id, Name = e.Name, Qty = e.Qty };
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
