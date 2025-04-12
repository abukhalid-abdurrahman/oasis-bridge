namespace BuildingBlocks.Extensions.Log;

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
        EventId = 2000,
        Level = LogLevel.Error,
        Message = "Unexpected error occurred: {ErrorMessage}")]
    public static partial void UnhandledError(this ILogger logger, string errorMessage);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "{WarningMessage}")]
    public static partial void GeneralWarning(this ILogger logger, string warningMessage);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "{InfoMessage}")]
    public static partial void GeneralInformation(this ILogger logger, string infoMessage);

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

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Wallet already linked. UserId: {UserId}, Wallet: {WalletAddress}")]
    public static partial void WalletAlreadyLinked(this ILogger logger, Guid userId, string walletAddress);
}