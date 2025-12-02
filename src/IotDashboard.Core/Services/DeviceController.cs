using IotDashboard.Core.Interfaces;
using IotDashboard.Core.Models;

namespace IotDashboard.Core.Services;

/// <summary>
/// Device controller for managing IoT device communication
/// </summary>
public class DeviceController : IDisposable
{
    private readonly IDeviceCommunication _communication;
    private readonly List<byte> _receiveBuffer = new();
    private bool _disposed;

    public event EventHandler<DeviceState>? StateUpdated;
    public event EventHandler<string>? ErrorOccurred;

    public DeviceState CurrentState { get; private set; } = new();

    public DeviceController(IDeviceCommunication communication)
    {
        _communication = communication;
        _communication.DataReceived += OnDataReceived;
        _communication.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public async Task<bool> ConnectAsync()
    {
        return await _communication.ConnectAsync();
    }

    public async Task DisconnectAsync()
    {
        await _communication.DisconnectAsync();
    }

    public async Task SetLightAsync(bool on)
    {
        var command = new ProtocolCommand
        {
            Type = CommandType.SetLight,
            Data = new[] { (byte)(on ? 1 : 0) }
        };
        await _communication.SendAsync(command.ToBytes());
    }

    public async Task SetFanSpeedAsync(int speed)
    {
        if (speed < 0 || speed > 100)
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between 0 and 100");

        var command = new ProtocolCommand
        {
            Type = CommandType.SetFanSpeed,
            Data = new[] { (byte)speed }
        };
        await _communication.SendAsync(command.ToBytes());
    }

    public async Task RequestStateAsync()
    {
        var command = new ProtocolCommand
        {
            Type = CommandType.GetState,
            Data = Array.Empty<byte>()
        };
        await _communication.SendAsync(command.ToBytes());
    }

    public async Task RequestSensorDataAsync()
    {
        var command = new ProtocolCommand
        {
            Type = CommandType.GetSensorData,
            Data = Array.Empty<byte>()
        };
        await _communication.SendAsync(command.ToBytes());
    }

    public async Task SendHeartbeatAsync()
    {
        var command = new ProtocolCommand
        {
            Type = CommandType.Heartbeat,
            Data = Array.Empty<byte>()
        };
        await _communication.SendAsync(command.ToBytes());
    }

    private void OnDataReceived(object? sender, DataReceivedEventArgs e)
    {
        try
        {
            // Add received data to buffer
            _receiveBuffer.AddRange(e.Data);

            // Try to parse complete commands from buffer
            while (TryParseCommand(out var command))
            {
                if (command != null)
                {
                    ProcessCommand(command);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error processing data: {ex.Message}");
        }
    }

    private bool TryParseCommand(out ProtocolCommand? command)
    {
        command = null;

        // Look for start byte
        var startIndex = _receiveBuffer.IndexOf(ProtocolCommand.StartByte);
        if (startIndex < 0)
        {
            _receiveBuffer.Clear();
            return false;
        }

        // Remove bytes before start byte
        if (startIndex > 0)
        {
            _receiveBuffer.RemoveRange(0, startIndex);
        }

        // Need at least 5 bytes for a complete command
        if (_receiveBuffer.Count < 5)
            return false;

        // Get expected length
        var length = _receiveBuffer[1];
        var totalLength = length + 4; // Start + Length + Data + Checksum + End

        if (_receiveBuffer.Count < totalLength)
            return false;

        // Extract command bytes
        var commandBytes = _receiveBuffer.Take(totalLength).ToArray();
        _receiveBuffer.RemoveRange(0, totalLength);

        // Try to parse
        command = ProtocolCommand.FromBytes(commandBytes);
        return command != null; // Return true only if parsing succeeded
    }

    private void ProcessCommand(ProtocolCommand command)
    {
        switch (command.Type)
        {
            case CommandType.GetState:
                if (command.Data.Length >= 6)
                {
                    CurrentState.Temperature = command.Data[0];
                    CurrentState.Humidity = command.Data[1];
                    CurrentState.LightOn = command.Data[2] != 0;
                    CurrentState.FanSpeed = command.Data[3];
                    CurrentState.IsOnline = command.Data[4] != 0;
                    CurrentState.BatteryLevel = command.Data[5];
                    CurrentState.Timestamp = DateTime.Now;
                    StateUpdated?.Invoke(this, CurrentState);
                }
                break;

            case CommandType.GetSensorData:
                if (command.Data.Length >= 2)
                {
                    CurrentState.Temperature = command.Data[0];
                    CurrentState.Humidity = command.Data[1];
                    CurrentState.Timestamp = DateTime.Now;
                    StateUpdated?.Invoke(this, CurrentState);
                }
                break;

            case CommandType.Heartbeat:
                // Device is alive
                CurrentState.IsOnline = true;
                CurrentState.Timestamp = DateTime.Now;
                break;
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateEventArgs e)
    {
        if (!e.IsConnected)
        {
            CurrentState.IsOnline = false;
            StateUpdated?.Invoke(this, CurrentState);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _communication.DataReceived -= OnDataReceived;
        _communication.ConnectionStateChanged -= OnConnectionStateChanged;
        _communication.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
