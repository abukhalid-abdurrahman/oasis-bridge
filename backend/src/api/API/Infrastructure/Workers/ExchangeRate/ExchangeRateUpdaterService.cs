namespace API.Infrastructure.Workers.ExchangeRate;

public sealed class ExchangeRateUpdaterService(DataContext context, HttpClient client)
{
    public async Task UpdateExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            decimal solToXrd = await GetExchangeRate("SOL", "XRD");
            decimal xrdToSol = await GetExchangeRate("XRD", "SOL");

            await SaveExchangeRateAsync("SOL", "XRD", solToXrd, cancellationToken);
            await SaveExchangeRateAsync("XRD", "SOL", xrdToSol, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating exchange rates: {ex.Message}");
        }
    }

    private async Task<decimal> GetExchangeRate(string fromSymbol, string toSymbol)
    {
        string url = "https://api.kucoin.com/api/v1/market/allTickers";
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        JObject json = JObject.Parse(responseBody);
        if (json["data"]?["ticker"] is not JArray tickers)
        {
            throw new Exception("Unexpected API response: 'data.ticker' field is missing or invalid.");
        }

        decimal fromUsdt = GetPrice(tickers, fromSymbol + "-USDT");
        decimal toUsdt = GetPrice(tickers, toSymbol + "-USDT");

        if (fromUsdt == 0 || toUsdt == 0)
        {
            throw new Exception($"Failed to fetch exchange rates for {fromSymbol} or {toSymbol}.");
        }

        return fromUsdt / toUsdt;
    }

    private static decimal GetPrice(JArray tickers, string symbol)
    {
        foreach (var ticker in tickers)
        {
            if (ticker["symbol"]?.ToString() == symbol &&
                decimal.TryParse(ticker["last"]?.ToString(), out decimal price))
            {
                return price;
            }
        }

        return 0;
    }

    private async Task SaveExchangeRateAsync(string fromSymbol, string toSymbol, decimal rate,
        CancellationToken cancellationToken)
    {
        NetworkToken? fromToken =
            await context.NetworkTokens.FirstOrDefaultAsync(t => t.Symbol == fromSymbol, cancellationToken);
        NetworkToken? toToken =
            await context.NetworkTokens.FirstOrDefaultAsync(t => t.Symbol == toSymbol, cancellationToken);

        if (fromToken == null || toToken == null)
        {
            Console.WriteLine($"Skipping saving exchange rate: Missing token info for {fromSymbol} or {toSymbol}");
            return;
        }

        var exchangeRate = new Domain.Entities.ExchangeRate
        {
            FromTokenId = fromToken.Id,
            ToTokenId = toToken.Id,
            Rate = rate,
            SourceUrl = "https://api.kucoin.com/api/v1/market/allTickers",
        };

        await context.ExchangeRates.AddAsync(exchangeRate, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}