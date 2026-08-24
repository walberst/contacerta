using ContaCerta.Domain.Enums;

namespace ContaCerta.Application.Contas;

public sealed record ContaDto(
    Guid Id,
    string Nome,
    TipoConta Tipo,
    decimal SaldoInicial,
    decimal SaldoAtual);
