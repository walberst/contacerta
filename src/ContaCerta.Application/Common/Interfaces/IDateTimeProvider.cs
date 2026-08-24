namespace ContaCerta.Application.Common.Interfaces;

/// <summary>
/// Abstrai "agora" para que handlers e testes nao dependam de DateTime.Now direto,
/// o que tornaria os testes de projecao de fluxo de caixa nao deterministicos.
/// </summary>
public interface IDateTimeProvider
{
    DateOnly Hoje { get; }
    DateTime AgoraUtc { get; }
}
