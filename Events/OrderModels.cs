﻿using MartenApp.Repositories;

namespace MartenApp.Events;

// Aggregate
public class Order : IAggregateRoot
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public int Qty { get; set; }
}

// Singlestream Read Model
public class OrderSummary : IReadModel
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public List<string> Names { get; set; } = new();
    public int Qty { get; set; }
}

// Multistream Read Model
public class UserOrdersSummary : IReadModel
{
    public Guid UserId { get; set; }
    public List<Guid> OrderIds { get; set; } = new();
    public int Qty { get; set; }
}