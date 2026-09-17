using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ContaCerta.Api.IntegrationTests;

/// <summary>
/// Aponta a Api sob teste para o SQL Server real subido via Testcontainers, no lugar
/// da connection string de appsettings.json. Deliberadamente NAO usamos banco em
/// memoria aqui: a ideia e exercitar o mapeamento EF Core, os tipos de coluna e o
/// FluentValidation exatamente como rodam em producao. Usa o TestServer padrao (em
/// memoria) do WebApplicationFactory; o teste de SignalR usa
/// TestServer.CreateWebSocketClient() para simular WebSocket de verdade sobre esse
/// mesmo host, sem precisar de um socket TCP real.
/// </summary>
public class ContaCertaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ContaCertaWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ContaCerta"] = _connectionString
            });
        });
    }
}
