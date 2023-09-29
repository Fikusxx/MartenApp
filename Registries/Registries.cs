using MartenApp.Events;
using Marten;

namespace MartenApp.Registries;

public class OrderRegistry : MartenRegistry
{
	public OrderRegistry()
	{
		For<Order>().Duplicate(x => x.Name);
	}
}