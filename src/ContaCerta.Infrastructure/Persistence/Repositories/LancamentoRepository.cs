using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaCerta.Infrastructure.Persistence.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly ApplicationDbContext _context;

    public LancamentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Lancamentos
            .Include(l => l.Conta)
            .Include(l => l.Categoria)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<PaginaDeResultado<Lancamento>> ListarPaginadoAsync(
        FiltroLancamentos filtro, CancellationToken cancellationToken = default)
    {
        var query = _context.Lancamentos
            .Include(l => l.Conta)
            .Include(l => l.Categoria)
            .AsQueryable();

        if (filtro.ContaId is { } contaId)
        {
            query = query.Where(l => l.ContaId == contaId);
        }

        if (filtro.CategoriaId is { } categoriaId)
        {
            query = query.Where(l => l.CategoriaId == categoriaId);
        }

        if (filtro.DataInicio is { } dataInicio)
        {
            query = query.Where(l => l.Data >= dataInicio);
        }

        if (filtro.DataFim is { } dataFim)
        {
            query = query.Where(l => l.Data <= dataFim);
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.DataCriacao)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return new PaginaDeResultado<Lancamento>(itens, total, filtro.Pagina, filtro.TamanhoPagina);
    }

    public async Task<IReadOnlyList<Lancamento>> ListarRecorrentesAsync(
        Guid? contaId, CancellationToken cancellationToken = default)
    {
        var query = _context.Lancamentos.Where(l => l.Recorrencia != Recorrencia.Nenhuma);

        if (contaId is { } id)
        {
            query = query.Where(l => l.ContaId == id);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lancamento>> ListarPorPeriodoAsync(
        DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        return await _context.Lancamentos
            .Include(l => l.Conta)
            .Where(l => l.Data >= inicio && l.Data <= fim)
            .ToListAsync(cancellationToken);
    }

    public void Adicionar(Lancamento lancamento) => _context.Lancamentos.Add(lancamento);
}
