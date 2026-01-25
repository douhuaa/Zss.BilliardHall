# AI 治理体系快速入门

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](adr/README.md)。

**目标读者**：新成员、希望快速了解 AI 治理体系的开发者  
**阅读时间**：15 分钟  
**版本**：1.0

---

## 一、一句话理解

> **本项目用 AI（Agents）作为"架构守护者"，在开发过程中实时监督代码符合 ADR（架构决策记录），通过 Instructions、Prompts、Skills 四层体系实现自动化治理。**

---

## 二、为什么需要 AI 治理体系？

### 传统问题

```
开发者写代码
  ↓
提交 PR
  ↓
CI 架构测试失败 ❌
  ↓
询问老员工："为什么失败？"
  ↓
老员工解释 ADR
  ↓
修改代码
  ↓
重新提交
```

**问题**：
- 老员工被重复询问，浪费时间
- 新人学习曲线陡峭
- 架构违规发现太晚，修复成本高

### AI 治理方案

```
开发者写代码
  ↓
AI Agent 实时提醒架构约束 ✅
  ↓
开发者边写边修正
  ↓
提交 PR（首次通过率 > 85%）
  ↓
CI 测试通过 ✅
```

**价值**：
- 预防 > 修复
- 自助学习 > 询问他人
- 快速反馈 > 延迟发现

---

## 三、五层体系快速理解

### 层级 0：ADR（宪法）

**是什么**：架构决策记录，项目的"宪法"

**在哪里**：`docs/adr/`

**作用**：定义"能做什么，不能做什么"

**示例**：
- ADR-0001：模块不能直接引用其他模块
- ADR-0005：Command Handler 不能返回业务数据

**你需要做什么**：
- ✅ 遇到架构问题时，查阅 ADR
- ✅ 以 ADR 为最终裁决依据
- ❌ 不要试图绕过 ADR

---

### 层级 1：Instructions（岗位说明书）

**是什么**：定义 AI 的身份和行为边界

**在哪里**：`.github/instructions/`

**作用**：告诉 AI"你是什么样的助手"

**示例**：
- `base.instructions.md`：所有 AI 的基本行为
- `backend.instructions.md`：后端开发时的行为
- `architecture-review.instructions.md`：架构评审时的行为

**你需要做什么**：
- 📖 了解即可，不需要频繁查看
- ✅ 相信 AI 会遵守这些边界

---

### 层级 2：Agents（执行主体）

**是什么**：带特定职责的 AI 角色

**在哪里**：`.github/agents/`

**作用**：在特定场景下工作的 AI 实例

**标准 Agents**：

| Agent                       | 用途            | 何时使用              |
|-----------------------------|---------------|-------------------|
| `@architecture-guardian`    | 架构守护者         | 设计阶段、编码阶段、提交前审查   |
| `@adr-reviewer`             | ADR 审查者       | 提交 PR、审查 ADR 文档   |
| `@test-generator`           | 测试生成器         | 生成测试代码            |
| `@module-boundary-checker`  | 模块边界检查器       | 检查跨模块调用           |
| `@handler-pattern-enforcer` | Handler 规范执行器 | 检查 Handler 是否符合规范 |
| `@documentation-maintainer` | 文档维护者         | 更新文档、检查文档质量       |

**你需要做什么**：
- ✅ 在合适的场景 `@` 对应的 Agent
- ✅ 跟随 Agent 的建议修正代码
- ❌ 不要无视 Agent 的警告

---

### 层级 3：Prompts（执行手册）

**是什么**：ADR 的场景化翻译

**在哪里**：`docs/copilot/`

**作用**：告诉 AI"遇到 X 场景，应该做什么"

**文件结构**：
- `adr-0001.prompts.md`：ADR-0001 的执行手册
- `adr-0002.prompts.md`：ADR-0002 的执行手册
- `architecture-test-failures.md`：测试失败诊断指南

**你需要做什么**：
- 📖 遇到 AI 警告时，查看对应的 Prompts 文件
- ✅ 从中学习正确的做法
- ✅ 补充常见问题到 Prompts

---

### 层级 4：Skills（工具函数）

**是什么**：AI 可以调用的工具

**在哪里**：`.github/skills/`

**作用**：实际执行代码分析、生成、修改等操作

**分类**：
- 代码生成：生成 Handler、测试、Endpoint
- 代码分析：扫描依赖、分析架构、检查命名
- 代码修改：批量重命名、移动文件（高风险）
- 文档生成：生成 ADR、Prompts
- 测试执行：运行架构测试、单元测试

**你需要做什么**：
- ℹ️ 了解 Skills 的存在即可
- ⚠️ Skills 由 Agent 调用，不能直接使用
- ✅ 高风险 Skills（代码修改）执行后必须验证

---

## 四、典型使用场景

### 场景 1：我想添加一个新功能

**步骤**：

1. **设计前询问**
```
@architecture-guardian
我想在 Orders 模块中添加一个"取消订单"的用例，有哪些架构约束？
```

2. **获得建议**
```
✅ 基于 ADR-0001 和 ADR-0005：
- 在 Orders/UseCases/CancelOrder/ 创建垂直切片
- 定义 CancelOrder Command
- 实现 CancelOrderHandler
- Handler 只返回 void 或订单 ID
- 不要创建 OrderService

📚 参考：docs/copilot/adr-0001.prompts.md
```

3. **按建议实施**

4. **提交前再次审查**
```
@architecture-guardian
请审查我的代码是否符合架构规范
```

---

### 场景 2：架构测试失败了

**步骤**：

1. **复制 CI 日志**

2. **询问 test-enforcer**
```
@test-enforcer
请根据以下架构测试失败日志，解释违规原因并提供修复建议

Module_Orders_Should_Not_Reference_Module_Members 失败
Expected: 0 types
Found: 1 type
- Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder.CreateOrderHandler
```

3. **获得诊断**
```
⚠️ 违反 ADR-0001：模块隔离

问题：
Orders 模块直接引用了 Members 模块的类型

正确做法：
- 方案 1：通过领域事件异步通信
- 方案 2：通过契约（DTO）查询数据
- 方案 3：通过原始类型传递 MemberId

📚 参考：docs/copilot/adr-0001.prompts.md（场景 3）
```

4. **按建议修复**

---

### 场景 3：我想跨模块调用

**步骤**：

1. **询问边界检查器**
```
@module-boundary-checker
Orders 模块需要查询 Members 模块的会员余额，应该怎么做？
```

2. **获得建议**
```
✅ 正确做法（按优先级）：

方案 1：通过契约查询（推荐）
```csharp
var memberBalance = await _queryBus.Send(
    new GetMemberBalance(memberId)
);
// 使用 memberBalance.Balance（只读）
```

方案 2：通过领域事件（如果是异步通知）
```csharp
await _eventBus.Publish(
    new OrderCreated(orderId, memberId)
);
// Members 模块订阅事件后自行处理
```

❌ 错误做法：
```csharp
// ❌ 不要直接引用
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(memberId);
```

📚 参考：docs/copilot/adr-0001.prompts.md
```

3. **按建议实施**

---

### 场景 4：我要提交 PR

**步骤**：

1. **使用 adr-reviewer 审查**
```
@adr-reviewer
请审查本 PR 的架构合规性
```

2. **获得报告**
```
## 架构审查报告

### ✅ 合规方面
- 模块边界清晰
- 命名规范符合要求
- 依赖方向正确

### ⚠️ 潜在关注点
- OrderService.cs 文件名不符合垂直切片模式
- 建议重构为 CreateOrder/CreateOrderHandler.cs

### 📚 建议阅读
- docs/copilot/adr-0001.prompts.md
```

3. **修正问题**

4. **填写 PR 模板中的 AI 参与清单**

---

## 五、关键原则（必须记住）

### 原则 1：ADR 优先级最高

```
ADR（宪法）> Agent 建议 > 个人经验
```

- ✅ 遇到冲突时，以 ADR 为准
- ❌ 不要因为 Agent 说"可以"就忽略 ADR
- ❌ 不要因为"以前这样做"就违反 ADR

### 原则 2：Agent 是辅助，不是替代

```
Agent 放大理解能力，不替代理解
```

- ✅ 使用 Agent 快速理解 ADR
- ✅ 使用 Agent 提前发现问题
- ❌ 不要完全依赖 Agent，必须阅读 ADR
- ❌ 不要认为"Agent 没说不行"就是合规

### 原则 3：架构测试是最终仲裁

```
Architecture Tests > Agent 建议 > 口头约定
```

- ✅ 测试失败 = 必须修复
- ❌ 不要试图绕过测试
- ❌ 不要"先 ignore 再说"

### 原则 4：不确定时保守处理

```
当无法确认 ADR 明确允许某行为时，假定该行为被禁止
```

- ✅ 不确定时，先查 ADR
- ✅ 还不确定时，问 Agent
- ✅ 仍不确定时，问有经验的同事
- ❌ 不要"先试试看"

---

## 六、常见误区

### ❌ 误区 1："既然有 Agent，我不用看 ADR 了"

**正确认知**：
- Agent 是帮你**更快**理解 ADR
- 不是**替代**阅读 ADR
- 不理解 ADR，只会更快触发 CI 失败

### ❌ 误区 2："Agent 说可以，那就可以"

**正确认知**：
- Agent 可能出错
- 以 ADR 为最终依据
- 以架构测试为最终验证

### ❌ 误区 3："这只是提示，我可以忽略"

**正确认知**：
- Agent 的警告基于 ADR
- 忽略警告 = 违反架构
- 最终 CI 会失败，修复成本更高

### ❌ 误区 4："Agent 没提醒，那就没问题"

**正确认知**：
- Agent 也有盲点
- 必须主动学习 ADR
- 架构测试是最终保障

---

## 七、快速检查清单

### 新功能开发前

- [ ] 询问 `@architecture-guardian` 相关约束
- [ ] 查阅相关 ADR 文档
- [ ] 查阅相关 Prompts 文件
- [ ] 确认模块边界
- [ ] 确认通信方式

### 编码过程中

- [ ] 遵循 Agent 的实时提醒
- [ ] 遇到警告立即修正
- [ ] 不确定时暂停询问
- [ ] 定期运行架构测试

### 提交 PR 前

- [ ] 使用 `@adr-reviewer` 审查
- [ ] 本地运行架构测试
- [ ] 填写 PR 模板中的 AI 参与清单
- [ ] 确认所有警告已处理

---

## 八、资源导航

### 完整文档

- [架构治理系统总览](ARCHITECTURE-GOVERNANCE-SYSTEM.md)
- [Agents 体系](.github/agents/README.md)
- [Instructions 体系](.github/instructions/README.md)
- [Prompts 库](copilot/README.md)
- [Skills 体系](.github/skills/README.md)

### 常用 ADR

- [ADR-0000：架构测试](adr/governance/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体](adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：三层启动](adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0005：Handler 模式](adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)

### 常用 Prompts

- [ADR-0001 提示词](copilot/adr-0001.prompts.md)
- [ADR-0005 提示词](copilot/adr-0005.prompts.md)
- [测试失败诊断](copilot/architecture-test-failures.md)
- [常见问题总结](copilot/pr-common-issues.prompts.md)

---

## 九、下一步

### 第一周

- [ ] 阅读本文档（你已经完成 ✅）
- [ ] 阅读 [Copilot 角色定位](copilot/README.md)
- [ ] 浏览 [ADR 目录](adr/README.md)
- [ ] 尝试使用 `@architecture-guardian`

### 第一个月

- [ ] 详细阅读宪法层 ADR（0001~0005）
- [ ] 参与至少 3 次 AI 辅助的 PR Review
- [ ] 补充一个案例到 Prompts 库
- [ ] 分享 AI 治理使用心得

---

## 十、获取帮助

### 遇到问题时

1. **AI 行为问题**
  - 查看对应的 Prompts 文件
  - 在 `.github/agents/` 中创建 Issue

2. **架构约束问题**
  - 查阅对应的 ADR
  - 询问 `@architecture-guardian`

3. **工具使用问题**
  - 查看 [Agents README](.github/agents/README.md)
  - 在团队讨论频道提问

---

**维护团队**：架构委员会  
**版本**：1.0  
**最后更新**：2026-01-25
