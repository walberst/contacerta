using System.Net.Http.Json;
using ContaCerta.Api.Contracts;
using ContaCerta.Application.Categorias;
using ContaCerta.Application.Contas;
using ContaCerta.Domain.Enums;
using ContaCerta.Infrastructure.Realtime;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

/// <summary>
/// Ponta a ponta da regra mais importante do sistema: uma despesa que cruza o
/// percentual de orcamento tem que chegar via SignalR no cliente conectado, nao so
/// gerar o evento internamente. Usa TestServer.CreateWebSocketClient() para simular
/// um WebSocket de verdade dentro do mesmo host de teste (o transporte LongPolling
/// sobre o TestServer em memoria se mostrou pouco confiavel para entrega em tempo
/// real durante os experimentos que levaram a essa escolha).
/// </summary>
[Collection(SqlServerCollection.Nome)]
public class OrcamentoAlertaSignalRTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private HubConnection _connection = null!;

    public OrcamentoAlertaSignalRTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var client = _fixture.Factory.CreateClient();
        var webSocketClient = _fixture.Factory.Server.CreateWebSocketClient();

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(client.BaseAddress!, "hubs/orcamento"), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.HttpMessageHandlerFactory = _ => _fixture.Factory.Server.CreateHandler();
                options.WebSocketFactory = async (context, cancellationToken) =>
                    await webSocketClient.ConnectAsync(context.Uri, cancellationToken);
            })
            .Build();

        await _connection.StartAsync();
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public async Task CriarDespesa_QueCruza80PorCentoDoOrcamento_DeveEmitirAlertaViaSignalR()
    {
        var alertaRecebido = new TaskCompletionSource<AlertaOrcamentoPayload>(TaskCreationOptions.RunContinuationsAsynchronously);
        _connection.On<AlertaOrcamentoPayload>("AlertaOrcamento", payload => alertaRecebido.TrySetResult(payload));

        var client = _fixture.Factory.CreateClient();

        var conta = await client.PostAsJsonAsync(
            "/api/contas", new CriarContaRequest($"Conta {Guid.NewGuid():N}", TipoConta.Negocio, 0m));
        var contaDto = await conta.Content.ReadFromJsonAsync<ContaDto>(JsonHelper.Options);

        var categoria = await client.PostAsJsonAsync(
            "/api/categorias", new CriarCategoriaRequest($"Categoria {Guid.NewGuid():N}", TipoLancamento.Despesa, 1000m, null));
        var categoriaDto = await categoria.Content.ReadFromJsonAsync<CategoriaDto>(JsonHelper.Options);

        // Primeira despesa deixa a categoria em 70%, ainda sem cruzar limite nenhum.
        var primeira = await client.PostAsJsonAsync("/api/lancamentos", new CriarLancamentoRequest(
            contaDto!.Id, categoriaDto!.Id, TipoLancamento.Despesa, 700m,
            DateOnly.FromDateTime(DateTime.Today), "Primeira compra", Recorrencia.Nenhuma));
        primeira.EnsureSuccessStatusCode();

        // Segunda despesa leva para 85%, cruzando o limite de 80% configurado por padrao.
        var segunda = await client.PostAsJsonAsync("/api/lancamentos", new CriarLancamentoRequest(
            contaDto.Id, categoriaDto.Id, TipoLancamento.Despesa, 150m,
            DateOnly.FromDateTime(DateTime.Today), "Segunda compra", Recorrencia.Nenhuma));
        segunda.EnsureSuccessStatusCode();

        var tarefaCompletada = await Task.WhenAny(alertaRecebido.Task, Task.Delay(TimeSpan.FromSeconds(15)));
        tarefaCompletada.Should().Be(alertaRecebido.Task, "o alerta de orcamento deveria ter chegado via SignalR");

        var payload = await alertaRecebido.Task;
        payload.CategoriaId.Should().Be(categoriaDto.Id);
        payload.PercentualLimite.Should().Be(80);
        payload.TotalGastoNoMes.Should().Be(850m);
    }
}
