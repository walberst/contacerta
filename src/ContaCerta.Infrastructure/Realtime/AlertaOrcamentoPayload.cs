namespace ContaCerta.Infrastructure.Realtime;

/// <summary>
/// Formato enviado ao dashboard via SignalR. Separado do evento de dominio para o
/// contrato do front nao ficar amarrado a forma interna do evento.
/// </summary>
public sealed record AlertaOrcamentoPayload(
    Guid CategoriaId,
    string NomeCategoria,
    Guid ContaId,
    int PercentualLimite,
    decimal TotalGastoNoMes,
    decimal OrcamentoMensal,
    DateOnly Referencia);
