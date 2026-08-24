namespace ContaCerta.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
