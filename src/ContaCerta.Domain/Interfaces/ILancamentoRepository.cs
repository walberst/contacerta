using ContaCerta.Domain.Entities;

namespace ContaCerta.Domain.Interfaces;

public sealed record FiltroLancamentos(
    Guid? ContaId,
    Guid? CategoriaId,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    int Pagina,
    int TamanhoPagina);

public sealed record PaginaDeResultado<T>(IReadOnlyList<T> Itens, int TotalDeItens, int Pagina, int TamanhoPagina);

public interface ILancamentoRepository
{
    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginaDeResultado<Lancamento>> ListarPaginadoAsync(FiltroLancamentos filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lancamento>> ListarRecorrentesAsync(Guid? contaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lancamentos com data dentro do intervalo informado (inclusive), com Conta e
    /// Categoria carregadas. Usado pelo resumo mensal do dashboard, que precisa
    /// separar totais por Conta.Tipo (pessoal x negocio).
    /// </summary>
    Task<IReadOnlyList<Lancamento>> ListarPorPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);

    void Adicionar(Lancamento lancamento);
}
