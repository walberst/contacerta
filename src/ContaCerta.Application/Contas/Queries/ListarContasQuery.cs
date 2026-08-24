using ContaCerta.Domain.Interfaces;
using MediatR;

namespace ContaCerta.Application.Contas.Queries;

public sealed record ListarContasQuery : IRequest<IReadOnlyList<ContaDto>>;

public class ListarContasQueryHandler : IRequestHandler<ListarContasQuery, IReadOnlyList<ContaDto>>
{
    private readonly IContaRepository _contaRepository;

    public ListarContasQueryHandler(IContaRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<IReadOnlyList<ContaDto>> Handle(ListarContasQuery request, CancellationToken cancellationToken)
    {
        var contas = await _contaRepository.ListarAsync(cancellationToken);

        var dtos = new List<ContaDto>(contas.Count);
        foreach (var conta in contas)
        {
            var saldoAtual = await _contaRepository.ObterSaldoAtualAsync(conta.Id, cancellationToken);
            dtos.Add(new ContaDto(conta.Id, conta.Nome, conta.Tipo, conta.SaldoInicial, saldoAtual));
        }

        return dtos;
    }
}
