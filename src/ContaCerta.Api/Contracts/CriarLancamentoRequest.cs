using ContaCerta.Domain.Enums;

namespace ContaCerta.Api.Contracts;

public sealed record CriarLancamentoRequest(
    Guid ContaId,
    Guid CategoriaId,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    Recorrencia Recorrencia);
