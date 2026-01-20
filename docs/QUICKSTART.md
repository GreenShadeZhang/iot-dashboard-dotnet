# 快速开始指南

本指南将帮助您在 5 分钟内运行 IoT Dashboard。

## 第一步: 准备硬件 (2 分钟)

### 您需要:
- ✅ Arduino Nano (或 Uno)
- ✅ USB 数据线
- ✅ 一台 Windows 10/11 电脑

### 可选组件:
- 💡 LED 灯 + 220Ω 电阻
- 🌀 小风扇或额外的 LED

## 第二步: 烧录 Arduino (2 分钟)

1. **下载 Arduino IDE**
   - 访问: https://www.arduino.cc/en/software
   - 下载并安装

2. **连接 Arduino**
   - 用 USB 线连接 Arduino 到电脑

3. **烧录固件**
   ```
   a. 打开 Arduino IDE
   b. File → Open → 选择 arduino/IotDeviceSimulator/IotDeviceSimulator.ino
   c. Tools → Board → Arduino Nano
   d. Tools → Processor → ATmega328P (尝试 Old Bootloader 如果上传失败)
   e. Tools → Port → 选择你的 COM 端口
   f. 点击上传按钮 (→)
   g. 等待 "Done uploading"
   ```

4. **验证**
   - 板载 LED 应该每秒闪烁一次 ✨

## 第三步: 运行应用 (1 分钟)

### 选项 A: 从源码运行

```bash
# 克隆仓库
git clone https://github.com/GreenShadeZhang/iot-dashboard-dotnet.git
cd iot-dashboard-dotnet

# 运行 (需要 .NET 8.0 SDK)
cd src/IotDashboard.App
dotnet run
```

### 选项 B: 使用 Visual Studio

```
1. 打开 IotDashboard.sln
2. 按 F5 运行
```

## 第四步: 连接设备 (30 秒)

1. 在应用顶部选择 COM 端口 (例如 COM3)
2. 点击 **Connect** 按钮
3. 等待状态栏显示 "Connected to COMx"

🎉 **成功!** 您现在可以看到实时数据了！

## 开始使用

### 查看传感器数据
- **温度**: 实时显示 (15-35°C 模拟值)
- **湿度**: 实时显示 (30-70% 模拟值)
- 数据每 2 秒自动更新

### 控制灯光
1. 在 "Light Control" 卡片中
2. 点击 **Turn On/Off** 按钮
3. 观察板载 LED (Pin 13) 或外接 LED (Pin 8)

### 调节风扇
1. 在 "Fan Control" 卡片中
2. 拖动滑块选择速度 (0-100%)
3. 点击 **Apply Speed** 按钮

## 硬件连接 (可选 - 增强体验)

### 添加外部 LED (灯光控制)

```
Arduino Pin 8 → [220Ω 电阻] → LED 正极(长脚)
LED 负极(短脚) → GND
```

### 添加风扇或 LED (风扇控制)

```
Arduino Pin 9 → [220Ω 电阻] → LED 正极
LED 负极 → GND
```

或使用三极管驱动真实风扇:
```
Arduino Pin 9 → [1kΩ] → 2N2222 Base
2N2222 Emitter → GND
2N2222 Collector → Fan 负极
外部电源 (+5V/12V) → Fan 正极
```

## 常见问题快速解决

### 找不到 COM 端口?
```
→ 点击 "Refresh" 按钮
→ 检查 USB 连接
→ 安装驱动: CH340 或 FTDI
```

### 连接失败?
```
→ 关闭 Arduino IDE 串口监视器
→ 重新选择端口
→ 重启应用
```

### 设备离线?
```
→ 检查 Arduino LED 是否闪烁
→ 重新烧录固件
→ 尝试按 Arduino 上的 Reset 按钮
```

### 上传固件失败?
```
→ 切换 Processor: Tools → Processor → ATmega328P (Old Bootloader)
→ 检查 Board: 确保选择了 Arduino Nano
→ 尝试不同的 USB 端口
```

## 下一步

现在您已经成功运行了基础功能，可以：

1. 📚 阅读 [完整文档](../README.md)
2. 🔧 添加真实的温湿度传感器 (DHT22)
3. 💡 连接更多 LED 和设备
4. 🚀 学习如何[扩展功能](DEVELOPMENT.md)
5. 🐛 遇到问题查看[故障排除](TROUBLESHOOTING.md)

## 支持

- 🐛 报告问题: [GitHub Issues](https://github.com/GreenShadeZhang/iot-dashboard-dotnet/issues)
- 💬 讨论交流: [GitHub Discussions](https://github.com/GreenShadeZhang/iot-dashboard-dotnet/discussions)
- 📖 查看文档: [docs/](.)

---

**提示**: 如果这个快速指南对您有帮助，请给项目一个 ⭐ Star！
