# 架构测试失败解释指南

**版本**：1.0  
**最后更新**：2026-01-21  
**用途**：帮助 Copilot 解释架构测试失败并提供修复建议

---

## 如何使用本指南

当架构测试失败时：

1. **复制完整的失败日志**
2. **询问 Copilot**："请根据以下架构测试失败日志，解释违规原因并提供修复建议"
3. **将失败日志粘贴给 Copilot**
4. **Copilot 会参考本指南和对应的 ADR 提供详细解释**

---

## 架构测试失败的标准解释模板

Copilot 在解释架构测试失败时，应遵循以下模板：

```markdown
### 违规分析

**违反的 ADR**：ADR-XXXX
**测试类**：ADR_XXXX_Architecture_Tests
**失败测试**：TestMethodName

### 问题描述

[用人话解释发生了什么违规]

### 违规原因

[解释为什么这是违规，引用 ADR 的具体条款]

### 修复建议

#### 步骤 1：[第一步]
[具体操作，包含代码示例]

#### 步骤 2：[第二步]
[具体操作，包含代码示例]

#### 步骤 3：验证
[如何验证修复是否成功]

### 相关资源

- [ADR 文档链接]
- [Prompt 文件链接]
```

---

## 常见架构测试失败场景

### 1. ADR-0001 相关失败

#### 场景 1.1：模块间非法依赖

**典型失败消息**：

```
❌ Modules_Should_Not_Reference_Other_Modules
   Expected: No references from 'Orders' to 'Members'
   Actual: Found reference to 'Zss.BilliardHall.Modules.Members.Domain'
```

**Copilot 应该这样解释**：

```markdown
### 违规分析

**违反的 ADR**：ADR-0001 - 模块化单体与垂直切片架构
**违规类型**：模块间直接依赖

### 问题描述

Orders 模块直接引用了 Members 模块的内部实现（Domain 层），违反了模块隔离原则。

### 违规原因

根据 ADR-0001：
- 模块必须保持独立边界
- 模块之间只能通过三种方式通信：
  1. 领域事件（Domain Events）
  2. 数据契约（Contracts）
  3. 原始类型（Primitives）

直接引用其他模块的内部实现会导致：
- 模块边界被打破
- 紧耦合
- 无法独立演进

### 修复建议

#### 步骤 1：识别依赖类型

检查 Orders 模块中哪些地方引用了 Members.Domain：

```bash
# 在 Orders 模块中搜索
grep -r "Zss.BilliardHall.Modules.Members.Domain" src/Modules/Orders/
```

#### 步骤 2：根据场景选择合规方式

**场景 A：需要传递 Member 标识**

```csharp
// ❌ 错误
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);
var order = new Order(member, items);

// ✅ 正确
var order = new Order(memberId, items); // Guid
```

**场景 B：需要查询 Member 数据**

```csharp
// ❌ 错误
using Zss.BilliardHall.Modules.Members.Domain;
var member = await _memberRepository.GetByIdAsync(id);
if (!member.IsActive) throw new Exception();

// ✅ 正确
var memberDto = await _queryBus.Send(new GetMemberById(id));
if (!memberDto.IsActive) throw new Exception();
```

**场景 C：需要通知 Member 模块**

```csharp
// ❌ 错误
using Zss.BilliardHall.Modules.Members.Domain;
await _memberService.UpdateOrderCount(memberId);

// ✅ 正确
await _eventBus.Publish(new OrderCreated(orderId, memberId));
```

#### 步骤 3：移除项目引用

```xml
<!-- Orders.csproj -->
<ItemGroup>
  <!-- ❌ 移除这个 -->
  <ProjectReference Include="..\Members\Members.csproj" />
</ItemGroup>
```

#### 步骤 4：验证

```bash
# 运行架构测试
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~ADR_0001"
```

### 相关资源

- [ADR-0001 完整文档](../adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0001 提示词库](adr-0001.prompts.md)

```

---

#### 场景 1.2：发现横向 Service 层

**典型失败消息**：
```

❌ Modules_Should_Not_Have_Service_Layer
Expected: No types ending with 'Service'
Actual: Found 'OrderService' in Orders module

```

**Copilot 解释重点**：
- 解释垂直切片架构原则
- 说明为什么 Service 层违反原则
- 提供将 Service 拆分为 Handler 的具体步骤

---

### 2. ADR-0002 相关失败

#### 场景 2.1：Program.cs 超过 30 行

**典型失败消息**：
```

❌ Program_Should_Be_Minimal
Expected: Program.cs <= 30 lines
Actual: Program.cs has 67 lines

```

**Copilot 解释重点**：
- 说明 Program.cs 为什么要保持简洁
- 提供提取配置到 Bootstrapper 的步骤
- 展示重构前后的对比

---

#### 场景 2.2：Platform 依赖 Application

**典型失败消息**：
```

❌ Platform_Should_Not_Depend_On_Application
Expected: No dependency from Platform to Application
Actual: Found reference to 'Zss.BilliardHall.Application'

```

**Copilot 解释重点**：
- 解释三层依赖方向（Host → Application → Platform）
- 说明反向依赖的危害
- 提供重构方案（依赖注入、接口抽象）

---

### 3. ADR-0003 相关失败

#### 场景 3.1：命名空间与目录不一致

**典型失败消息**：
```

❌ Namespace_Should_Match_Directory_Structure
Expected: Zss.BilliardHall.Modules.Orders.Domain
Actual: Zss.BilliardHall.Orders

```

**Copilot 解释重点**：
- 解释命名空间自动推导规则
- 提供修复命名空间声明的示例
- 说明如何验证修复

---

### 4. ADR-0004 相关失败

#### 场景 4.1：项目中指定了包版本

**典型失败消息**：
```

❌ Projects_Should_Not_Specify_Package_Version
Expected: No Version attribute in PackageReference
Actual: Found Version="7.0.0" in Orders.csproj

```

**Copilot 解释重点**：
- 解释 CPM 原则
- 提供在 Directory.Packages.props 中定义版本的示例
- 说明如何在项目中引用（不带版本）

---

### 5. ADR-0005 相关失败

#### 场景 5.1：Endpoint 包含业务逻辑

**典型失败消息**：
```

❌ Endpoints_Should_Not_Contain_Business_Logic
Expected: Endpoints only map and forward
Actual: Found business logic in CreateOrderEndpoint

```

**Copilot 解释重点**：
- 解释 Endpoint 的职责边界
- 提供将业务逻辑提取到 Handler 的步骤
- 展示重构前后的代码对比

---

## 复杂场景的解释指导

### 场景：多个测试同时失败

当多个架构测试同时失败时，Copilot 应该：

1. **按 ADR 编号分组失败**
2. **识别根本原因**（通常一个根本问题会导致多个测试失败）
3. **提供按优先级排序的修复建议**

**示例**：
```markdown
### 失败总览

发现 5 个架构测试失败，归属于 3 个 ADR：

#### ADR-0001 相关（2 个）
- Modules_Should_Not_Reference_Other_Modules
- Modules_Should_Not_Have_Service_Layer

#### ADR-0002 相关（2 个）
- Platform_Should_Not_Depend_On_Application
- Program_Should_Be_Minimal

#### ADR-0004 相关（1 个）
- Projects_Should_Not_Specify_Package_Version

### 根本原因分析

这些失败看似独立，实际上有共同的根本原因：
[分析根本原因]

### 推荐修复顺序

1. **优先修复 ADR-0001 违规**（影响最大）
   [详细步骤]

2. **然后修复 ADR-0002 违规**
   [详细步骤]

3. **最后修复 ADR-0004 违规**（最简单）
   [详细步骤]
```

---

## Copilot 使用提示

### 提问技巧

**好的提问方式**：

```
请根据以下架构测试失败日志，解释违规原因并提供修复建议：

[粘贴完整的测试失败输出]
```

**提供额外上下文**：

```
我在实现 [功能描述] 时，遇到了以下架构测试失败：

[粘贴失败日志]

我的代码在 [文件路径]，主要做了 [简要说明]。
```

---

### 验证修复的标准流程

Copilot 在提供修复建议后，应该告诉用户如何验证：

```bash
# 1. 运行特定 ADR 的测试
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~ADR_000X"

# 2. 如果通过，运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 3. 如果全部通过，运行完整测试套件
dotnet test
```

---

## 特殊情况处理

### 情况 1：确实需要破例

如果确实需要违反架构规则，Copilot 应该：

1. **确认是否真的需要破例**
2. **提供破例流程指导**
3. **提醒需要在 PR 中声明**

```markdown
### 破例流程

如果确实需要违反 ADR-XXXX，请遵循以下流程：

1. **在 PR 标题中添加 `[ARCH-VIOLATION]` 前缀**
   ```

[ARCH-VIOLATION] feat(Orders): 临时允许同步调用 Members

   ```

2. **在 PR 描述中填写破例详情**：
   - 违反的 ADR 规则
   - 破例理由
   - 影响范围
   - 归还计划
   - 失效期

3. **获得架构师审批**

4. **破例将被记录在案以备审计**

详见：[PR 模板架构违规部分](../../.github/PULL_REQUEST_TEMPLATE.md)
```

---

### 情况 2：测试误报

如果怀疑是测试误报，Copilot 应该：

1. **先假设测试是正确的**
2. **仔细检查代码是否真的合规**
3. **如果确认是误报，提供报告流程**

```markdown
### 如果确认是测试误报

1. 在 GitHub Issues 中创建 Bug 报告
2. 标题：`[ArchTest] 测试误报：TestName`
3. 提供详细的场景说明和代码示例
4. 等待架构团队确认

在修复前，可以临时：
- 在测试中添加例外
- 在 PR 中说明情况
- 获得 Tech Lead 批准
```

---

## 持续改进

### 收集典型案例

当遇到新的失败场景时：

1. **记录失败日志**
2. **记录 Copilot 的解释**
3. **记录修复步骤**
4. **补充到对应的 adr-XXXX.prompts.md**

---

### 更新本指南

本指南应该持续更新：

- 每月回顾常见失败场景
- 补充新的解释模板
- 优化现有解释的质量
- 收集团队反馈

---

## 快速参考

### ADR 与测试类映射

| ADR      | 测试类                         | 主要检查项           |
|----------|-----------------------------|-----------------|
| ADR-0001 | ADR_0001_Architecture_Tests | 模块隔离、垂直切片       |
| ADR-0002 | ADR_0002_Architecture_Tests | 三层依赖、Program.cs |
| ADR-0003 | ADR_0003_Architecture_Tests | 命名空间一致性         |
| ADR-0004 | ADR_0004_Architecture_Tests | CPM、依赖层级        |
| ADR-0005 | ADR_0005_Architecture_Tests | CQRS、Handler 职责 |

---

### 常用命令

```bash
# 运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 运行特定 ADR 的测试
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~ADR_0001"

# 运行单个测试
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~Modules_Should_Not_Reference_Other_Modules"

# 查看详细输出
dotnet test src/tests/ArchitectureTests/ --verbosity detailed
```

---

## 相关资源

- [各 ADR 的 Prompt 文件](.)
- [ADR 文档目录](../adr/)
- [架构测试源代码](../../src/tests/ArchitectureTests/ADR/)
- [PR 模板](../../.github/PULL_REQUEST_TEMPLATE.md)

---

## 版本历史

| 版本  | 日期         | 变更说明 |
|-----|------------|------|
| 1.0 | 2026-01-21 | 初始版本 |
