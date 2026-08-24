using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaCerta.Infrastructure.Persistence.Repositories;

public class ContaRepository : IContaRepository
{
    private readonly ApplicationDbContext _context;

    public ContaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Conta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Contas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Conta>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _context.Contas.OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task<decimal> ObterSaldoAtualAsync(Guid contaId, CancellationToken cancellationToken = default)
    {
        var conta = await _context.Contas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == contaId, cancellationToken);
        if (conta is null)
        {
            return 0m;
        }

        var totalReceitas = await _context.Lancamentos
            .Where(l => l.ContaId == contaId && l.Tipo == TipoLancamento.Receita)
            .SumAsync(l => (decimal?)l.Valor, cancellationToken) ?? 0m;

        var totalDespesas = await _context.Lancamentos
            .Where(l => l.ContaId == contaId && l.Tipo == TipoLancamento.Despesa)
            .SumAsync(l => (decimal?)l.Valor, cancellationToken) ?? 0m;

        return conta.SaldoInicial + totalReceitas - totalDespesas;
    }

    public void Adicionar(Conta conta) => _context.Contas.Add(conta);
}
