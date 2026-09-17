using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContaCerta.Infrastructure.Persistence;

/// <summary>
/// Usado apenas pela ferramenta "dotnet ef" para gerar/aplicar migrations em tempo de
/// design. Evita depender do host da Api (que faria MigrateAsync contra um banco que
/// pode nem existir ainda nesse momento) so para conseguir instanciar o DbContext.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=ContaCerta;User Id=sa;Password=DesignTimeOnly_!1;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options, new PublisherNuloParaDesignTime());
    }

    private sealed class PublisherNuloParaDesignTime : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
