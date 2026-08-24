using ContaCerta.Domain.Enums;

namespace ContaCerta.Application.Categorias;

public sealed record CategoriaDto(
    Guid Id,
    string Nome,
    TipoLancamento Tipo,
    decimal OrcamentoMensal,
    IReadOnlyCollection<int> PercentuaisAlerta);

/// <summary>
/// DTO usado na tela de orcamento por categoria: mostra o quanto ja foi gasto no mes
/// corrente e o percentual utilizado, para o front desenhar a barra de progresso.
/// </summary>
public sealed record CategoriaOrcamentoDto(
    Guid Id,
    string Nome,
    decimal OrcamentoMensal,
    decimal TotalGastoNoMes,
    decimal PercentualUtilizado,
    bool OrcamentoEstourado);
