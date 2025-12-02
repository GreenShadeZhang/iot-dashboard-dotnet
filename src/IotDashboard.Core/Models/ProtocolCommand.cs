namespace IotDashboard.Core.Models;

/// <summary>
/// Command types supported by the protocol
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Get current device state
    /// </summary>
    GetState = 0x01,

    /// <summary>
    /// Set light on/off
    /// </summary>
    SetLight = 0x02,

    /// <summary>
    /// Set fan speed
    /// </summary>
    SetFanSpeed = 0x03,

    /// <summary>
    /// Get sensor data
    /// </summary>
    GetSensorData = 0x04,

    /// <summary>
    /// Reset device
    /// </summary>
    Reset = 0x05,

    /// <summary>
    /// Heartbeat/ping
    /// </summary>
    Heartbeat = 0x06
}

/// <summary>
/// Serial protocol command structure
/// </summary>
public class ProtocolCommand
{
    /// <summary>
    /// Start byte (0xAA)
    /// </summary>
    public const byte StartByte = 0xAA;

    /// <summary>
    /// End byte (0x55)
    /// </summary>
    public const byte EndByte = 0x55;

    /// <summary>
    /// Command type
    /// </summary>
    public CommandType Type { get; set; }

    /// <summary>
    /// Data payload
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Converts the command to byte array for transmission
    /// Format: [StartByte][Length][Type][Data...][Checksum][EndByte]
    /// </summary>
    public byte[] ToBytes()
    {
        var length = (byte)(Data.Length + 1); // Type + Data
        var buffer = new List<byte>
        {
            StartByte,
            length,
            (byte)Type
        };
        
        buffer.AddRange(Data);
        
        // Calculate checksum (simple XOR of all bytes)
        byte checksum = length;
        checksum ^= (byte)Type;
        foreach (var b in Data)
        {
            checksum ^= b;
        }
        
        buffer.Add(checksum);
        buffer.Add(EndByte);
        
        return buffer.ToArray();
    }

    /// <summary>
    /// Parses a byte array into a command
    /// </summary>
    public static ProtocolCommand? FromBytes(byte[] bytes)
    {
        if (bytes.Length < 5) return null; // Minimum: Start + Length + Type + Checksum + End
        if (bytes[0] != StartByte) return null;
        if (bytes[^1] != EndByte) return null;

        var length = bytes[1];
        var type = (CommandType)bytes[2];
        var dataLength = length - 1;
        var data = new byte[dataLength];
        Array.Copy(bytes, 3, data, 0, dataLength);

        // Verify checksum
        byte checksum = length;
        checksum ^= bytes[2];
        for (int i = 0; i < dataLength; i++)
        {
            checksum ^= data[i];
        }

        if (checksum != bytes[3 + dataLength]) return null;

        return new ProtocolCommand
        {
            Type = type,
            Data = data
        };
    }
}
