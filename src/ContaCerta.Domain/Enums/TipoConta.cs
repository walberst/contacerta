namespace ContaCerta.Domain.Enums;

/// <summary>
/// A separacao pessoal versus negocio e o motivo do sistema existir: o MEI que mistura
/// as duas coisas so percebe o rombo tarde demais. Cada conta carrega essa marcacao para
/// que relatorios e projecao de fluxo de caixa nunca somem os dois mundos sem querer.
/// </summary>
public enum TipoConta
{
    Pessoal = 1,
    Negocio = 2
}
