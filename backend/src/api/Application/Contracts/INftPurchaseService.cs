namespace Application.Contracts;

public interface INftPurchaseService
{
    Task<Result<Guid>> CreateAsync(Guid rwaId);
}