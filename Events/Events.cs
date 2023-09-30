namespace MartenApp.Events;


public interface IDomainEvent
{
	public Guid Id { get;}
	public Guid OrderId { get; }
	public Guid UserId { get; }
}

public interface IHasUserId
{
	public Guid UserId { get; }
}

public sealed record OrderCreatedEvent(Guid Id, Guid OrderId, Guid UserId, string Name, int Qty) : IHasUserId, IDomainEvent;
public sealed record OrderUpdatedEvent(Guid Id, Guid OrderId, Guid UserId, int Qty) : IHasUserId, IDomainEvent;
public sealed record OrderCompletedEvent(Guid Id, Guid OrderId, Guid UserId, string Name) : IHasUserId, IDomainEvent;
public sealed record OrderCancelledEvent(Guid Id);
public sealed record OrderRefundedEvent(Guid Id, string Reason);