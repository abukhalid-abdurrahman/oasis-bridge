namespace Common.Enums;

/// <summary>
/// Enum representing the possible statuses of a blockchain transaction.
/// This is used to track the current state of a transaction in the bridge system.
/// </summary>
public enum BridgeTransactionStatus
{
    /// <summary>
    /// Indicates that the transaction has completed successfully.
    /// </summary>
    Succeed,

    /// <summary>
    /// Indicates that the transaction has failed.
    /// This could occur due to various reasons such as insufficient funds or network issues.
    /// </summary>
    Failed,

    /// <summary>
    /// Indicates that the transaction is currently in progress.
    /// The transaction is being processed but has not yet completed.
    /// </summary>
    InProgress,

    /// <summary>
    /// Indicates that the transaction could not be found.
    /// This could be due to an invalid transaction ID or the transaction not existing on the blockchain.
    /// </summary>
    NotFound
}