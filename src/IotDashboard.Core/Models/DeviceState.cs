namespace IotDashboard.Core.Models;

/// <summary>
/// Represents the state of a device
/// </summary>
public class DeviceState
{
    /// <summary>
    /// Timestamp of the state
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Humidity in percentage
    /// </summary>
    public double Humidity { get; set; }

    /// <summary>
    /// Light state (on/off)
    /// </summary>
    public bool LightOn { get; set; }

    /// <summary>
    /// Fan speed (0-100)
    /// </summary>
    public int FanSpeed { get; set; }

    /// <summary>
    /// Device online status
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Battery level (0-100)
    /// </summary>
    public int BatteryLevel { get; set; }

    /// <summary>
    /// Additional device information
    /// </summary>
    public string? DeviceInfo { get; set; }
}
