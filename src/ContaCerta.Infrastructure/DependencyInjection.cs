using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Domain.Interfaces;
using ContaCerta.Infrastructure.Persistence;
using ContaCerta.Infrastructure.Persistence.Repositories;
using ContaCerta.Infrastructure.Realtime;
using ContaCerta.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContaCerta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // A connection string e resolvida dentro do callback (via IConfiguration injetado),
        // nunca lida antecipadamente aqui fora do container: o WebApplicationFactory dos
        // testes de integracao so injeta o override de configuracao no momento do Build(),
        // entao ler configuration.GetConnectionString direto aqui pegaria sempre o valor
        // antigo do appsettings.json e quebraria os testes contra o SQL Server de teste.
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("ContaCerta")
                ?? throw new InvalidOperationException("Connection string 'ContaCerta' nao configurada.");

            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IContaRepository, ContaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<SeedDataRunner>();

        // AddMediatR na Application so escaneia o assembly da Application (onde ficam
        // commands/queries). O handler que traduz alerta de orcamento em push do
        // SignalR mora aqui na Infrastructure (depende de IHubContext), entao precisa
        // de um segundo registro apontando para este assembly, senao o MediatR nunca
        // encontra esse INotificationHandler e o Publish vira um no-op silencioso.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddSignalR();

        return services;
    }
}
