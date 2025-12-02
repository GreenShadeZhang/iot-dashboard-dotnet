# 贡献指南

感谢您对 IoT Dashboard 项目的关注！我们欢迎任何形式的贡献。

## 如何贡献

### 报告 Bug

如果您发现了 bug，请创建一个 Issue，包含以下信息：

1. **标题**: 简短描述问题
2. **环境**:
   - 操作系统版本
   - .NET 版本
   - Arduino 型号
3. **重现步骤**: 详细的重现步骤
4. **预期行为**: 您期望发生什么
5. **实际行为**: 实际发生了什么
6. **截图**: 如果适用，添加截图
7. **日志**: 相关的错误日志或堆栈跟踪

### 提出新功能

创建一个 Feature Request Issue，包含：

1. **问题描述**: 这个功能要解决什么问题
2. **建议方案**: 您建议的实现方式
3. **替代方案**: 其他可能的实现方式
4. **使用场景**: 实际的使用案例

### 提交代码

1. **Fork 仓库**
   ```bash
   # 在 GitHub 上点击 Fork 按钮
   git clone https://github.com/YOUR_USERNAME/iot-dashboard-dotnet.git
   cd iot-dashboard-dotnet
   ```

2. **创建分支**
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   ```

3. **进行更改**
   - 遵循现有的代码风格
   - 添加必要的注释
   - 更新相关文档
   - 如果可能，添加测试

4. **提交更改**
   ```bash
   git add .
   git commit -m "feat: add amazing feature"
   # 或
   git commit -m "fix: resolve issue with serial port"
   ```

   提交信息格式:
   - `feat:` 新功能
   - `fix:` Bug 修复
   - `docs:` 文档更新
   - `style:` 代码格式（不影响代码运行）
   - `refactor:` 重构
   - `test:` 测试相关
   - `chore:` 构建过程或辅助工具的变动

5. **推送到 GitHub**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **创建 Pull Request**
   - 访问您的 fork 仓库
   - 点击 "New Pull Request"
   - 填写 PR 描述
   - 等待代码审查

## 代码规范

### C# 代码

- 使用 4 个空格缩进
- 遵循 Microsoft C# 编码规范
- 使用有意义的变量名
- 添加 XML 文档注释

```csharp
/// <summary>
/// 连接到设备
/// </summary>
/// <returns>如果连接成功返回 true</returns>
public async Task<bool> ConnectAsync()
{
    // 实现代码
}
```

### Arduino 代码

- 使用 2 个空格缩进
- 遵循 Arduino 编码规范
- 添加清晰的注释

```cpp
// 初始化串口通讯
void setup() {
  Serial.begin(115200);
}
```

### XAML

- 使用 2 个空格缩进
- 合理使用空白行分隔逻辑区块
- 属性对齐方式保持一致

## 测试

在提交 PR 之前，请确保：

1. 代码能够成功编译
2. 现有功能没有被破坏
3. 新功能按预期工作
4. 更新了相关文档

### 运行测试

```bash
# 构建项目
dotnet build

# 运行测试（如果有）
dotnet test
```

## 文档

如果您的更改影响了用户使用方式，请更新以下文档：

- `README.md` - 主要使用说明
- `docs/PROTOCOL.md` - 协议文档
- `docs/ARDUINO_SETUP.md` - Arduino 设置指南
- `docs/DEVELOPMENT.md` - 开发指南

## 代码审查流程

1. 提交 PR 后，维护者会进行审查
2. 如果需要修改，会在 PR 中留下评论
3. 根据反馈进行修改并推送
4. 审查通过后，PR 将被合并

## 许可证

提交代码即表示您同意将代码以 MIT 许可证发布。

## 行为准则

- 尊重他人
- 建设性的反馈
- 欢迎新贡献者
- 保持专业态度

## 需要帮助？

如果您有任何问题：

1. 查看现有的 Issues 和 Discussions
2. 阅读文档
3. 创建新的 Issue 提问

## 优先级

以下类型的贡献特别受欢迎：

1. Bug 修复
2. 性能优化
3. 文档改进
4. 单元测试
5. 新的传感器支持
6. 新的通讯方式（WebSocket, MQTT 等）
7. UI/UX 改进
8. 国际化支持

感谢您的贡献！
