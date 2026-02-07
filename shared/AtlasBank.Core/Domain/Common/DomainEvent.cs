namespace AtlasBank.Core.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Interface for domain event handlers
/// </summary>
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Domain event publisher for publishing events
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
}
