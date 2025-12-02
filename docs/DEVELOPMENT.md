# 开发指南

## 开发环境设置

### 前置要求

- Windows 10 版本 1809 或更高
- Visual Studio 2022 (推荐) 或 Visual Studio Code
- .NET 8.0 SDK
- Windows App SDK

### Visual Studio 设置

1. 安装 Visual Studio 2022
2. 在安装器中选择以下工作负载：
   - .NET desktop development
   - Universal Windows Platform development
   - Windows application development (WinUI)

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/GreenShadeZhang/iot-dashboard-dotnet.git
cd iot-dashboard-dotnet

# 还原依赖
dotnet restore

# 构建核心库
dotnet build src/IotDashboard.Core

# 构建应用（需要 Windows）
dotnet build src/IotDashboard.App
```

## 项目架构

### 分层设计

```
┌─────────────────────────────────┐
│   Presentation Layer (WinUI)    │  MainWindow, ViewModels
├─────────────────────────────────┤
│   Business Logic Layer          │  DeviceController
├─────────────────────────────────┤
│   Data Access Layer             │  IDeviceCommunication
├─────────────────────────────────┤
│   Infrastructure Layer          │  SerialPortCommunication
└─────────────────────────────────┘
```

### 核心组件

#### IDeviceCommunication (接口)
- 定义通讯方法的契约
- 支持事件驱动的数据接收
- 异步操作

#### SerialPortCommunication (实现)
- 串口通讯的具体实现
- 处理串口打开、关闭、读写
- 线程安全

#### DeviceController (控制器)
- 业务逻辑层
- 协议解析和封装
- 状态管理

#### MainViewModel (视图模型)
- MVVM 模式
- UI 绑定
- 命令处理

## 添加新功能

### 1. 添加新的传感器类型

#### Step 1: 更新数据模型

在 `IotDashboard.Core/Models/DeviceState.cs` 中添加新属性：

```csharp
public class DeviceState
{
    // 现有属性...
    
    /// <summary>
    /// 新传感器: 光照强度 (0-100)
    /// </summary>
    public int LightIntensity { get; set; }
}
```

#### Step 2: 更新协议

在 `IotDashboard.Core/Models/ProtocolCommand.cs` 中添加新命令：

```csharp
public enum CommandType
{
    // 现有命令...
    
    /// <summary>
    /// 获取光照强度
    /// </summary>
    GetLightIntensity = 0x07
}
```

#### Step 3: 更新 DeviceController

在 `DeviceController.cs` 的 `ProcessCommand` 方法中添加处理逻辑：

```csharp
case CommandType.GetLightIntensity:
    if (command.Data.Length >= 1)
    {
        CurrentState.LightIntensity = command.Data[0];
        StateUpdated?.Invoke(this, CurrentState);
    }
    break;
```

#### Step 4: 更新 ViewModel

在 `MainViewModel.cs` 中添加属性：

```csharp
[ObservableProperty]
private int _lightIntensity;

private void OnStateUpdated(object? sender, DeviceState state)
{
    _dispatcherQueue.TryEnqueue(() =>
    {
        // 现有更新...
        LightIntensity = state.LightIntensity;
    });
}
```

#### Step 5: 更新 UI

在 `MainWindow.xaml` 中添加显示控件：

```xml
<StackPanel>
    <TextBlock Text="Light Intensity"/>
    <ProgressBar Value="{x:Bind ViewModel.LightIntensity, Mode=OneWay}" 
                 Maximum="100"/>
</StackPanel>
```

#### Step 6: 更新 Arduino 固件

在 Arduino 代码中添加传感器读取和响应。

### 2. 添加新的通讯方式（WebSocket）

#### Step 1: 创建新的通讯类

```csharp
public class WebSocketCommunication : IDeviceCommunication
{
    private ClientWebSocket? _webSocket;
    private readonly Uri _uri;
    
    public WebSocketCommunication(string url)
    {
        _uri = new Uri(url);
    }
    
    public async Task<bool> ConnectAsync()
    {
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(_uri, CancellationToken.None);
        // 开始接收循环...
        return true;
    }
    
    // 实现其他接口方法...
}
```

#### Step 2: 在 ViewModel 中使用

```csharp
// 替换 SerialPortCommunication
var communication = new WebSocketCommunication("ws://localhost:8080");
_deviceController = new DeviceController(communication);
```

## 调试技巧

### 1. 串口调试

启用详细日志：

```csharp
_communication.DataReceived += (s, e) =>
{
    Console.WriteLine($"Received: {BitConverter.ToString(e.Data)}");
};
```

### 2. 协议调试

在 `DeviceController` 中添加断点：
- `OnDataReceived` 方法
- `ProcessCommand` 方法

### 3. UI 调试

使用 Live Visual Tree 查看 XAML 层次结构：
- Debug > Windows > Live Visual Tree

### 4. 模拟设备

不连接 Arduino 时，可以创建模拟设备：

```csharp
public class MockCommunication : IDeviceCommunication
{
    private Timer? _timer;
    
    public Task<bool> ConnectAsync()
    {
        _timer = new Timer(_ =>
        {
            // 模拟接收数据
            var mockData = CreateMockData();
            DataReceived?.Invoke(this, new DataReceivedEventArgs 
            { 
                Data = mockData 
            });
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        
        return Task.FromResult(true);
    }
}
```

## 测试

### 单元测试示例

创建测试项目：

```bash
dotnet new xunit -n IotDashboard.Tests
dotnet add reference ../src/IotDashboard.Core
```

测试协议解析：

```csharp
[Fact]
public void ProtocolCommand_ToBytes_CreatesValidPacket()
{
    var command = new ProtocolCommand
    {
        Type = CommandType.SetLight,
        Data = new byte[] { 1 }
    };
    
    var bytes = command.ToBytes();
    
    Assert.Equal(0xAA, bytes[0]); // Start
    Assert.Equal(0x55, bytes[^1]); // End
}
```

## 性能优化

### 1. 减少 UI 更新频率

使用节流器避免过于频繁的 UI 更新：

```csharp
private DateTime _lastUiUpdate = DateTime.MinValue;

private void OnStateUpdated(object? sender, DeviceState state)
{
    var now = DateTime.Now;
    if ((now - _lastUiUpdate).TotalMilliseconds < 100)
        return; // 跳过太频繁的更新
        
    _lastUiUpdate = now;
    _dispatcherQueue.TryEnqueue(() => { /* 更新 UI */ });
}
```

### 2. 异步操作

所有 I/O 操作都应该是异步的：

```csharp
// 好的做法
await _communication.SendAsync(data);

// 避免
_communication.SendAsync(data).Wait();
```

### 3. 资源释放

确保正确实现 IDisposable：

```csharp
public void Dispose()
{
    _timer?.Dispose();
    _communication?.Dispose();
    GC.SuppressFinalize(this);
}
```

## 代码规范

### 命名约定

- 类名: PascalCase
- 方法名: PascalCase
- 私有字段: _camelCase
- 属性: PascalCase
- 常量: PascalCase

### 注释

使用 XML 文档注释：

```csharp
/// <summary>
/// 发送命令到设备
/// </summary>
/// <param name="command">要发送的命令</param>
/// <returns>如果成功返回 true</returns>
public async Task<bool> SendCommandAsync(ProtocolCommand command)
{
    // 实现...
}
```

## 发布

### 创建发布版本

```bash
dotnet publish src/IotDashboard.App -c Release -r win-x64 --self-contained
```

### 创建 MSIX 包

在 Visual Studio 中：
1. 右键点击项目
2. Publish > Create App Packages
3. 选择 Sideloading
4. 配置版本号和架构
5. Create

## 贡献指南

1. Fork 仓库
2. 创建功能分支: `git checkout -b feature/amazing-feature`
3. 提交更改: `git commit -m 'Add amazing feature'`
4. 推送到分支: `git push origin feature/amazing-feature`
5. 创建 Pull Request

## 资源链接

- [WinUI 3 文档](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [.NET MAUI vs WinUI](https://docs.microsoft.com/dotnet/maui/what-is-maui)
- [Serial Port 文档](https://docs.microsoft.com/dotnet/api/system.io.ports.serialport)
- [Arduino 参考](https://www.arduino.cc/reference/en/)
