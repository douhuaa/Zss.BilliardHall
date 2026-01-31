# 架构测试一对一映射与方法细化总结

**日期**：2026-01-31  
**版本**：1.0  
**状态**：✅ 已实施

---

## 背景与目标

### 问题陈述

随着项目规模增长，需要建立更严格的架构测试规范，确保：
1. 每个 ADR 有且仅有一个对应的测试类
2. 每条架构规则细化为独立的测试方法
3. 测试风格保持一致，便于维护和协作

### 核心目标

建立**架构测试一对一映射与方法细化**机制，提升测试的：
- **隔离性**：规则变动互不影响
- **可追溯性**：测试与 ADR 精确对应
- **可维护性**：变更影响范围最小化
- **协作效率**：团队成员并行开发
- **CI/CD 效率**：失败精确定位

---

## 实施方案

### 1. ADR-0000.Z 规则

在 [ADR-0000：架构测试与 CI 治理宪法](../adr/governance/ADR-0000-architecture-tests.md) 中新增章节：

#### 核心原则

- **一对一映射**：每个 ADR → 唯一测试类
- **规则独立**：每条规则 → 独立测试方法
- **风格一致**：统一命名和组织模式

#### 命名规范

**测试类**：
```csharp
public sealed class ADR_0001_Architecture_Tests { }
```

**测试方法**：
```csharp
[Fact(DisplayName = "ADR-0001.1: 模块不应相互引用")]
public void Modules_Should_Not_Reference_Other_Modules() { }
```

#### 组织结构

```csharp
#region 1. 模块隔离约束 (ADR-0001.1, 0001.2, 0001.7)

[Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
public void Modules_Should_Not_Reference_Other_Modules() 
{
    // 测试逻辑
    Assert.True(result.IsSuccessful,
        $"❌ ADR-0001.1 违规: ...\n" +
        $"违规类型: ...\n" +
        $"修复建议：\n" +
        $"  1. 使用领域事件进行异步通信\n" +
        $"参考：docs/adr/constitutional/ADR-0001-...");
}

#endregion
```

### 2. 自动化验证

在 `ADR_0000_Architecture_Tests.cs` 中新增测试：

```csharp
[Fact(DisplayName = "ADR-0000.Z: 测试方法 DisplayName 应包含 ADR 编号")]
public void Test_Methods_Should_Include_ADR_Number_In_DisplayName()
{
    // 验证所有测试方法的 DisplayName 包含 ADR 编号
    // 支持 3 位（如 910）和 4 位（如 0001）编号
}
```

**验证内容**：
- ✅ 测试类名符合 `ADR_{编号}_Architecture_Tests` 格式
- ✅ DisplayName 包含对应 ADR 编号（建议性，不强制阻断）
- ✅ 对非标准编号提供兼容支持

### 3. 文档更新

#### ArchitectureTests/README.md

在文档开头新增"**核心设计原则**"章节，包括：
- 一对一映射规则详解
- 五大优势说明
- 命名规范示例
- 组织结构要求
- 强制执行机制

---

## 优势验证

### 1. 隔离性强

**现状**：176 个架构测试，每个测试方法验证一条独立规则

**优势**：
- 单一规则失败不影响其他测试
- 修改某条规则只需更新对应测试方法
- 降低回归风险

### 2. 易于追踪和维护

**现状**：所有测试 DisplayName 包含 ADR 编号和子编号

**示例**：
- `ADR-0001.1: 模块不应相互引用`
- `ADR-0005.3: Handler 不应依赖 ASP.NET 类型`

**优势**：
- 测试失败时立即知道违反了哪条 ADR 规则
- 从 ADR 文档可精确找到对应测试
- 变更 ADR 时可快速定位需更新的测试

### 3. 融合 ADR 文档链路

**机制**：
- ADR-0000 自动验证一对一映射关系
- 每个测试失败消息引用 ADR 文档
- 测试与 ADR 双向可追溯

**示例错误消息**：
```
❌ ADR-0001.1 违规: 模块 Orders 不应依赖模块 Members。
违规类型: Orders.Domain.OrderService
修复建议：
  1. 使用领域事件进行异步通信
  2. 使用数据契约进行只读查询
  3. 传递原始类型（Guid、string）而非领域对象
参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md
```

### 4. 便于团队协作

**组织方式**：
- 使用 `#region` 分组相关规则
- 每个 ADR 独立测试类
- 清晰的命名约定

**优势**：
- 不同开发者可并行维护不同 ADR 的测试
- 减少代码冲突
- 新人快速理解测试结构

### 5. 提升 CI/CD 效率

**现状**：测试输出精确到具体规则

**示例**：
```
Passed ADR-0001.1: 模块不应相互引用 [15 ms]
Failed ADR-0001.2: 模块项目文件不应引用其他模块 [8 ms]
Passed ADR-0001.3: Handler 应该在 UseCases 命名空间下 [12 ms]
```

**优势**：
- 失败时精确定位到子规则
- 缩短排查时间
- CI 日志清晰可读

---

## 现有测试符合性

### 统计数据

- **总测试数**：176 个
- **通过率**：100%
- **测试类数**：27 个 ADR 测试类
- **平均每类测试数**：6.5 个

### 典型示例

#### ADR-0001（11 个测试）

```
✅ ADR-0001.1: 模块不应相互引用
✅ ADR-0001.2: 模块项目文件不应引用其他模块
✅ ADR-0001.3: Handler 应该在 UseCases 命名空间下
✅ ADR-0001.4: 模块不应包含横向 Service 类
✅ ADR-0001.5: 模块间只允许事件/契约/原始类型通信
✅ ADR-0001.6: Contract 不应包含业务判断字段
✅ ADR-0001.7: 命名空间应匹配模块边界
```

#### ADR-0005（12 个测试）

```
✅ ADR-0005.1: Handler 应有明确的命名约定
✅ ADR-0005.2: Endpoint 不应包含业务逻辑
✅ ADR-0005.3: Handler 不应依赖 ASP.NET 类型
✅ ADR-0005.4: Handler 应该是无状态的
✅ ADR-0005.5: 模块间不应有未审批的同步调用
...
```

---

## 扩展能力

### 架构治理可扩展性

**新增规则流程**：
1. 在 ADR 文档中定义新规则（如 ADR-0001.8）
2. 在对应测试类中添加新测试方法
3. 使用标准命名：`[Fact(DisplayName = "ADR-0001.8: ...")]`
4. ADR-0000 自动验证映射关系

**优势**：
- 不影响现有测试
- 遵循统一模式
- 自动化验证保证一致性

### 规范沉淀与技术公开

**文档链路**：
```
ADR 正文
  ↓ 映射
测试类（ADR_XXXX_Architecture_Tests.cs）
  ↓ 引用
Copilot Prompts（adr-XXXX.prompts.md）
  ↓ 指导
开发实践
```

**优势**：
- 新成员快速理解架构约束
- 测试即文档
- 降低学习成本

### 降低回归风险

**隔离机制**：
- 每条规则独立测试方法
- 区域分组避免混淆
- 精确的错误消息

**示例**：
修改 ADR-0001.2（项目引用规则）时：
- ✅ 只需修改 `Module_Csproj_Should_Not_Reference_Other_Modules` 方法
- ✅ 其他 10 个测试不受影响
- ✅ CI 精确报告受影响的规则

---

## 实施效果

### 测试结果

```bash
$ dotnet test src/tests/ArchitectureTests -c Release

Test Run Successful.
Total tests: 176
     Passed: 176
 Total time: 2.77 seconds
```

### 关键测试

#### ADR-0000 元规则验证

```
✅ ADR-0000: 每条 ADR 必须有且仅有唯一对应的架构测试类
✅ ADR-0000: 架构测试类必须包含最少断言数（反作弊）
✅ ADR-0000: 测试失败消息必须包含 ADR 编号（反作弊）
✅ ADR-0000: 禁止跳过架构测试（反作弊）
✅ ADR-0000.Z: 测试方法 DisplayName 应包含 ADR 编号 ← 新增
```

---

## 最佳实践

### 1. 编写新测试

```csharp
/// <summary>
/// ADR-XXXX: {ADR 标题}
/// 验证 {验证内容}
/// 
/// 测试映射：
/// - Test_Method_Name → ADR-XXXX.Y ({规则描述})
/// </summary>
public sealed class ADR_XXXX_Architecture_Tests
{
    #region 1. {分组名称} (ADR-XXXX.1, XXXX.2)
    
    [Fact(DisplayName = "ADR-XXXX.1: {规则描述}")]
    public void Test_Method_Name()
    {
        // 测试逻辑
        Assert.True(result.IsSuccessful,
            $"❌ ADR-XXXX.1 违规: {原因}\n" +
            $"违规类型: {类型}\n" +
            $"修复建议：\n" +
            $"  1. {建议 1}\n" +
            $"  2. {建议 2}\n" +
            $"参考：{ADR 文档路径}");
    }
    
    #endregion
}
```

### 2. 维护现有测试

- ✅ 保持 DisplayName 包含 ADR 编号
- ✅ 使用 #region 分组相关测试
- ✅ 错误消息包含完整的修复建议
- ✅ 引用具体的 ADR 文档章节

### 3. 命名约定

| 元素     | 格式                                       | 示例                                      |
|--------|------------------------------------------|------------------------------------------|
| 测试类    | `ADR_{编号}_Architecture_Tests`         | `ADR_0001_Architecture_Tests`            |
| 测试方法   | 动词开头，清晰表达约束                            | `Modules_Should_Not_Reference_Other_Modules` |
| DisplayName | `ADR-{编号}.{子编号}: {规则描述}`            | `ADR-0001.1: 模块不应相互引用`                  |
| Region   | `{序号}. {分组名称} (ADR-{编号}.{子编号列表})` | `1. 模块隔离约束 (ADR-0001.1, 0001.2, 0001.7)` |

---

## 相关文档

- [ADR-0000：架构测试与 CI 治理宪法](../adr/governance/ADR-0000-architecture-tests.md)
- [ArchitectureTests README](../../src/tests/ArchitectureTests/README.md)
- [架构测试实施指南](./governance/architecture-test-implementation-guide.md)

---

## 版本历史

| 版本 | 日期       | 变更说明               |
|----|----------|---------------------|
| 1.0 | 2026-01-31 | 初始版本，实施一对一映射与方法细化规则 |

---

**维护者**：架构委员会  
**审核者**：@douhuaa  
**状态**：✅ Active
