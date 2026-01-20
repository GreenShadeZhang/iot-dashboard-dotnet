# 系统架构

## 总体架构

```
┌─────────────────────────────────────────────────────────────┐
│                    IoT Dashboard System                     │
├──────────────────────────┬──────────────────────────────────┤
│      WinUI 3 App         │       Arduino Device             │
│  (PC Side - Windows)     │    (MCU Side - Embedded)         │
└──────────────────────────┴──────────────────────────────────┘
```

## WinUI 应用架构

### 分层架构

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ MainWindow   │  │ ViewModels   │  │ Converters   │  │
│  │  (XAML/UI)   │  │   (MVVM)     │  │    (UI)      │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
├─────────────────────────────────────────────────────────┤
│                   Business Logic Layer                  │
│  ┌──────────────────────────────────────────────────┐   │
│  │         DeviceController (Core Logic)            │   │
│  │  - Protocol handling                             │   │
│  │  - State management                              │   │
│  │  - Command processing                            │   │
│  └──────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────┤
│                   Abstraction Layer                     │
│  ┌──────────────────────────────────────────────────┐   │
│  │      IDeviceCommunication (Interface)            │   │
│  │  + ConnectAsync()                                │   │
│  │  + DisconnectAsync()                             │   │
│  │  + SendAsync()                                   │   │
│  │  + event DataReceived                            │   │
│  └──────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────┤
│                  Implementation Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Serial     │  │  WebSocket   │  │    TCP/IP    │  │
│  │ Communication│  │Communication │  │Communication │  │
│  │ (Implemented)│  │  (Future)    │  │  (Future)    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### 数据流

```
用户操作 (UI Event)
    ↓
ViewModel (Command)
    ↓
DeviceController (Business Logic)
    ↓
ProtocolCommand (Encoding)
    ↓
IDeviceCommunication (Send)
    ↓
SerialPort (Hardware I/O)
    ↓
Arduino Device
    ↓
Response Data
    ↓
SerialPort (Receive)
    ↓
IDeviceCommunication (DataReceived Event)
    ↓
DeviceController (Protocol Parsing)
    ↓
DeviceState (Model Update)
    ↓
ViewModel (State Update)
    ↓
UI Update (Data Binding)
```

## Arduino 固件架构

### 主循环结构

```
Setup Phase
    ↓
    ├── Initialize Serial (115200 baud)
    ├── Initialize GPIO Pins
    ├── Initialize Device State
    └── Startup Indication
    ↓
Main Loop
    ├── Update Sensors (Every 1s)
    │   ├── Simulate Temperature
    │   ├── Simulate Humidity
    │   └── Update Battery Level
    │
    ├── Process Serial Data
    │   ├── Buffer Management
    │   ├── Packet Detection
    │   ├── Checksum Verification
    │   └── Command Processing
    │
    └── Status Indication (LED Blink)
```

### 命令处理流程

```
Serial Data Received
    ↓
Add to Buffer
    ↓
Look for Start Byte (0xAA)
    ↓
Wait for Complete Packet
    ↓
Verify End Byte (0x55)
    ↓
Calculate & Verify Checksum
    ↓
Parse Command Type
    ↓
    ├── GET_STATE → Send current state
    ├── SET_LIGHT → Update LED, Send state
    ├── SET_FAN_SPEED → Update PWM, Send state
    ├── GET_SENSOR_DATA → Send sensor data
    ├── RESET → Reset device, Send state
    └── HEARTBEAT → Send heartbeat response
```

## 通讯协议栈

### 协议层次

```
┌──────────────────────────────────────┐
│      Application Layer               │
│  (DeviceState, Commands)             │
├──────────────────────────────────────┤
│      Protocol Layer                  │
│  (ProtocolCommand - Encoding/Decode) │
├──────────────────────────────────────┤
│      Transport Layer                 │
│  (IDeviceCommunication)              │
├──────────────────────────────────────┤
│      Physical Layer                  │
│  (Serial Port, USB-TTL)              │
└──────────────────────────────────────┘
```

### 数据包格式

```
Byte 0: [0xAA] Start Marker
Byte 1: [Length] Payload length
Byte 2: [Type] Command type
Byte 3-n: [Data...] Variable data
Byte n+1: [Checksum] XOR checksum
Byte n+2: [0x55] End Marker
```

## MVVM 模式实现

```
┌─────────────────────────────────────────────────┐
│                    View                         │
│              (MainWindow.xaml)                  │
│  - UI Controls                                  │
│  - Data Binding: {x:Bind ViewModel.Property}   │
│  - Command Binding: {x:Bind ViewModel.Command} │
└────────────────┬────────────────────────────────┘
                 │
                 │ Binding
                 ↓
┌─────────────────────────────────────────────────┐
│                 ViewModel                       │
│            (MainViewModel.cs)                   │
│  - ObservableProperty                           │
│  - RelayCommand                                 │
│  - State Management                             │
│  - UI Logic                                     │
└────────────────┬────────────────────────────────┘
                 │
                 │ Uses
                 ↓
┌─────────────────────────────────────────────────┐
│                   Model                         │
│         (DeviceState, DeviceController)         │
│  - Business Logic                               │
│  - Data Models                                  │
│  - Device Communication                         │
└─────────────────────────────────────────────────┘
```

## 事件驱动架构

```
Arduino Device
    │
    │ (Serial Data)
    ↓
SerialPort.DataReceived Event
    │
    ↓
IDeviceCommunication.DataReceived Event
    │
    ↓
DeviceController Processing
    │
    ↓
DeviceController.StateUpdated Event
    │
    ↓
ViewModel.OnStateUpdated Handler
    │
    ↓
DispatcherQueue (UI Thread)
    │
    ↓
Update Observable Properties
    │
    ↓
INotifyPropertyChanged.PropertyChanged Event
    │
    ↓
UI Auto-Update (via Binding)
```

## 线程模型

```
┌─────────────────┐
│   UI Thread     │ ← Main application thread
│  (Dispatcher)   │ ← All UI updates must happen here
└────────┬────────┘
         │
         │ Commands / Events
         ↓
┌─────────────────┐
│ Worker Thread   │ ← Serial communication
│ (ThreadPool)    │ ← DeviceController logic
└────────┬────────┘
         │
         │ DispatcherQueue.TryEnqueue
         ↓
┌─────────────────┐
│   UI Thread     │ ← Safe UI updates
│  (Dispatcher)   │
└─────────────────┘
```

## 扩展点

### 1. 添加新的通讯方式

```csharp
public class WebSocketCommunication : IDeviceCommunication
{
    // 实现接口
}
```

### 2. 添加新的传感器

```csharp
// 在 DeviceState 中添加属性
public class DeviceState
{
    public int NewSensorValue { get; set; }
}

// 在 Arduino 中添加读取逻辑
void updateSensors() {
    state.newSensorValue = readNewSensor();
}
```

### 3. 添加新的控制命令

```csharp
// 添加命令类型
public enum CommandType
{
    NewCommand = 0x07
}

// 添加处理逻辑
public async Task ExecuteNewCommandAsync(int value)
{
    var command = new ProtocolCommand
    {
        Type = CommandType.NewCommand,
        Data = new[] { (byte)value }
    };
    await _communication.SendAsync(command.ToBytes());
}
```

## 安全考虑

1. **输入验证**: 所有来自设备的数据都需要验证
2. **校验和**: 使用 XOR 校验和检测传输错误
3. **超时处理**: 避免无限等待
4. **异常处理**: 捕获并处理所有可能的异常
5. **资源释放**: 正确实现 IDisposable

## 性能优化

1. **异步操作**: 所有 I/O 操作使用 async/await
2. **事件节流**: 限制 UI 更新频率
3. **缓冲区管理**: 避免频繁的内存分配
4. **数据绑定**: 使用 x:Bind 替代 Binding（编译时绑定）

## 技术栈总结

| 层次 | 技术 |
|------|------|
| UI Framework | WinUI 3 / Windows App SDK |
| Programming Language | C# 10, C++ (Arduino) |
| .NET Runtime | .NET 8.0 |
| MVVM Framework | CommunityToolkit.Mvvm |
| Serial Communication | System.IO.Ports |
| Hardware | Arduino Nano (ATmega328P) |
| Protocol | Custom binary protocol |
| Build System | MSBuild / dotnet CLI |
