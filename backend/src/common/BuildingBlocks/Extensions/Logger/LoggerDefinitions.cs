namespace BuildingBlocks.Extensions.Logger;

public static partial class LoggerDefinitions
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Operation started: {OperationName} at {StartTime}")]
    public static partial void OperationStarted(this ILogger logger, string operationName, DateTimeOffset startTime);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Operation completed: {OperationName} at {EndTime}")]
    public static partial void OperationCompleted(this ILogger logger, string operationName, DateTimeOffset endTime);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Process started: {ProcessName}. Details: {Details}")]
    public static partial void ProcessStarted(this ILogger logger, string processName, string details);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Process completed: {ProcessName}. Details: {Details}")]
    public static partial void ProcessCompleted(this ILogger logger, string processName, string details);

   
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Information,
        Message = "Successfully completed operation: {OperationName}.")]
    public static partial void SuccessOperation(this ILogger logger, string operationName);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Warning,
        Message = "Operation was canceled: {OperationName}.")]
    public static partial void OperationCanceled(this ILogger logger, string operationName);
    
}