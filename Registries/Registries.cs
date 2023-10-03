using MartenApp.Events;
using Marten;

namespace MartenApp.Registries;

public class OrderRegistry : MartenRegistry
{
	public OrderRegistry()
	{
		For<Order>().Identity(x => x.Id);
	}
}

public class OrderSummaryRegistry : MartenRegistry
{
    public OrderSummaryRegistry()
    {
		For<OrderSummary>().Identity(x => x.OrderId);
	}
}

public class UserOrdersSummaryRegistry : MartenRegistry
{
	public UserOrdersSummaryRegistry()
	{
		For<UserOrdersSummary>().Identity(x => x.UserId);
	}
}