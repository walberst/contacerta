using ContaCerta.Domain.Common;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Exceptions;

namespace ContaCerta.Domain.Entities;

/// <summary>
/// Conta bancaria ou carteira controlada pelo usuario. O campo Tipo e o que garante a
/// separacao entre dinheiro pessoal e do negocio nos relatorios, o problema central que
/// o ContaCerta resolve.
/// </summary>
public class Conta : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public TipoConta Tipo { get; private set; }
    public decimal SaldoInicial { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private Conta()
    {
    }

    public static Conta Criar(string nome, TipoConta tipo, decimal saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ValorInvalidoException("Nome da conta e obrigatorio.");
        }

        return new Conta
        {
            Nome = nome.Trim(),
            Tipo = tipo,
            SaldoInicial = saldoInicial,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
        {
            throw new ValorInvalidoException("Nome da conta e obrigatorio.");
        }

        Nome = novoNome.Trim();
    }
}
