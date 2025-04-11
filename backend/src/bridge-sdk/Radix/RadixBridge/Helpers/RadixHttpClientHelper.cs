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
        try
        {
            Logger.LogInformation("Starting PostAsync for URL: {Url} at {Time}", url, DateTimeOffset.UtcNow);

            // Serialize the request object to JSON
            Logger.LogInformation("Serializing request object of type {Type} to JSON.", typeof(TRequest));
            string json = JsonConvert.SerializeObject(request);

            Logger.LogInformation("Serialized JSON: {Json}", json);

            // Create the HTTP content with the serialized JSON data
            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            Logger.LogInformation("HTTP content created with UTF-8 encoding and 'application/json' content type.");

            // Send the HTTP POST request and await the response
            Logger.LogInformation("Sending HTTP POST request to URL: {Url}", url);
            using HttpResponseMessage response = await httpClient.PostAsync(url, content, token);

            Logger.LogInformation("Received HTTP response with status code: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                Logger.LogInformation("Response status indicates success. Reading response content.");
                string stringRes = await response.Content.ReadAsStringAsync(token);
                Logger.LogInformation("Response content read: {ResponseContent}", stringRes);

                // Deserialize the response string into the specified response type
                Logger.LogInformation("Deserializing response into type {Type}.", typeof(TResponse));
                TResponse? deserializedResponse = JsonConvert.DeserializeObject<TResponse>(stringRes);
                Logger.LogInformation("Deserialization successful. Returning deserialized response.");
                return deserializedResponse;
            }

            Logger.LogWarning("HTTP response was not successful. Status code: {StatusCode}", response.StatusCode);
            return default;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "HTTP Request Error in PostAsync: {Message}", e.Message);
            throw; // Rethrow to allow upstream handling
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
        Logger.LogInformation("Starting GetConstructionMetadata at {Time}", DateTimeOffset.UtcNow);

        // Prepare the data with the correct network identifier
        var data = new
        {
            network = options.NetworkId == 0x01
                ? RadixBridgeHelper.MainNet
                : RadixBridgeHelper.StokeNet
        };

        Logger.LogInformation("Prepared request data for construction metadata: {Data}",
            JsonConvert.SerializeObject(data));

        // Use the PostAsync helper method to send the request
        CurrentEpochResponse? response = await PostAsync<object, CurrentEpochResponse>(
            client,
            $"{options.HostUri}/core/lts/transaction/construction",
            data
        );

        Logger.LogInformation("Finished GetConstructionMetadata at {Time}", DateTimeOffset.UtcNow);
        return response;
    }
}