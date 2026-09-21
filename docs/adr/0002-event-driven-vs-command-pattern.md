---
AIGC:
  ContentProducer: '001191110102MAD55U9H0F10002'
  ContentPropagator: '001191110102MAD55U9H0F10002'
  Label: '1'
  ProduceID: 'd594d2b1-5ddd-4480-9fd6-b3481a233f25'
  PropagateID: 'd594d2b1-5ddd-4480-9fd6-b3481a233f25'
  ReservedCode1: 'ec0686a4-a9c1-4f6d-b89f-01896c355ef0'
  ReservedCode2: 'ec0686a4-a9c1-4f6d-b89f-01896c355ef0'
---

# ADR-0002: 事件驱动架构而非命令模式

**日期：** 2026-09-21
**状态：** 已接受

## 背景

录制和回放需要一种数据结构来表示操作序列。两种候选方案：

1. **事件驱动**：每个输入事件直接记录为 InputEvent 对象，事件列表贯穿录制→存储→回放
2. **命令模式**：将每个操作抽象为 Command 对象（ClickCommand、MoveCommand、KeyCommand 等），Command 支持 Execute/序列化/反序列化

## 决策

采用 **事件驱动架构**。

## 理由

- 录制本身就是事件流，直接存储事件是最自然的表达
- JSON 序列化简单：InputEvent → JSON → InputEvent，无需额外抽象层
- 开发量更小：不需要为每种操作定义 Command 类和 Execute 方法
- 事件列表天然支持时间序列回放，按 Timestamp 调度即可
- 步骤编辑通过 DataGrid 直接修改 InputEvent 属性即可实现

## 代价

- 命令模式在复杂编辑场景（如事务性操作、撤销/重做）更有优势，但当前需求不需要
- 未来如果需要条件分支和循环逻辑，可能需要升级为命令模式或脚本引擎