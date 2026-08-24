using ContaCerta.Domain.Enums;

namespace ContaCerta.Application.Lancamentos;

public sealed record LancamentoDto(
    Guid Id,
    Guid ContaId,
    string NomeConta,
    Guid CategoriaId,
    string NomeCategoria,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    Recorrencia Recorrencia);
