using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

/// <summary>
/// Um container de SQL Server para toda a colecao de testes, nao um por teste: subir
/// o container custa segundos, entao todos os testes de integracao compartilham a
/// mesma instancia e rodam sequencialmente (colecao xUnit garante isso).
/// </summary>
public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    public ContaCertaWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Factory = new ContaCertaWebApplicationFactory(_container.GetConnectionString());

        // So acessar Services ja constroi o host e roda as migrations (Program.cs faz
        // MigrateAsync antes do Run), entao depois disso o schema ja existe no container.
        using var scope = Factory.Services.CreateScope();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Nome)]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Nome = "SqlServer collection";
}
