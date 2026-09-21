---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '677ca077-e63c-4f14-bea1-7343ea890a28'
  PropagateID: '677ca077-e63c-4f14-bea1-7343ea890a28'
  ReservedCode1: '556a1c40-e571-49f1-bcdc-efc56dc6ae3f'
  ReservedCode2: '556a1c40-e571-49f1-bcdc-efc56dc6ae3f'
---

# ADR-0003: JSON 作为脚本存储格式

**日期：** 2026-09-21
**状态：** 已接受

## 背景

脚本需要持久化存储，候选格式：

1. **JSON**：文本格式，可读性强，System.Text.Json 原生支持
2. **XML**：WPF 原生支持但冗长，可读性差
3. **二进制**：体积小但不可读，无法手动编辑
4. **自定义文本格式**：需自行实现解析器

## 决策

采用 **JSON 格式**，使用 System.Text.Json 序列化。

## 理由

- 可读性强：用户可直接用文本编辑器查看和手动修改脚本
- .NET 原生支持：System.Text.Json 性能好，无需第三方依赖
- 支持多态序列化：InputEvent 的子类 MouseEvent/KeyboardEvent 可通过自定义 Converter 处理
- 便于版本管理：JSON 文件可提交到 Git，diff 友好
- 扩展性好：未来增加新事件类型只需扩展 JSON 结构

## 代价

- 比二进制格式体积大（但录制脚本通常不大，可接受）
- 多态序列化需要自定义 JsonConverter 增加少量开发量