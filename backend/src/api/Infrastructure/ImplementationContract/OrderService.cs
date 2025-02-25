namespace Infrastructure.ImplementationContract;

public sealed class OrderService(
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ISolanaBridge solanaBridge,
    IRadixBridge radixBridge,
    RadixTechnicalAccountBridgeOptions radixOp) : IOrderService
{
    private const string Solana = "Solana";
    private const string Radix = "Radix";

    private const string Sol = "SOL";
    private const string Xrd = "XRD";

    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
    }

    public async Task<Result<CheckBalanceResponse>> CheckBalanceAsync(Guid orderId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
    }
}