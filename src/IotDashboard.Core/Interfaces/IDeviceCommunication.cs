namespace IotDashboard.Core.Interfaces;

/// <summary>
/// Interface for device communication abstraction
/// Allows for easy replacement of communication methods (Serial, WebSocket, etc.)
/// </summary>
public interface IDeviceCommunication : IDisposable
{
    /// <summary>
    /// Event raised when data is received from the device
    /// </summary>
    event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// Gets a value indicating whether the connection is open
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Opens the connection to the device
    /// </summary>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Closes the connection to the device
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Sends data to the device
    /// </summary>
    /// <param name="data">Data to send</param>
    Task SendAsync(byte[] data);

    /// <summary>
    /// Sends a command string to the device
    /// </summary>
    /// <param name="command">Command to send</param>
    Task SendCommandAsync(string command);
}

/// <summary>
/// Event args for data received events
/// </summary>
public class DataReceivedEventArgs : EventArgs
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Event args for connection state changes
/// </summary>
public class ConnectionStateEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
}
