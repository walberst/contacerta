using ContaCerta.Domain.Entities;

namespace ContaCerta.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Soma dos lancamentos de despesa da categoria dentro do mes de referencia.
    /// Usado tanto para avaliar cruzamento de limite quanto para a tela de orcamento.
    /// </summary>
    Task<decimal> ObterTotalGastoNoMesAsync(Guid categoriaId, int ano, int mes, CancellationToken cancellationToken = default);

    void Adicionar(Categoria categoria);
}
