namespace Domain;

public abstract class Entity<TId> : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public TId Id { get; init; }
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
