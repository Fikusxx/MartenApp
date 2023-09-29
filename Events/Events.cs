namespace MartenApp.Events;


public interface IHasUserId
{
	public Guid UserId { get; }
}

public sealed record OrderCreatedEvent(Guid Id, Guid OrderId, Guid UserId, string Name, int Qty) : IHasUserId;
public sealed record OrderUpdatedEvent(Guid Id, Guid OrderId, Guid UserId, int Qty) : IHasUserId;
public sealed record OrderCompletedEvent(Guid Id, Guid OrderId, Guid UserId, string Name) : IHasUserId;
public sealed record OrderCancelledEvent(Guid Id);
public sealed record OrderRefundedEvent(Guid Id, string Reason);