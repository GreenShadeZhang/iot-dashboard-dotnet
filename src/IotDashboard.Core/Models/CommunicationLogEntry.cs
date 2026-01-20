namespace IotDashboard.Core.Models;

/// <summary>
/// Communication direction
/// </summary>
public enum CommunicationDirection
{
    Sent,
    Received
}

/// <summary>
/// Represents a communication log entry
/// </summary>
public class CommunicationLogEntry
{
    /// <summary>
    /// Timestamp of the communication
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Direction of communication
    /// </summary>
    public CommunicationDirection Direction { get; set; }

    /// <summary>
    /// Command type
    /// </summary>
    public CommandType CommandType { get; set; }

    /// <summary>
    /// Raw bytes
    /// </summary>
    public byte[] RawData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the formatted hex string of raw data
    /// </summary>
    public string HexString => BitConverter.ToString(RawData).Replace("-", " ");

    /// <summary>
    /// Gets the time formatted string
    /// </summary>
    public string TimeString => Timestamp.ToString("HH:mm:ss.fff");
}
