using System.Net;
using System.Net.Http.Json;
using ContaCerta.Api.Contracts;
using ContaCerta.Application.Contas;
using ContaCerta.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

[Collection(SqlServerCollection.Nome)]
public class ContasEndpointTests
{
    private readonly HttpClient _client;

    public ContasEndpointTests(SqlServerFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201EPersistir()
    {
        var request = new CriarContaRequest($"Conta teste {Guid.NewGuid():N}", TipoConta.Negocio, 1000m);

        var resposta = await _client.PostAsJsonAsync("/api/contas", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var conta = await resposta.Content.ReadFromJsonAsync<ContaDto>(JsonHelper.Options);
        conta.Should().NotBeNull();
        conta!.SaldoAtual.Should().Be(1000m);
    }

    [Fact]
    public async Task Criar_ComNomeVazio_DeveRetornar400()
    {
        var request = new CriarContaRequest(string.Empty, TipoConta.Pessoal, 0m);

        var resposta = await _client.PostAsJsonAsync("/api/contas", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_DuasVezesComMesmoNomeETipo_SegundaDeveRetornar409()
    {
        var nome = $"Conta duplicada {Guid.NewGuid():N}";
        var request = new CriarContaRequest(nome, TipoConta.Negocio, 0m);

        var primeira = await _client.PostAsJsonAsync("/api/contas", request);
        var segunda = await _client.PostAsJsonAsync("/api/contas", request);

        primeira.StatusCode.Should().Be(HttpStatusCode.Created);
        segunda.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Listar_DeveIncluirContaRecemCriada()
    {
        var nome = $"Conta listagem {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/contas", new CriarContaRequest(nome, TipoConta.Pessoal, 250m));

        var resposta = await _client.GetAsync("/api/contas");
        var contas = await resposta.Content.ReadFromJsonAsync<List<ContaDto>>(JsonHelper.Options);

        contas.Should().Contain(c => c.Nome == nome);
    }
}
