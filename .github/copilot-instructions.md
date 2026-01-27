# GitHub Copilot 指令集成

**版本**：1.0  
**最后更新**：2026-01-25  
**状态**：Active

> ⚖️ **绝对权威仅归属 ADR 正文**


## 一、回答规范
- 所有回答必须使用中文。
- 遇到架构相关问题，必须遵循 ADR 正文。
- 所有输出必须使用三态标记：
  - ✅ Allowed
  - ⚠️ Blocked
  - ❓ Uncertain
- 禁止模糊判断、绕过测试或破例。
- 输出必须附带 ADR 编号或 Agent 名称。

## 二、GitHub Copilot 指令集成
- ADR 是唯一权威，所有操作必须遵循 ADR 正文。
- 所有 Agent 输出必须三态标记。
- Specialist Agent 只负责其领域，不能越权修改 ADR。
- 所有输出必须返回到 Guardian，由 Guardian 统一输出。
- 维护、审核、更新等职责有专门 Agent。

## 三、子代理调用规则
- 必须优先调用与任务匹配的专用 Agent（如 documentation-maintainer、adr-reviewer、architecture-guardian 等）。
- 只有在没有合适 Agent 时才自行处理。

## 四、文档编写指令
- 文件路径示例：`./github/instructions/documentation.instructions.md`
- 文档必须用简体中文，结构、引用、风格、交叉链接等均有详细规范。
- 所有文档引用 ADR 时，需用仓库内相对路径，并优先引用宪法/治理类 ADR。
- 文件内应内嵌常用文档治理相关 ADR 快速链接和完整索引，便于直接引用。
- README、示例、FAQ、Guide 等文档的边界、声明、引用、格式均有详细约束。
- 发现冲突时，以 ADR 正文为准并修正辅导材料。

## 五、系统级 Agent 行为约束
- 必须持续推进任务直至彻底解决，不能提前终止。
- 每次操作前需详细规划，操作后需验证结果，发现问题要继续修正。
- 不能重复输出，不能遗漏边界情况，需多轮自查。
- 文件编辑时用 `...existing code...` 标记未变部分，避免重复。

**维护**：架构委员会  
**审核**：@douhuaa  
**状态**：✅ Active
