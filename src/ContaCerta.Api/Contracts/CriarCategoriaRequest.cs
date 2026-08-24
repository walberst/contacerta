using ContaCerta.Domain.Enums;

namespace ContaCerta.Api.Contracts;

public sealed record CriarCategoriaRequest(
    string Nome,
    TipoLancamento Tipo,
    decimal OrcamentoMensal,
    IReadOnlyCollection<int>? PercentuaisAlerta);

public sealed record AtualizarOrcamentoRequest(decimal NovoOrcamentoMensal);
