using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaCerta.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(cancellationToken);

    public async Task<decimal> ObterTotalGastoNoMesAsync(
        Guid categoriaId, int ano, int mes, CancellationToken cancellationToken = default)
    {
        var inicio = new DateOnly(ano, mes, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);

        return await _context.Lancamentos
            .Where(l => l.CategoriaId == categoriaId
                     && l.Tipo == TipoLancamento.Despesa
                     && l.Data >= inicio
                     && l.Data <= fim)
            .SumAsync(l => (decimal?)l.Valor, cancellationToken) ?? 0m;
    }

    public void Adicionar(Categoria categoria) => _context.Categorias.Add(categoria);
}
