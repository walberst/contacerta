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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ContaCerta")
            ?? throw new InvalidOperationException("Connection string 'ContaCerta' nao configurada.");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IContaRepository, ContaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<SeedDataRunner>();

        services.AddSignalR();

        return services;
    }
}
