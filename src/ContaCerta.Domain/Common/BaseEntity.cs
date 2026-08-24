using MediatR;

namespace ContaCerta.Domain.Common;

/// <summary>
/// Base para entidades que participam de regras de negocio e podem gerar eventos de
/// dominio. Guardamos os eventos aqui e so os despachamos depois que a transacao foi
/// persistida com sucesso (ver ApplicationDbContext.SaveChangesAsync), assim nunca
/// notificamos um alerta de orcamento que acabou sendo revertido por falha no banco.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<INotification> _domainEvents = new();

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
