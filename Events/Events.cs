namespace MartenApp.Events;


// dummy interface just to check outbox is working
public interface IDomainEvent
{
	public Guid OrderId { get; }
	public Guid UserId { get; }
}

// for multi select projector
public interface IHasUserId
{
	public Guid UserId { get; }
}

// core events 
public sealed record OrderCreatedEvent(Guid OrderId, Guid UserId, string Name, int Qty) : IHasUserId, IDomainEvent;
public sealed record OrderUpdatedEvent(Guid OrderId, Guid UserId, int Qty) : IHasUserId, IDomainEvent;
public sealed record OrderCompletedEvent(Guid OrderId, Guid UserId, string Name) : IHasUserId, IDomainEvent;


// to check ShouldDelete / DeleteEvent<T> functionality for documents
public sealed record OrderCancelledEvent(Guid Id);
public sealed record OrderRefundedEvent(Guid Id, string Reason);