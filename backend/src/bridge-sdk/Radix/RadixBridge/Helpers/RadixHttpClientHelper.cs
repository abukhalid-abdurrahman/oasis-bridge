namespace RadixBridge.Helpers;

/// <summary>
/// Provides helper methods for making HTTP requests to the Radix API.
/// These methods assist with sending POST requests and deserializing the response.
/// </summary>
public static class RadixHttpClientHelper
{
    // Static logger for detailed logging. In production, consider using dependency injection.
    private static readonly ILogger Logger = LoggerFactory.Create(_ => { })
        .CreateLogger("RadixHttpClientHelper");

    /// <summary>
    /// Sends an HTTP POST request with the specified request object and deserializes the response into the specified response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object to be sent in the body.</typeparam>
    /// <typeparam name="TResponse">The type of the response to be deserialized.</typeparam>
    /// <param name="httpClient">The HTTP client used to send the request.</param>
    /// <param name="url">The URL to which the POST request is sent.</param>
    /// <param name="request">The request object to be serialized and sent in the POST body.</param>
    /// <param name="token">The cancellation token to monitor task cancellation.</param>
    /// <returns>The deserialized response object of type TResponse, or null if the request was unsuccessful or an error occurred.</returns>
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(HttpClient httpClient, string url,
        TRequest request, CancellationToken token = default)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        Logger.OperationStarted(nameof(PostAsync), date);
        try
        {
            string json = JsonConvert.SerializeObject(request);

            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await httpClient.PostAsync(url, content, token);


            if (response.IsSuccessStatusCode)
            {
                string stringRes = await response.Content.ReadAsStringAsync(token);
                TResponse? deserializedResponse = JsonConvert.DeserializeObject<TResponse>(stringRes);
                Logger.OperationCompleted(nameof(PostAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return deserializedResponse;
            }


            Logger.OperationCompleted(nameof(PostAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);

            return default;
        }
        catch (Exception e)
        {
            Logger.OperationException(nameof(PostAsync), e.Message);
            Logger.OperationCompleted(nameof(PostAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return default;
        }
    }

    /// <summary>
    /// Sends a request to retrieve construction metadata, such as the current epoch, for a given network.
    /// </summary>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="options">The Radix technical account bridge options.</param>
    /// <returns>A task representing the operation. The task result contains the current epoch response, or null if the request fails.</returns>
    public static async Task<CurrentEpochResponse?> GetConstructionMetadata(this HttpClient client,
        RadixTechnicalAccountBridgeOptions options)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        Logger.OperationStarted(nameof(GetConstructionMetadata), date);

        try
        {
            var data = new
            {
                network = options.NetworkId == 0x01
                    ? RadixBridgeHelper.MainNet
                    : RadixBridgeHelper.StokeNet
            };


            CurrentEpochResponse? response = await PostAsync<object, CurrentEpochResponse>(
                client,
                $"{options.HostUri}/core/lts/transaction/construction",
                data
            );

            Logger.OperationCompleted(nameof(GetConstructionMetadata), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return response;
        }
        catch (Exception e)
        {
            Logger.OperationException(nameof(GetConstructionMetadata), e.Message);
            Logger.OperationCompleted(nameof(GetConstructionMetadata), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return default;
        }
    }
}