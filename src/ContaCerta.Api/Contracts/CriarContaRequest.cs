using ContaCerta.Domain.Enums;

namespace ContaCerta.Api.Contracts;

public sealed record CriarContaRequest(string Nome, TipoConta Tipo, decimal SaldoInicial);
