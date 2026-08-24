namespace ContaCerta.Domain.Enums;

/// <summary>
/// Por enquanto o sistema so entende recorrencia mensal (o cenario real do MEI:
/// aluguel, assinatura de sistema, parcela de emprestimo). Deixado como enum, e nao
/// bool, para abrir espaco a semanal/anual no futuro sem quebrar o modelo.
/// </summary>
public enum Recorrencia
{
    Nenhuma = 0,
    Mensal = 1
}
