# 故障排除指南

## 常见问题及解决方案

### 应用程序问题

#### 1. 应用程序无法启动

**症状**: 双击应用程序后没有反应或立即崩溃

**可能原因及解决方案**:

- **缺少 Windows App SDK**
  ```
  解决方案: 安装 Windows App SDK Runtime
  下载地址: https://docs.microsoft.com/windows/apps/windows-app-sdk/downloads
  ```

- **操作系统版本过低**
  ```
  最低要求: Windows 10 版本 1809 (Build 17763)
  检查方法: Win + R → winver
  解决方案: 更新 Windows 系统
  ```

- **.NET Runtime 缺失**
  ```
  解决方案: 安装 .NET 8.0 Runtime
  下载地址: https://dotnet.microsoft.com/download/dotnet/8.0
  ```

#### 2. 编译错误

**症状**: 在 Visual Studio 中编译失败

**错误**: `NETSDK1100: To build a project targeting Windows...`
```
解决方案: 
1. 确保在 Windows 系统上编译
2. 安装 Windows SDK
3. 在 Visual Studio Installer 中安装 "Windows application development" 工作负载
```

**错误**: `The type or namespace name 'WindowsAppSDK' could not be found`
```
解决方案:
1. 运行 dotnet restore
2. 清理解决方案: Build > Clean Solution
3. 重新构建: Build > Rebuild Solution
```

### 串口通讯问题

#### 3. 找不到 COM 端口

**症状**: 下拉列表中没有显示任何 COM 端口

**解决步骤**:

1. **检查 Arduino 连接**
   ```
   - 确保 USB 线连接牢固
   - 尝试不同的 USB 端口
   - 检查 USB 线是否支持数据传输（不是仅充电线）
   ```

2. **检查设备管理器**
   ```
   Win + X → 设备管理器 → 端口 (COM 和 LPT)
   
   如果显示黄色感叹号:
   - 右键 → 更新驱动程序
   - 安装对应的驱动程序（CH340 或 FTDI）
   ```

3. **安装驱动程序**
   
   **CH340 驱动** (大多数克隆板):
   - 下载: http://www.wch.cn/download/CH341SER_EXE.html
   - 安装后重启计算机
   
   **FTDI 驱动** (原装板):
   - 下载: https://ftdichip.com/drivers/vcp-drivers/
   - 安装后重启计算机

4. **尝试手动刷新**
   ```
   点击应用程序中的 "Refresh" 按钮
   ```

#### 4. 连接失败

**症状**: 点击 "Connect" 后显示连接失败

**错误**: `Access to the port 'COMx' is denied`
```
原因: 端口被其他程序占用
解决方案:
1. 关闭 Arduino IDE 的串口监视器
2. 关闭其他串口终端程序
3. 重启应用程序
4. 如果问题持续，重启计算机
```

**错误**: `The port 'COMx' does not exist`
```
原因: Arduino 断开连接或端口号改变
解决方案:
1. 检查 USB 连接
2. 在设备管理器中确认端口号
3. 点击 Refresh 按钮更新端口列表
```

**错误**: `Timeout waiting for connection`
```
原因: Arduino 没有响应
解决方案:
1. 确认 Arduino 固件已正确烧录
2. 检查板载 LED 是否闪烁（表示固件运行）
3. 尝试重新烧录固件
4. 检查波特率是否为 115200
```

#### 5. 设备不响应

**症状**: 连接成功但设备显示为离线，或控制命令无效

**诊断步骤**:

1. **检查固件状态**
   ```
   - 板载 LED (Pin 13) 应该每秒闪烁一次
   - 如果不闪烁，说明固件没有正常运行
   ```

2. **使用 Arduino IDE 测试**
   ```
   Tools > Serial Monitor
   - 设置波特率: 115200
   - 设置 "No line ending"
   - 观察是否有数据输出
   ```

3. **检查波特率**
   ```
   确保 PC 端和 Arduino 端都使用 115200
   ```

4. **重新烧录固件**
   ```
   1. 打开 IotDeviceSimulator.ino
   2. 验证代码（对勾按钮）
   3. 上传到 Arduino（箭头按钮）
   4. 等待上传完成
   ```

### Arduino 问题

#### 6. 固件上传失败

**错误**: `avrdude: stk500_recv(): programmer is not responding`

**解决方案**:

1. **切换处理器类型**
   ```
   Tools > Processor
   尝试切换:
   - ATmega328P
   - ATmega328P (Old Bootloader)
   ```

2. **检查开发板选择**
   ```
   Tools > Board
   确保选择了正确的板型:
   - Arduino Nano
   - Arduino Uno (如果是 Uno)
   ```

3. **按住 Reset 技巧**
   ```
   1. 点击上传
   2. 等待出现 "Uploading..."
   3. 按下 Arduino 上的 Reset 按钮
   4. 立即释放
   ```

4. **使用 USBtinyISP 或其他编程器**
   ```
   如果 bootloader 损坏，可能需要使用外部编程器重新烧录
   ```

#### 7. LED 不亮

**症状**: 控制灯光后 LED 没有反应

**检查清单**:

1. **极性检查**
   ```
   LED 长脚(正极) → Arduino Pin 8
   LED 短脚(负极) → 电阻 → GND
   ```

2. **电阻值检查**
   ```
   使用 220Ω - 1kΩ 电阻
   电阻值太大会导致 LED 很暗
   ```

3. **引脚测试**
   ```
   使用万用表测量 Pin 8 电压:
   - 灯光打开时应该是 ~5V
   - 灯光关闭时应该是 ~0V
   ```

4. **尝试板载 LED**
   ```
   修改固件使用 Pin 13 (板载 LED) 进行测试:
   const int LED_PIN = 13;
   ```

#### 8. 风扇不转

**症状**: 调节风扇速度后风扇没有反应

**检查步骤**:

1. **PWM 输出测试**
   ```cpp
   // 在 Arduino 中添加测试代码
   void loop() {
     for(int i = 0; i <= 255; i += 5) {
       analogWrite(FAN_PIN, i);
       delay(100);
     }
   }
   ```

2. **检查电源**
   ```
   - Arduino 5V 只能驱动小型风扇
   - 大功率风扇需要外部电源 + 三极管驱动
   ```

3. **三极管电路检查**
   ```
   Pin 9 → 1kΩ 电阻 → 三极管 Base
   三极管 Emitter → GND
   三极管 Collector → 风扇负极
   外部电源 → 风扇正极
   ```

### 数据问题

#### 9. 传感器数据不准确

**症状**: 温度/湿度显示异常值

**说明**:
```
固件默认使用模拟数据，这是正常的。
温度范围: 15-35°C (随机波动)
湿度范围: 30-70% (随机波动)
```

**添加真实传感器**:

1. **使用 DHT22 传感器**
   ```
   连接:
   - VCC → 5V
   - GND → GND
   - DATA → Pin 2
   
   安装库:
   Tools > Manage Libraries > 搜索 "DHT sensor library"
   
   修改代码:
   #include "DHT.h"
   #define DHTPIN 2
   #define DHTTYPE DHT22
   DHT dht(DHTPIN, DHTTYPE);
   
   void updateSensors() {
     state.temperature = dht.readTemperature();
     state.humidity = dht.readHumidity();
   }
   ```

#### 10. 数据包损坏

**症状**: 偶尔出现通讯错误或无响应

**可能原因**:

1. **USB 线质量差**
   ```
   解决方案: 更换质量好的 USB 数据线
   ```

2. **电磁干扰**
   ```
   解决方案:
   - 远离电源适配器
   - 使用屏蔽 USB 线
   - 添加退耦电容
   ```

3. **波特率不稳定**
   ```
   解决方案:
   - 确保晶振准确（Arduino Nano 使用 16MHz）
   - 降低波特率到 57600 或 9600 测试
   ```

### 性能问题

#### 11. UI 响应缓慢

**症状**: 界面卡顿或更新延迟

**优化方案**:

1. **调整更新频率**
   ```csharp
   // 在 MainViewModel.cs 中
   _heartbeatTimer = new Timer(async _ => { ... },
       null, 
       TimeSpan.Zero, 
       TimeSpan.FromSeconds(5)); // 从 2 秒改为 5 秒
   ```

2. **添加 UI 更新节流**
   ```csharp
   private DateTime _lastUiUpdate = DateTime.MinValue;
   
   private void OnStateUpdated(object? sender, DeviceState state)
   {
       var now = DateTime.Now;
       if ((now - _lastUiUpdate).TotalMilliseconds < 200)
           return;
       _lastUiUpdate = now;
       // 更新 UI
   }
   ```

## 调试技巧

### 启用详细日志

在 `DeviceController.cs` 中添加:

```csharp
private void OnDataReceived(object? sender, DataReceivedEventArgs e)
{
    Console.WriteLine($"[RX] {BitConverter.ToString(e.Data)}");
    // 处理数据...
}

public async Task SendAsync(byte[] data)
{
    Console.WriteLine($"[TX] {BitConverter.ToString(data)}");
    await _communication.SendAsync(data);
}
```

### 使用串口监视器

同时运行 Arduino IDE 串口监视器查看原始数据：
```
注意: 只能在 IoT Dashboard 断开连接后使用
```

### 抓包分析

使用 USB 串口抓包工具（如 com0com）分析通讯。

## 获取帮助

如果以上方法都无法解决问题：

1. **查看 Issues**: https://github.com/GreenShadeZhang/iot-dashboard-dotnet/issues
2. **创建新 Issue**: 包含详细的错误信息、截图和日志
3. **查看文档**: 阅读 `docs/` 目录下的其他文档

## 重置到初始状态

如果一切都出问题了：

1. **重新克隆仓库**
   ```bash
   git clone https://github.com/GreenShadeZhang/iot-dashboard-dotnet.git
   ```

2. **重新烧录 Arduino**
   ```
   使用 Arduino IDE 烧录 IotDeviceSimulator.ino
   ```

3. **重新构建应用**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```
