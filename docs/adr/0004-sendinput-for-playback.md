---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: '3c65eceb-9faf-4b99-8f6c-4706065903b1'
  PropagateID: '3c65eceb-9faf-4b99-8f6c-4706065903b1'
  ReservedCode1: '22fdb9db-673a-4300-a850-1d46da436b88'
  ReservedCode2: '22fdb9db-673a-4300-a850-1d46da436b88'
---

# ADR-0004: 使用 SendInput API 模拟输入

**日期：** 2026-09-21
**状态：** 已接受

## 背景

回放需要模拟鼠标和键盘操作，候选方案：

1. **SendInput API**：Windows 原生输入模拟 API，支持鼠标和键盘
2. **mouse_event / keybd_event**：已过时的 API，Microsoft 建议用 SendInput 替代
3. **UI Automation**：更高层但延迟高，不适合精确回放
4. **第三方库（InputSimulator 等）**：本质也是封装 SendInput

## 决策

采用 **SendInput Win32 API**，自行封装 P/Invoke 调用。

## 理由

- Windows 官方推荐的输入模拟 API
- 支持鼠标移动、点击、滚轮和键盘按键的统一接口
- 可批量发送多个输入事件（INPUT 数组），减少调用开销
- 延迟低，适合按 Timestamp 精确调度
- 自行封装避免第三方依赖，控制力更强

## 代价

- 需要 P/Invoke 声明 INPUT 结构体和 SendInput 函数
- 坐标模式需要处理（绝对坐标 vs 相对坐标），选择绝对坐标以确保回放准确性