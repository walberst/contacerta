using ContaCerta.Domain.Common;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Exceptions;

namespace ContaCerta.Domain.Entities;

/// <summary>
/// Categoria de lancamento (ex: Aluguel, Fornecedor, Alimentacao). Quando o orcamento
/// mensal e maior que zero, a categoria passa a ser monitorada: cada despesa lancada
/// recalcula o total do mes e verifica se algum percentual configurado foi cruzado.
/// </summary>
public class Categoria : BaseEntity
{
    private static readonly int[] PercentuaisPadrao = { 80, 100 };

    public string Nome { get; private set; } = string.Empty;
    public TipoLancamento Tipo { get; private set; }
    public decimal OrcamentoMensal { get; private set; }
    public DateTime DataCriacao { get; private set; }

    // Guardado como CSV (ex: "80,100") em vez de colecao mapeada diretamente porque o
    // EF Core, no SQL Server, nao lida bem com colecao primitiva atras de campo
    // privado sem setter publico. String com setter privado e trivial de mapear e
    // o parse fica escondido atras da propriedade publica abaixo.
    public string PercentuaisAlertaCsv { get; private set; } = string.Empty;

    public IReadOnlyCollection<int> PercentuaisAlerta => ParseCsv(PercentuaisAlertaCsv);

    private Categoria()
    {
    }

    public static Categoria Criar(
        string nome,
        TipoLancamento tipo,
        decimal orcamentoMensal,
        IEnumerable<int>? percentuaisAlerta = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ValorInvalidoException("Nome da categoria e obrigatorio.");
        }

        if (orcamentoMensal < 0)
        {
            throw new ValorInvalidoException("Orcamento mensal nao pode ser negativo.");
        }

        var percentuaisValidos = (percentuaisAlerta ?? PercentuaisPadrao)
            .Where(p => p is > 0 and <= 300)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        IReadOnlyCollection<int> percentuaisFinais = percentuaisValidos.Count > 0 ? percentuaisValidos : PercentuaisPadrao;

        return new Categoria
        {
            Nome = nome.Trim(),
            Tipo = tipo,
            OrcamentoMensal = orcamentoMensal,
            DataCriacao = DateTime.UtcNow,
            PercentuaisAlertaCsv = string.Join(',', percentuaisFinais)
        };
    }

    public void AtualizarOrcamento(decimal novoOrcamentoMensal)
    {
        if (novoOrcamentoMensal < 0)
        {
            throw new ValorInvalidoException("Orcamento mensal nao pode ser negativo.");
        }

        OrcamentoMensal = novoOrcamentoMensal;
    }

    /// <summary>
    /// Retorna os percentuais configurados que passaram a ser ultrapassados entre
    /// o total antes e o total depois do novo lancamento. So conta como "cruzado" o
    /// percentual que o total anterior ainda nao alcancava, para nao repetir o mesmo
    /// alerta em toda despesa lancada depois que a categoria ja estourou.
    /// </summary>
    public IReadOnlyCollection<int> ObterLimitesCruzados(decimal totalGastoAntes, decimal totalGastoDepois)
    {
        if (OrcamentoMensal <= 0)
        {
            return Array.Empty<int>();
        }

        var cruzados = new List<int>();
        foreach (var percentual in PercentuaisAlerta.OrderBy(p => p))
        {
            var limiteValor = OrcamentoMensal * percentual / 100m;
            if (totalGastoAntes < limiteValor && totalGastoDepois >= limiteValor)
            {
                cruzados.Add(percentual);
            }
        }

        return cruzados;
    }

    private static IReadOnlyCollection<int> ParseCsv(string csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<int>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
}
