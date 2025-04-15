namespace BuildingBlocks.Extensions.Logger;

public static partial class LoggerDefinitions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Operation started: {OperationName} at {StartTime}")]
    public static partial void OperationStarted(
        this ILogger logger,
        string operationName,
        DateTimeOffset startTime);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Operation completed: {OperationName} at {EndTime}. Duration: {Elapsed}")]
    public static partial void OperationCompleted(
        this ILogger logger,
        string operationName,
        DateTimeOffset endTime,
        TimeSpan elapsed);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Exception in {OperationName}.\nMessages:{Message}")]
    public static partial void OperationException(this ILogger logger, string operationName, string message);
}