namespace RadixBridge.Helpers;

/// <summary>
/// Provides helper methods for making HTTP requests to the Radix API.
/// These methods assist with sending POST requests and deserializing the response.
/// </summary>
public static class RadixHttpClientHelper
{
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
            // Serialize the request object to JSON for transmission
            string json = JsonConvert.SerializeObject(request);

            // Create the HTTP content with the serialized JSON data, set the encoding to UTF-8, and specify the content type as application/json
            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the HTTP POST request and await the response
            using HttpResponseMessage response = await httpClient.PostAsync(url, content, token);

            // Check if the response indicates success (status code 2xx)
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string stringRes = await response.Content.ReadAsStringAsync(token);

                // Deserialize the response string into the specified response type and return it
                return JsonConvert.DeserializeObject<TResponse>(stringRes);
            }

            // Return default (null) if the response status indicates failure
            return default;
        }
        catch (Exception e)
        {
            // Log the error to the console (can be replaced with proper logging mechanism)
            Console.WriteLine($"HTTP Request Error: {e.Message}");

            // Rethrow the exception to allow upstream handling of the error
            throw;
        }
    }

    /// <summary>
    /// Sends a request to retrieve construction metadata, such as the current epoch, for a given network.
    /// This method uses the <see cref="PostAsync{TRequest,TResponse}"/> helper method.
    /// </summary>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="options"></param>
    /// <returns>A task representing the operation. The task result contains the current epoch response, or null if the request fails.</returns>
    public static async Task<CurrentEpochResponse?> GetConstructionMetadata(this HttpClient client,RadixTechnicalAccountBridgeOptions options)
    {
        // Prepare the data to be sent in the request, containing the network ID
        var data = new
        {
            network = options.NetworkId==0x01
                ?RadixBridgeHelper.MainNet
                :RadixBridgeHelper.StokeNet
        };

        // Use the PostAsync helper method to send the request and retrieve the response
        return await PostAsync<object, CurrentEpochResponse>(
            client,
            "https://stokenet-core.radix.live/core/lts/transaction/construction", // URL for the endpoint
            data // The network ID data to send in the request body
        );
    }
}