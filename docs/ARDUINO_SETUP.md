# Arduino 设置指南

## 硬件要求

- Arduino Nano (或其他兼容板，如 Uno, Mega)
- USB 数据线
- LED 灯（可选，可以使用板载 LED）
- 小风扇或 LED 模拟风扇（可选）
- 面包板和跳线（可选）

## 硬件连接

### 基础版本（仅使用板载组件）

无需额外连接，固件将使用：
- Pin 13 (板载 LED) 作为状态指示灯
- 可以直接烧录和测试

### 完整版本（添加外部组件）

| 组件 | Arduino Pin | 说明 |
|------|-------------|------|
| 灯光控制 LED | Pin 8 | 通过 220Ω 电阻连接到 GND |
| 风扇/PWM LED | Pin 9 | PWM 输出，通过三极管驱动风扇 |
| 状态指示灯 | Pin 13 | 板载 LED，自动闪烁表示在线 |

### 接线图（外部 LED）

```
Arduino Pin 8 ---[220Ω]---[LED]---GND
Arduino Pin 9 ---[220Ω]---[LED]---GND (或通过三极管驱动风扇)
```

### 风扇驱动电路（可选）

```
Arduino Pin 9 ---[1kΩ]--- 2N2222 Base
                          2N2222 Emitter --- GND
                          2N2222 Collector --- Fan (-) 
VCC (5V/12V) --- Fan (+)
VCC --- [二极管] --- Fan (-)  (保护二极管)
```

## 软件设置

### 1. 安装 Arduino IDE

1. 从 [Arduino 官网](https://www.arduino.cc/en/software) 下载并安装 Arduino IDE
2. 安装完成后启动 IDE

### 2. 配置开发板

1. 连接 Arduino Nano 到计算机
2. 在 Arduino IDE 中:
   - **Tools > Board** > 选择 "Arduino Nano"
   - **Tools > Processor** > 选择 "ATmega328P" 或 "ATmega328P (Old Bootloader)"
     - 如果上传失败，尝试切换处理器选项
   - **Tools > Port** > 选择对应的 COM 端口 (如 COM3)

### 3. 烧录固件

1. 打开 `arduino/IotDeviceSimulator/IotDeviceSimulator.ino`
2. 点击"验证"按钮（对勾图标）检查代码是否有错误
3. 点击"上传"按钮（右箭头图标）烧录固件
4. 等待上传完成

### 4. 验证固件

上传成功后：
- 板载 LED (Pin 13) 应该每秒闪烁一次
- 这表示固件正在运行且设备在线

### 5. 测试串口通讯

1. 打开串口监视器: **Tools > Serial Monitor**
2. 设置波特率为 **115200**
3. 设置行结束符为 **No line ending**
4. 你应该能看到设备在线

## 驱动程序

### CH340 芯片（常见于便宜的 Arduino Nano 克隆板）

如果设备管理器中没有显示串口：
1. 下载 [CH340 驱动](http://www.wch.cn/download/CH341SER_EXE.html)
2. 安装驱动
3. 重新连接 Arduino

### FTDI 芯片（原装 Arduino Nano）

Windows 通常会自动安装驱动，如果没有：
1. 从 [FTDI 官网](https://ftdichip.com/drivers/vcp-drivers/) 下载驱动
2. 安装后重启计算机

## 常见问题

### 上传失败：avrdude: stk500_recv(): programmer is not responding

**解决方法**:
1. 尝试切换处理器选项（Old Bootloader）
2. 检查 USB 线缆是否支持数据传输（不是仅充电线）
3. 尝试不同的 USB 端口
4. 按住 Reset 按钮，在上传过程中释放

### 找不到 COM 端口

**解决方法**:
1. 检查设备管理器，查看是否有未知设备
2. 安装对应的驱动程序（CH340 或 FTDI）
3. 尝试不同的 USB 端口
4. 重启计算机

### LED 不亮

**解决方法**:
1. 检查 LED 极性（长脚为正极）
2. 确认电阻连接正确（220Ω-1kΩ）
3. 使用万用表测试 Pin 8 输出电压
4. 尝试使用板载 LED (Pin 13) 进行测试

### 风扇不转

**解决方法**:
1. 检查风扇电源电压
2. 确认三极管连接正确
3. 测试 Pin 9 PWM 输出
4. 尝试直接将风扇连接到 5V 测试风扇是否正常

## 自定义固件

### 修改引脚定义

在 `.ino` 文件顶部修改：

```cpp
const int LED_PIN = 8;    // 改为你的 LED 引脚
const int FAN_PIN = 9;    // 改为你的风扇引脚
```

### 添加真实传感器（DHT22）

1. 安装 DHT 库: **Tools > Manage Libraries** > 搜索 "DHT"
2. 修改代码添加传感器支持：

```cpp
#include "DHT.h"

#define DHTPIN 2     
#define DHTTYPE DHT22
DHT dht(DHTPIN, DHTTYPE);

void setup() {
  dht.begin();
  // ...
}

void updateSensors() {
  state.temperature = dht.readTemperature();
  state.humidity = dht.readHumidity();
  // ...
}
```

### 调整更新频率

修改 `UPDATE_INTERVAL` 常量：

```cpp
const unsigned long UPDATE_INTERVAL = 2000; // 2秒更新一次
```

## 下一步

固件烧录完成后，你可以：
1. 启动 WinUI 应用程序
2. 选择对应的 COM 端口
3. 连接并开始控制设备

详见主 [README.md](../README.md) 文件。
