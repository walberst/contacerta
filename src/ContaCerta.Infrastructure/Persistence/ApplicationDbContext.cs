using ContaCerta.Domain.Common;
using ContaCerta.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaCerta.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// So despacha os eventos de dominio depois que o SaveChanges de verdade
    /// aconteceu. Se a gente publicasse antes e o commit falhasse, o dashboard
    /// receberia via SignalR um alerta de orcamento que nunca foi persistido.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entidadesComEventos = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var resultado = await base.SaveChangesAsync(cancellationToken);

        var eventos = entidadesComEventos.SelectMany(e => e.DomainEvents).ToList();
        foreach (var entidade in entidadesComEventos)
        {
            entidade.ClearDomainEvents();
        }

        foreach (var evento in eventos)
        {
            await _publisher.Publish(evento, cancellationToken);
        }

        return resultado;
    }
}
