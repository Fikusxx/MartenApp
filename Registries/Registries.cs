using MartenApp.Events;
using Marten;

namespace MartenApp.Registries;

public class OrderRegistry : MartenRegistry
{
	public OrderRegistry()
	{
		For<Order>().Identity(x => x.Id);
		For<Order>().GinIndexJsonData();
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