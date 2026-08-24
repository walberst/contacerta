namespace ContaCerta.Application.Common.Models;

public sealed record PaginatedResult<T>(IReadOnlyList<T> Itens, int TotalDeItens, int Pagina, int TamanhoPagina)
{
    public int TotalDePaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalDeItens / (double)TamanhoPagina);
}
