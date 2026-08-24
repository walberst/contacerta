using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;

namespace ContaCerta.Domain.Services;

public sealed record PontoFluxoCaixa(DateOnly Data, decimal Saldo);

public sealed record ProjecaoFluxoCaixa(
    decimal SaldoAtual,
    decimal SaldoProjetado,
    decimal TotalReceitasPrevistas,
    decimal TotalDespesasPrevistas,
    IReadOnlyList<PontoFluxoCaixa> Pontos);

/// <summary>
/// Projeta o saldo dos proximos N dias somando, dia a dia, as ocorrencias futuras de
/// lancamentos recorrentes ao saldo atual. Nao tenta prever gastos avulsos (aqueles
/// sem recorrencia), porque nao ha base nenhuma para estimar quando ou se eles vao
/// se repetir, seria chute e nao projecao.
/// </summary>
public static class ProjetorDeFluxoDeCaixa
{
    public const int HorizonteDiasPadrao = 30;

    public static ProjecaoFluxoCaixa Projetar(
        decimal saldoAtual,
        IEnumerable<Lancamento> lancamentosRecorrentes,
        DateOnly dataReferencia,
        int horizonteDias = HorizonteDiasPadrao)
    {
        var fimJanela = dataReferencia.AddDays(horizonteDias);
        var deltasPorDia = new SortedDictionary<DateOnly, decimal>();

        var totalReceitas = 0m;
        var totalDespesas = 0m;

        foreach (var lancamento in lancamentosRecorrentes.Where(l => l.EhRecorrente))
        {
            foreach (var ocorrencia in lancamento.OcorrenciasEntre(dataReferencia, fimJanela))
            {
                var impacto = lancamento.Tipo == TipoLancamento.Receita ? lancamento.Valor : -lancamento.Valor;
                deltasPorDia[ocorrencia] = deltasPorDia.GetValueOrDefault(ocorrencia) + impacto;

                if (lancamento.Tipo == TipoLancamento.Receita)
                {
                    totalReceitas += lancamento.Valor;
                }
                else
                {
                    totalDespesas += lancamento.Valor;
                }
            }
        }

        var pontos = new List<PontoFluxoCaixa>(horizonteDias);
        var saldoCorrente = saldoAtual;

        for (var i = 1; i <= horizonteDias; i++)
        {
            var dia = dataReferencia.AddDays(i);
            if (deltasPorDia.TryGetValue(dia, out var delta))
            {
                saldoCorrente += delta;
            }

            pontos.Add(new PontoFluxoCaixa(dia, saldoCorrente));
        }

        return new ProjecaoFluxoCaixa(saldoAtual, saldoCorrente, totalReceitas, totalDespesas, pontos);
    }
}
