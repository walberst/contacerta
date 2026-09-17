using System.Net;
using System.Net.Http.Json;
using ContaCerta.Api.Contracts;
using ContaCerta.Application.Categorias;
using ContaCerta.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

[Collection(SqlServerCollection.Nome)]
public class CategoriasEndpointTests
{
    private readonly HttpClient _client;

    public CategoriasEndpointTests(SqlServerFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201()
    {
        var request = new CriarCategoriaRequest($"Categoria {Guid.NewGuid():N}", TipoLancamento.Despesa, 1000m, null);

        var resposta = await _client.PostAsJsonAsync("/api/categorias", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var categoria = await resposta.Content.ReadFromJsonAsync<CategoriaDto>(JsonHelper.Options);
        categoria!.PercentuaisAlerta.Should().BeEquivalentTo(new[] { 80, 100 });
    }

    [Fact]
    public async Task AtualizarOrcamento_CategoriaInexistente_DeveRetornar404()
    {
        var resposta = await _client.PutAsJsonAsync(
            $"/api/categorias/{Guid.NewGuid()}/orcamento", new AtualizarOrcamentoRequest(500m));

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AtualizarOrcamento_ComValorNegativo_DeveRetornar400()
    {
        var criar = await _client.PostAsJsonAsync(
            "/api/categorias", new CriarCategoriaRequest($"Categoria {Guid.NewGuid():N}", TipoLancamento.Despesa, 100m, null));
        var categoria = await criar.Content.ReadFromJsonAsync<CategoriaDto>(JsonHelper.Options);

        var resposta = await _client.PutAsJsonAsync(
            $"/api/categorias/{categoria!.Id}/orcamento", new AtualizarOrcamentoRequest(-10m));

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarOrcamento_ComValorValido_DeveRetornar204EAtualizar()
    {
        var criar = await _client.PostAsJsonAsync(
            "/api/categorias", new CriarCategoriaRequest($"Categoria {Guid.NewGuid():N}", TipoLancamento.Despesa, 100m, null));
        var categoria = await criar.Content.ReadFromJsonAsync<CategoriaDto>(JsonHelper.Options);

        var resposta = await _client.PutAsJsonAsync(
            $"/api/categorias/{categoria!.Id}/orcamento", new AtualizarOrcamentoRequest(2000m));

        resposta.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listagem = await _client.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias", JsonHelper.Options);
        listagem.Should().Contain(c => c.Id == categoria.Id && c.OrcamentoMensal == 2000m);
    }
}
