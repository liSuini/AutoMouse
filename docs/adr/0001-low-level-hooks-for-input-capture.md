---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: 'b0076e01-0aab-4e8f-8f79-c9df552ee475'
  PropagateID: 'b0076e01-0aab-4e8f-8f79-c9df552ee475'
  ReservedCode1: 'e7719fbc-f458-4b98-8d3f-b353a142baa2'
  ReservedCode2: 'e7719fbc-f458-4b98-8d3f-b353a142baa2'
---

# ADR-0001: 使用低级钩子捕获全局输入

**日期：** 2026-09-21
**状态：** 已接受

## 背景

AutoMouse 需要在系统全局范围内捕获鼠标和键盘事件，不受当前活动窗口限制。Windows 提供多种输入捕获机制：

1. **WH_MOUSE_LL / WH_KEYBOARD_LL（低级钩子）**：在输入到达目标窗口前拦截，全局生效
2. **WH_MOUSE / WH_KEYBOARD（高级钩子）**：在目标窗口处理后才拦截，可能被应用过滤
3. **Raw Input API**：直接读取输入设备数据，最低延迟但接口复杂
4. **GetAsyncKeyState 轮询**：定时轮询按键状态，精度差且 CPU 开销高

## 决策

采用 **WH_MOUSE_LL + WH_KEYBOARD_LL 低级钩子**。

## 理由

- 全局捕获：在任何应用中操作都能被录制，满足办公自动化和游戏辅助场景
- 低延迟：在输入到达目标窗口前拦截，时间戳精度高
- Windows 原生支持：SetWindowsHookEx/UnhookWindowsHookEx，无需第三方库
- WPF Dispatcher 自动提供消息循环，钩子回调可正常运行
- 低级钩子比高级钩子更可靠，不会被目标应用过滤或吞掉事件

## 代价

- 需要管理钩子句柄的生命周期（安装/卸载）
- 低级钩子回调如果处理时间过长（超过系统超时），会被系统移除，需注意回调效率
- 录制时也会捕获工具自身的操作，需要在录制中忽略工具窗口区域或做事件过滤