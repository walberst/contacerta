namespace ContaCerta.Application.Dashboard;

public sealed record ResumoPorTipoContaDto(decimal TotalReceitas, decimal TotalDespesas, decimal Saldo);

/// <summary>
/// Resumo do mes corrente separado por pessoal e negocio, o cerne do problema que o
/// ContaCerta resolve: nunca deixar o dono do MEI enxergar os dois como uma coisa so.
/// </summary>
public sealed record ResumoMensalDto(
    int Ano,
    int Mes,
    ResumoPorTipoContaDto Pessoal,
    ResumoPorTipoContaDto Negocio,
    ResumoPorTipoContaDto Consolidado);
