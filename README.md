# IoT Dashboard - 物联网监控仪表盘

一个基于 WinUI 3 的智能家居/物联网监控仪表盘应用程序，用于展示串口数据传输和可视化。

## 功能特性

- **环境传感器监控**
  - 实时温度监测（°C）
  - 实时湿度监测（%）
  
- **设备控制**
  - 智能灯光开关控制
  - 风扇速度调节（0-100%）
  
- **设备状态**
  - 在线/离线状态显示
  - 电池电量监控
  - 最后更新时间

- **通讯协议**
  - 基于串口（Serial Port）的设备通讯
  - 自定义二进制协议，支持校验和
  - 可扩展的接口设计，便于替换为 WebSocket 等其他通讯方式

## 项目结构

```
iot-dashboard-dotnet/
├── src/
│   ├── IotDashboard.App/          # WinUI 3 应用程序
│   │   ├── ViewModels/            # MVVM 视图模型
│   │   ├── Converters/            # XAML 值转换器
│   │   ├── MainWindow.xaml        # 主窗口界面
│   │   └── App.xaml               # 应用程序入口
│   └── IotDashboard.Core/         # 核心库
│       ├── Interfaces/            # 通讯接口抽象
│       ├── Models/                # 数据模型
│       └── Services/              # 服务实现
├── arduino/
│   └── IotDeviceSimulator/        # Arduino Nano 测试固件
└── README.md
```

## 技术栈

- **前端**: WinUI 3 (Windows App SDK)
- **.NET**: .NET 8.0
- **MVVM**: CommunityToolkit.Mvvm
- **串口通讯**: System.IO.Ports
- **硬件**: Arduino Nano

## 快速开始

### 前置要求

- Windows 10 版本 1809 (Build 17763) 或更高版本
- Visual Studio 2022 (带 Windows App SDK 工作负载)
- .NET 8.0 SDK
- Arduino IDE (用于烧录固件)
- Arduino Nano 或兼容板

### 安装步骤

#### 1. 克隆仓库

```bash
git clone https://github.com/GreenShadeZhang/iot-dashboard-dotnet.git
cd iot-dashboard-dotnet
```

#### 2. 构建应用程序

```bash
dotnet restore
dotnet build
```

#### 3. 运行应用程序

```bash
cd src/IotDashboard.App
dotnet run
```

或者在 Visual Studio 中打开 `IotDashboard.sln` 并按 F5 运行。

### Arduino 固件烧录

#### 硬件连接

- **LED (灯光)**: 连接到 Pin 8 (或使用内置 LED Pin 13)
- **风扇/PWM输出**: 连接到 Pin 9
- **状态指示灯**: 内置 LED Pin 13

#### 烧录步骤

1. 打开 Arduino IDE
2. 打开 `arduino/IotDeviceSimulator/IotDeviceSimulator.ino` 文件
3. 选择开发板: **Tools > Board > Arduino Nano**
4. 选择处理器: **Tools > Processor > ATmega328P** (或 ATmega328P Old Bootloader)
5. 选择串口: **Tools > Port** > 选择对应的 COM 端口
6. 点击上传按钮（或按 Ctrl+U）

#### 验证固件

上传成功后，内置 LED 应该会闪烁，表示设备在线。

## 使用说明

### 连接设备

1. 启动 WinUI 应用程序
2. 在顶部工具栏选择 Arduino 的 COM 端口（例如 COM3）
3. 点击"Refresh"按钮刷新可用端口
4. 点击"Connect"按钮连接设备
5. 连接成功后，状态栏会显示"Connected to COMx"

### 控制设备

- **灯光控制**: 点击"Turn On/Off"按钮切换灯光状态
- **风扇控制**: 
  1. 拖动滑块调整风扇速度（0-100%）
  2. 点击"Apply Speed"按钮发送命令
- **传感器数据**: 每2秒自动更新一次温度和湿度数据

### 断开连接

点击"Disconnect"按钮断开与设备的连接。

## 串口通讯协议

### 数据包格式

```
[StartByte][Length][Type][Data...][Checksum][EndByte]
```

- **StartByte**: `0xAA` - 起始字节
- **Length**: 数据长度（Type + Data 字节数）
- **Type**: 命令类型
- **Data**: 可变长度的数据载荷
- **Checksum**: 校验和（Length, Type, Data 字节的 XOR）
- **EndByte**: `0x55` - 结束字节

### 命令类型

| 命令 | 值 | 说明 | 数据格式 |
|------|-----|------|----------|
| GET_STATE | 0x01 | 获取设备状态 | 返回: [温度][湿度][灯光][风扇速度][在线][电量] |
| SET_LIGHT | 0x02 | 设置灯光 | 发送: [0/1] (关/开) |
| SET_FAN_SPEED | 0x03 | 设置风扇速度 | 发送: [速度值 0-100] |
| GET_SENSOR_DATA | 0x04 | 获取传感器数据 | 返回: [温度][湿度] |
| RESET | 0x05 | 重置设备 | 无数据 |
| HEARTBEAT | 0x06 | 心跳包 | 无数据 |

### 示例

设置灯光打开:
```
0xAA 0x02 0x02 0x01 0x01 0x55
```
- StartByte: 0xAA
- Length: 0x02 (Type + Data)
- Type: 0x02 (SET_LIGHT)
- Data: 0x01 (ON)
- Checksum: 0x02 XOR 0x02 XOR 0x01 = 0x01
- EndByte: 0x55

## 架构设计

### 接口抽象

`IDeviceCommunication` 接口提供了通讯方法的抽象，便于替换不同的通讯方式：

```csharp
public interface IDeviceCommunication : IDisposable
{
    event EventHandler<DataReceivedEventArgs>? DataReceived;
    event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;
    
    bool IsConnected { get; }
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task SendAsync(byte[] data);
    Task SendCommandAsync(string command);
}
```

### 实现类

- **SerialPortCommunication**: 串口通讯实现
- 未来可以添加: **WebSocketCommunication**, **TcpCommunication** 等

### MVVM 模式

应用程序使用 MVVM 模式，使用 CommunityToolkit.Mvvm 简化实现：

- **MainViewModel**: 主界面的视图模型
- **RelayCommand**: 命令绑定
- **ObservableProperty**: 属性变化通知

## 扩展开发

### 添加新的通讯方式

1. 实现 `IDeviceCommunication` 接口
2. 在 ViewModel 中替换通讯实例

示例（WebSocket）:

```csharp
public class WebSocketCommunication : IDeviceCommunication
{
    // 实现接口方法
    public async Task<bool> ConnectAsync() { ... }
    public async Task SendAsync(byte[] data) { ... }
    // ...
}

// 在 ViewModel 中使用
var communication = new WebSocketCommunication("ws://localhost:8080");
_deviceController = new DeviceController(communication);
```

### 添加新的传感器

1. 在 `DeviceState` 模型中添加新属性
2. 更新 Arduino 固件以发送新数据
3. 更新协议解析逻辑
4. 在 UI 中添加显示控件

## 参考项目

- [ElectronBot.DotNet](https://github.com/maker-community/ElectronBot.DotNet) - WinUI 框架参考
- [Letianpai MCU](https://github.com/Letianpai-Robot/MCU/tree/main/L81_MCU_PVT) - 串口协议参考

## 故障排除

### 无法找到 COM 端口
- 确保 Arduino 已正确连接到计算机
- 检查设备管理器中的端口
- 安装 CH340 或 FTDI 驱动程序（如果需要）

### 连接失败
- 确认选择了正确的 COM 端口
- 关闭其他占用串口的程序（如 Arduino IDE 串口监视器）
- 检查波特率设置（默认 115200）

### 设备不响应
- 重新烧录 Arduino 固件
- 检查硬件连接
- 重启应用程序和 Arduino

### 编译错误
- 确保安装了 .NET 8.0 SDK
- 确保安装了 Windows App SDK
- 运行 `dotnet restore` 恢复 NuGet 包

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件

## 贡献

欢迎提交 Pull Request 或创建 Issue！

## 作者

GreenShadeZhang

## 更新日志

### v1.0.0 (2024-12-02)
- 初始版本发布
- 实现串口通讯
- 实现基础设备控制（灯光、风扇）
- 实现传感器数据监控（温度、湿度）
- Arduino Nano 测试固件
