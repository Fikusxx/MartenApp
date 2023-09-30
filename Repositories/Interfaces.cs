namespace MartenApp.Repositories;

public interface IAggregateRoot { } 
public interface IAggregateRootRepository<T> where T : IAggregateRoot
{
	public Task StoreAsync(T aggregate, CancellationToken ct = default);
	public Task<T?> AggregateAsync(Guid id, int? version = null, CancellationToken ct = default);
	public Task<T?> LoadAsync(Guid id, CancellationToken ct = default);
}

public interface IReadModel { }
public interface IReadModelRepository<T> where T : IReadModel 
{
	public Task<T?> LoadAsync(Guid id, CancellationToken ct = default);
}