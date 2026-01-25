# GitHub Copilot Agents 体系

**版本**：1.0  
**最后更新**：2026-01-25  
**状态**：Active

---

## 一、Agents 定位

### 核心概念

> **Agent ≠ Copilot 本体**  
> **Agent = 在特定职责下工作的 Copilot 角色**

```
Agent = Instructions + 特定职责域 + ADR 约束视角
```

### 在治理体系中的位置

```
ADR（宪法）
  ↓
Instructions（角色边界）
  ↓
Agents（执行主体）← 你在这里
  ↓
Prompts（场景触发）
  ↓
Skills（工具能力）
```

---

## 二、Agent 的本质

### Agent 回答三个问题

1. **我是谁？**（角色认知）
2. **我被允许做什么？**（权限边界）
3. **我要为哪类违规负责？**（职责范围）

### Agent 的能力边界

| 维度  | Agent          |
|-----|----------------|
| 本质  | 角色约束           |
| 决策权 | ❌ 无            |
| 解释权 | ❌ 无            |
| 执行权 | ✅ 有（在 ADR 范围内） |
| 风险  | 越权             |

---

## 三、标准 Agents 定义

### 3.1 architecture-guardian（架构守护者）

**职责**：实时监督代码符合所有架构约束

**监督的 ADR**：所有 ADR（0000~0999）

**工作场景**：
- 开发阶段实时提醒
- 编码时阻止架构违规
- 提交前预审查

**风险等级**：⚠️ 极高

**配置文件**：[architecture-guardian.agent.md](architecture-guardian.agent.md)

---

### 3.2 adr-reviewer（ADR 审查者）

**职责**：审查 ADR 文档的质量和完整性

**监督的 ADR**：ADR-0900（ADR 流程）

**工作场景**：
- 新 ADR 提交时审查
- ADR 修订时检查合规性
- 确保 ADR/测试/Prompt 三位一体

**风险等级**：⚠️ 高

**配置文件**：[adr-reviewer.agent.md](adr-reviewer.agent.md)

---

### 3.3 test-generator（测试生成器）

**职责**：生成符合架构规范的测试代码

**监督的 ADR**：
- ADR-0000（架构测试）
- ADR-0122（测试组织）

**工作场景**：
- 生成单元测试
- 生成架构测试
- 生成集成测试

**风险等级**：⚠️ 中

**配置文件**：[test-generator.agent.md](test-generator.agent.md)

---

### 3.4 module-boundary-checker（模块边界检查器）

**职责**：专门监督模块隔离和边界约束

**监督的 ADR**：ADR-0001（模块化单体）

**工作场景**：
- 检查跨模块引用
- 监督模块间通信方式
- 验证 Contracts 使用

**风险等级**：⚠️ 极高

**配置文件**：[module-boundary-checker.agent.md](module-boundary-checker.agent.md)

---

### 3.5 handler-pattern-enforcer（Handler 规范执行器）

**职责**：确保 Handler 模式正确使用

**监督的 ADR**：
- ADR-0005（应用内交互模型）
- ADR-0201（Handler 生命周期）

**工作场景**：
- 检查 Handler 签名
- 验证 Command/Query 分离
- 监督资源释放

**风险等级**：⚠️ 高

**配置文件**：[handler-pattern-enforcer.agent.md](handler-pattern-enforcer.agent.md)

---

### 3.6 documentation-maintainer（文档维护者）

**职责**：维护文档质量和一致性

**监督的 ADR**：
- ADR-0900（ADR 流程）
- 文档编写规范

**工作场景**：
- 检查文档格式
- 验证文档链接
- 更新文档索引

**风险等级**：⚠️ 低

**配置文件**：[documentation-maintainer.agent.md](documentation-maintainer.agent.md)

---

## 四、Agent 配置文件标准结构

每个 Agent 配置文件（`*.agent.md`）必须包含：

### 4.1 元数据

```yaml
---
name: "Agent 名称"
description: "简短描述"
version: "1.0"
risk_level: "高/中/低"
supervised_adrs: ["ADR-0001", "ADR-0002"]
tools: ["tool1", "tool2"]
---
```

### 4.2 角色定义

```markdown
# 角色定义

## 我是谁
（角色认知）

## 我的职责
（具体职责列表）

## 我的权限边界
- ✅ 允许做什么
- ❌ 禁止做什么
```

### 4.3 工作流程

```markdown
# 工作流程

## 触发场景
（什么时候激活这个 Agent）

## 执行步骤
1. 第一步
2. 第二步
3. ...

## 输出结果
（生成什么）
```

### 4.4 约束与检查清单

```markdown
# 约束与检查清单

## 必须检查的点
- [ ] 检查项 1
- [ ] 检查项 2

## 必须阻止的行为
- ❌ 禁止行为 1
- ❌ 禁止行为 2
```

---

## 五、Agent 使用指南

### 5.1 如何激活 Agent

**方式 1：在 IDE 中**
```
@architecture-guardian 
我想在 Orders 模块中添加一个新的用例，有哪些架构约束？
```

**方式 2：在 PR Review 中**
```
@adr-reviewer
请审查这个 PR 的架构合规性
```

**方式 3：在问题讨论中**
```
@module-boundary-checker
这种跨模块调用方式是否合规？
```

### 5.2 Agent 响应模式

**预防模式**：在开发前提醒约束
```
开发者：我想做 X
Agent：根据 ADR-YYYY，你需要注意：...
```

**阻止模式**：在违规时立即阻止
```
开发者：（写了违规代码）
Agent：⚠️ 这违反了 ADR-YYYY，不允许执行
```

**诊断模式**：在测试失败后解释
```
开发者：为什么架构测试失败了？
Agent：失败原因：... 修复建议：...
```

---

## 六、Agent 之间的协作

### 6.1 协作关系图

```mermaid
graph TB
    Dev[开发者] --> Guardian[architecture-guardian]
    
    Guardian --> ModuleChecker[module-boundary-checker]
    Guardian --> HandlerEnforcer[handler-pattern-enforcer]
    
    Dev --> TestGen[test-generator]
    TestGen --> Guardian
    
    PR[PR 提交] --> ADRReviewer[adr-reviewer]
    ADRReviewer --> Guardian
    
    CI[CI 失败] --> TestEnforcer[test-enforcer]
    TestEnforcer --> Guardian
    
    Doc[文档更新] --> DocMaintainer[documentation-maintainer]
    
    style Guardian fill:#ffcccc
    style ModuleChecker fill:#ffe0cc
    style HandlerEnforcer fill:#ffe0cc
```

### 6.2 协作原则

- **Guardian 是中枢**：其他 Agent 都向它汇报
- **专业分工**：每个 Agent 只负责自己的领域
- **相互验证**：一个 Agent 的输出可以被另一个验证

---

## 七、Agent 的限制（重要）

### 7.1 Agent 不能做什么

| Agent **不能**做的事       | 原因           |
|----------------------|--------------|
| ❌ 最终裁决架构决策           | 只有 ADR 有此权限  |
| ❌ 批准架构破例             | 需要人工审批       |
| ❌ 修改 ADR 本身           | ADR 有专门流程    |
| ❌ 绕过架构测试             | 测试是最终仲裁者     |
| ❌ 替代人工理解 ADR         | Agent 是放大器，不是替代 |
| ❌ 自行决定"可以破例"         | 破例需要走正式流程    |

### 7.2 Agent 的风险

| 风险类型 | 描述                 | 防范措施            |
|------|--------------------|--------------------|
| 越权   | Agent 做了不该做的决策     | Instructions 明确边界 |
| 误导   | Agent 给出错误建议       | 以 ADR 为准，Agent 只是辅助 |
| 过度依赖 | 开发者完全依赖 Agent      | 强调 Agent 不替代理解   |
| 冲突   | 不同 Agent 建议矛盾      | Guardian 统一协调     |

---

## 八、Agent 演进与维护

### 8.1 何时添加新 Agent

**添加条件**：
- ✅ 某类架构约束需要专门监督
- ✅ 违规模式足够常见
- ✅ 有明确的 ADR 依据

**不应添加的情况**：
- ❌ 只是为了方便
- ❌ 没有对应的 ADR
- ❌ 职责与现有 Agent 重叠

### 8.2 Agent 配置更新流程

1. **识别需求**：发现 Agent 行为不符合预期
2. **检查 ADR**：确认 ADR 是否有变更
3. **更新配置**：修改 Agent 配置文件
4. **测试验证**：验证 Agent 行为
5. **文档同步**：更新相关文档
6. **团队通知**：通知团队 Agent 变更

### 8.3 版本管理

- Agent 配置文件纳入版本控制
- 重大变更需要版本号递增
- 保留变更历史记录

---

## 九、快速参考

### 9.1 按场景选择 Agent

| 场景                | 使用的 Agent                     |
|-------------------|------------------------------|
| 设计新功能             | architecture-guardian        |
| 跨模块调用             | module-boundary-checker      |
| 编写 Handler        | handler-pattern-enforcer     |
| 生成测试              | test-generator               |
| 提交 PR             | adr-reviewer                 |
| 架构测试失败            | test-enforcer                |
| 更新文档              | documentation-maintainer     |

### 9.2 Agent 风险等级

| 等级  | Agent                                        | 影响范围 |
|-----|----------------------------------------------|------|
| 极高  | architecture-guardian, module-boundary-checker | 系统级  |
| 高   | adr-reviewer, handler-pattern-enforcer         | 模块级  |
| 中   | test-generator                               | 文件级  |
| 低   | documentation-maintainer                     | 文档级  |

---

## 十、与其他层级的关系

### 10.1 从 Instructions 继承

```
Instructions（定义行为边界）
    ↓
Agent（在特定场景应用边界）
```

### 10.2 调用 Prompts

```
Agent（识别场景）
    ↓
加载对应的 Prompts
    ↓
执行具体检查
```

### 10.3 使用 Skills

```
Agent（决定要做什么）
    ↓
调用 Skills（执行具体操作）
    ↓
验证结果
```

---

## 十一、常见问题（FAQ）

### Q: Agent 和 Instructions 的区别是什么？

**A:**
- **Instructions**：定义 Copilot 的整体行为边界（"我是什么样的助手？"）
- **Agent**：在特定场景下的角色实例（"在这个场景下我的角色是什么？"）

### Q: 可以同时激活多个 Agent 吗？

**A:** 可以，但会由 `architecture-guardian` 协调。通常：
- 开发阶段：`architecture-guardian` + 专业 Agent
- PR 阶段：`adr-reviewer`
- CI 失败：`test-enforcer`

### Q: Agent 的建议和 ADR 冲突怎么办？

**A:** **ADR 优先级最高**。Agent 只是辅助工具，不能覆盖 ADR。

### Q: 如何知道应该问哪个 Agent？

**A:** 使用决策树：
```
问题类型？
├─ 架构约束 → architecture-guardian
├─ 模块边界 → module-boundary-checker
├─ Handler 规范 → handler-pattern-enforcer
├─ 测试编写 → test-generator
├─ ADR 审查 → adr-reviewer
└─ 文档维护 → documentation-maintainer
```

---

## 十二、下一步

### 了解具体 Agent

- [architecture-guardian](architecture-guardian.agent.md)
- [adr-reviewer](adr-reviewer.agent.md)
- [test-generator](test-generator.agent.md)
- [module-boundary-checker](module-boundary-checker.agent.md)
- [handler-pattern-enforcer](handler-pattern-enforcer.agent.md)
- [documentation-maintainer](documentation-maintainer.agent.md)

### 了解相关系统

- [Instructions 体系](../instructions/README.md)
- [Prompts 库](../../docs/copilot/README.md)
- [Skills 体系](../skills/README.md)
- [架构治理系统总览](../../docs/ARCHITECTURE-GOVERNANCE-SYSTEM.md)

---

## 版本历史

| 版本  | 日期         | 变更说明              |
|-----|------------|-------------------|
| 1.0 | 2026-01-25 | 初始版本，建立 6 个标准 Agent |

---

**维护团队**：架构委员会  
**审核人**：@douhuaa  
**状态**：✅ Active
