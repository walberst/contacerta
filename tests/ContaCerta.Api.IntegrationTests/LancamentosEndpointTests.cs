using System.Net;
using System.Net.Http.Json;
using ContaCerta.Api.Contracts;
using ContaCerta.Application.Common.Models;
using ContaCerta.Application.Contas;
using ContaCerta.Application.Categorias;
using ContaCerta.Application.Lancamentos;
using ContaCerta.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

[Collection(SqlServerCollection.Nome)]
public class LancamentosEndpointTests
{
    private readonly HttpClient _client;

    public LancamentosEndpointTests(SqlServerFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    private async Task<(Guid ContaId, Guid CategoriaId)> CriarContaECategoriaAsync()
    {
        var conta = await _client.PostAsJsonAsync(
            "/api/contas", new CriarContaRequest($"Conta {Guid.NewGuid():N}", TipoConta.Negocio, 5000m));
        var contaDto = await conta.Content.ReadFromJsonAsync<ContaDto>(JsonHelper.Options);

        var categoria = await _client.PostAsJsonAsync(
            "/api/categorias", new CriarCategoriaRequest($"Categoria {Guid.NewGuid():N}", TipoLancamento.Despesa, 1000m, null));
        var categoriaDto = await categoria.Content.ReadFromJsonAsync<CategoriaDto>(JsonHelper.Options);

        return (contaDto!.Id, categoriaDto!.Id);
    }

    [Fact]
    public async Task Criar_ComContaECategoriaValidas_DeveRetornar201()
    {
        var (contaId, categoriaId) = await CriarContaECategoriaAsync();

        var request = new CriarLancamentoRequest(
            contaId, categoriaId, TipoLancamento.Despesa, 150m, DateOnly.FromDateTime(DateTime.Today), "Compra teste", Recorrencia.Nenhuma);

        var resposta = await _client.PostAsJsonAsync("/api/lancamentos", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Criar_ComContaInexistente_DeveRetornar404()
    {
        var (_, categoriaId) = await CriarContaECategoriaAsync();

        var request = new CriarLancamentoRequest(
            Guid.NewGuid(), categoriaId, TipoLancamento.Despesa, 150m, DateOnly.FromDateTime(DateTime.Today), "Teste", Recorrencia.Nenhuma);

        var resposta = await _client.PostAsJsonAsync("/api/lancamentos", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Criar_ComValorZero_DeveRetornar400()
    {
        var (contaId, categoriaId) = await CriarContaECategoriaAsync();

        var request = new CriarLancamentoRequest(
            contaId, categoriaId, TipoLancamento.Despesa, 0m, DateOnly.FromDateTime(DateTime.Today), "Teste", Recorrencia.Nenhuma);

        var resposta = await _client.PostAsJsonAsync("/api/lancamentos", request);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Listar_ComPaginacao_DeveRespeitarTamanhoDaPagina()
    {
        var (contaId, categoriaId) = await CriarContaECategoriaAsync();

        for (var i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/lancamentos", new CriarLancamentoRequest(
                contaId, categoriaId, TipoLancamento.Despesa, 10m + i, DateOnly.FromDateTime(DateTime.Today), $"Item {i}", Recorrencia.Nenhuma));
        }

        var resposta = await _client.GetFromJsonAsync<PaginatedResult<LancamentoDto>>(
            $"/api/lancamentos?contaId={contaId}&pagina=1&tamanhoPagina=2", JsonHelper.Options);

        resposta!.Itens.Should().HaveCount(2);
        resposta.TotalDeItens.Should().Be(5);
        resposta.TotalDePaginas.Should().Be(3);
    }
}
