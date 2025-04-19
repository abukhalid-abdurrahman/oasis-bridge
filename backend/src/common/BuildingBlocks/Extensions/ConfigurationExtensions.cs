namespace BuildingBlocks.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Retrieves the required string value for the specified configuration key.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="key">The key of the configuration value.</param>
    /// <returns>The non-empty string value associated with the specified key.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the configuration value is null, empty, or consists only of white-space characters.
    /// </exception>
    public static string GetRequiredString(this IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Configuration value for '{key}' is required and was not found or empty.");

        return value;
    }

    /// <summary>
    /// Retrieves the required integer value for the specified configuration key.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="key">The key of the configuration value.</param>
    /// <returns>The parsed integer value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the value is missing or cannot be parsed as a valid integer.
    /// </exception>
    public static int GetRequiredInt(this IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (!int.TryParse(value, out int result))
            throw new InvalidOperationException(
                $"Configuration value for '{key}' must be a valid integer. Found: '{value}'.");

        return result;
    }

    /// <summary>
    /// Retrieves the required boolean value for the specified configuration key.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="key">The key of the configuration value.</param>
    /// <returns>The parsed boolean value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the value is missing or cannot be parsed as a valid boolean.
    /// </exception>
    public static bool GetRequiredBool(this IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (!bool.TryParse(value, out bool result))
            throw new InvalidOperationException(
                $"Configuration value for '{key}' must be a valid boolean. Found: '{value}'.");

        return result;
    }
}