using System.IO.Ports;
using System.Text;
using IotDashboard.Core.Interfaces;

namespace IotDashboard.Core.Services;

/// <summary>
/// Serial port communication implementation
/// </summary>
public class SerialPortCommunication : IDeviceCommunication
{
    private SerialPort? _serialPort;
    private readonly string _portName;
    private readonly int _baudRate;
    private bool _disposed;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    public bool IsConnected => _serialPort?.IsOpen ?? false;

    /// <summary>
    /// Initializes a new instance of SerialPortCommunication
    /// </summary>
    /// <param name="portName">COM port name (e.g., "COM3")</param>
    /// <param name="baudRate">Baud rate (default: 115200)</param>
    public SerialPortCommunication(string portName, int baudRate = 115200)
    {
        _portName = portName;
        _baudRate = baudRate;
    }

    /// <summary>
    /// Gets available COM ports
    /// </summary>
    public static string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                return true;
            }

            _serialPort = new SerialPort(_portName, _baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.DataReceived += OnSerialDataReceived;
            
            await Task.Run(() => _serialPort.Open());

            OnConnectionStateChanged(true, "Connected successfully");
            return true;
        }
        catch (Exception ex)
        {
            OnConnectionStateChanged(false, $"Connection failed: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                await Task.Run(() =>
                {
                    _serialPort.DataReceived -= OnSerialDataReceived;
                    _serialPort.Close();
                });
            }

            OnConnectionStateChanged(false, "Disconnected");
        }
        catch (Exception ex)
        {
            OnConnectionStateChanged(false, $"Disconnect error: {ex.Message}");
        }
    }

    public async Task SendAsync(byte[] data)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            throw new InvalidOperationException("Port is not open");
        }

        await Task.Run(() => _serialPort.Write(data, 0, data.Length));
    }

    public async Task SendCommandAsync(string command)
    {
        var data = Encoding.ASCII.GetBytes(command);
        await SendAsync(data);
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            return;

        try
        {
            var bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead > 0)
            {
                var buffer = new byte[bytesToRead];
                _serialPort.Read(buffer, 0, bytesToRead);
                
                DataReceived?.Invoke(this, new DataReceivedEventArgs
                {
                    Data = buffer,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            // Handle read errors
            Console.WriteLine($"Error reading serial data: {ex.Message}");
        }
    }

    private void OnConnectionStateChanged(bool isConnected, string? message)
    {
        ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs
        {
            IsConnected = isConnected,
            Message = message
        });
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_serialPort != null)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DataReceived -= OnSerialDataReceived;
                _serialPort.Close();
            }
            _serialPort.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
