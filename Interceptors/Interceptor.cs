using MartenApp.Events;
using Newtonsoft.Json;
using Marten.Events;
using Marten;

namespace MartenApp.Interceptors;

public class Interceptor : DocumentSessionListenerBase
{
	public override void BeforeSaveChanges(IDocumentSession session)
	{
		// sync save changes, todo

		return;
	}

	public override Task BeforeSaveChangesAsync(IDocumentSession session, CancellationToken token)
	{
		var pending = session.PendingChanges;

		if (pending is null)
			return Task.CompletedTask;

		List<IEvent> events = pending.Streams().SelectMany(x => x.Events).ToList();

		if (events.Count <= 0)
			return Task.CompletedTask;

		var outboxMessages = new List<MessageOutbox>();
		var options = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

		events.ForEach(x =>
		{
			if (x.Data is IDomainEvent e)
			{
				var data = JsonConvert.SerializeObject(e, options);
				var outbox = new MessageOutbox() { Id = Guid.NewGuid(), CreateAt = DateTime.UtcNow, Content = data };
				outboxMessages.Add(outbox);
			}
		});

		if (outboxMessages.Count > 0)
			session.Insert(outboxMessages.AsEnumerable());

		return Task.CompletedTask;
	}
}
