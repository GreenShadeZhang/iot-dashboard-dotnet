using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IotDashboard.Core.Models;
using IotDashboard.Core.Services;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using System;

namespace IotDashboard.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private DeviceController? _deviceController;
    private readonly DispatcherQueue _dispatcherQueue;
    private System.Threading.Timer? _heartbeatTimer;

    [ObservableProperty]
    private string _selectedPort = "";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private double _temperature;

    [ObservableProperty]
    private double _humidity;

    [ObservableProperty]
    private bool _lightOn;

    [ObservableProperty]
    private int _fanSpeed;

    [ObservableProperty]
    private bool _isOnline;

    [ObservableProperty]
    private int _batteryLevel;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _lastUpdate = "Never";

    public ObservableCollection<string> AvailablePorts { get; } = new();

    public MainViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        RefreshPorts();
    }

    [RelayCommand]
    private void RefreshPorts()
    {
        var ports = SerialPortCommunication.GetAvailablePorts();
        AvailablePorts.Clear();
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }

        if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(SelectedPort))
        {
            SelectedPort = AvailablePorts[0];
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (IsConnected)
        {
            await DisconnectAsync();
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                StatusMessage = "Please select a port";
                return;
            }

            var communication = new SerialPortCommunication(SelectedPort);
            _deviceController = new DeviceController(communication);
            _deviceController.StateUpdated += OnStateUpdated;
            _deviceController.ErrorOccurred += OnErrorOccurred;

            var connected = await _deviceController.ConnectAsync();
            if (connected)
            {
                IsConnected = true;
                StatusMessage = $"Connected to {SelectedPort}";
                
                // Start heartbeat timer with error handling
                _heartbeatTimer = new System.Threading.Timer(async _ =>
                {
                    try
                    {
                        if (_deviceController != null && IsConnected)
                        {
                            await _deviceController.RequestStateAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Heartbeat error: {ex.Message}");
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            }
            else
            {
                StatusMessage = "Connection failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        try
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;

            if (_deviceController != null)
            {
                await _deviceController.DisconnectAsync();
                _deviceController.StateUpdated -= OnStateUpdated;
                _deviceController.ErrorOccurred -= OnErrorOccurred;
                _deviceController.Dispose();
                _deviceController = null;
            }

            IsConnected = false;
            IsOnline = false;
            StatusMessage = "Disconnected";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleLightAsync()
    {
        if (_deviceController == null || !IsConnected) return;

        try
        {
            var newState = !LightOn;
            await _deviceController.SetLightAsync(newState);
            // Note: LightOn will be updated when device confirms the state change
            // via OnStateUpdated event handler
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateFanSpeedAsync()
    {
        if (_deviceController == null || !IsConnected) return;

        try
        {
            await _deviceController.SetFanSpeedAsync(FanSpeed);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void OnStateUpdated(object? sender, DeviceState state)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Temperature = state.Temperature;
            Humidity = state.Humidity;
            LightOn = state.LightOn;
            FanSpeed = state.FanSpeed;
            IsOnline = state.IsOnline;
            BatteryLevel = state.BatteryLevel;
            LastUpdate = state.Timestamp.ToString("HH:mm:ss");
            
            if (IsOnline)
            {
                StatusMessage = "Device online";
            }
        });
    }

    private void OnErrorOccurred(object? sender, string error)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage = $"Error: {error}";
        });
    }
}
