using ContaCerta.Domain.Entities;

namespace ContaCerta.Domain.Interfaces;

public interface IContaRepository
{
    Task<Conta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<decimal> ObterSaldoAtualAsync(Guid contaId, CancellationToken cancellationToken = default);
    void Adicionar(Conta conta);
}
