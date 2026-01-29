# ADR 测试规则缺失分析与纵向修复计划

> **报告生成时间**：2026-01-29  
> **分析范围**：Zss.BilliardHall 项目所有 ADR 与架构测试  
> **分析方法**：系统性扫描 + 分层分析 + 优先级评估  
> **报告目的**：识别 ADR 测试缺口并制定纵向修复计划

---

## 执行摘要

本次扫描共审查了 **45 份 ADR 文档** 和 **26 个架构测试文件**，发现 **34 个 ADR 缺少对应的架构测试**，整体测试覆盖率仅为 **24%**。

### 核心发现

- ✅ **宪法层（Constitutional）完全覆盖**：8/8 ADRs 有测试（100%）
- ⚠️ **治理层（Governance）严重不足**：3/25 ADRs 有测试（12%）
- ❌ **结构层（Structure）完全缺失**：0/5 ADRs 有测试（0%）
- ❌ **运行层（Runtime）完全缺失**：0/4 ADRs 有测试（0%）
- ❌ **技术层（Technical）完全缺失**：0/4 ADRs 有测试（0%）

### 风险等级

🔴 **高风险** - 多个层级测试完全缺失，架构约束无法自动化验证

---

## 一、缺失测试清单

### 1.1 治理层（Governance Layer）- 22 个缺失

#### P0 级（需立即补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-900 | ADR 流程 | ✅ | P0 |
| ADR-904 | 架构测试最小断言语义 | ✅ | P0 |

#### P1 级（14天内补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-901 | 警告约束语义 | ❌ | P1 |
| ADR-902 | ADR 模板结构契约 | ❌ | P1 |
| ADR-905 | 执行级别分类 | ❌ | P1 |
| ADR-906 | 分析器 CI 门映射协议 | ❌ | P1 |
| ADR-940 | ADR 关系可追溯性管理 | ❌ | P1 |
| ADR-946 | ADR 标题层级语义约束 | ❌ | P1 |
| ADR-947 | 关系章节结构解析安全 | ❌ | P1 |

#### P2 级（30天内补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-945 | ADR 时间线演进视图 | ❌ | P2 |
| ADR-950 | 指南与 FAQ 文档治理 | ❌ | P2 |
| ADR-951 | 案例仓库管理 | ❌ | P2 |
| ADR-952 | 工程标准 ADR 边界 | ❌ | P2 |
| ADR-955 | 文档搜索与可发现性 | ❌ | P2 |
| ADR-960 | 入职文档治理 | ❌ | P2 |
| ADR-965 | 入职交互式学习路径 | ❌ | P2 |

#### P3 级（60天内补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-970 | 自动化日志集成标准 | ❌ | P3 |
| ADR-975 | 文档质量监控 | ❌ | P3 |
| ADR-980 | ADR 生命周期同步 | ❌ | P3 |
| ADR-990 | 文档演进路线图 | ❌ | P3 |

**已有测试**：ADR-0000, ADR-0900, ADR-0930

---

### 1.2 结构层（Structure Layer）- 5 个缺失

#### P0 级（需立即补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-121 | Contract DTO 命名组织 | ✅ | P0 |

#### P1 级（14天内补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-120 | 领域事件命名约定 | ❌ | P1 |
| ADR-122 | 测试组织命名 | ❌ | P1 |
| ADR-123 | Repository 接口分层 | ❌ | P1 |
| ADR-124 | Endpoint 命名约束 | ❌ | P1 |

**已有测试**：无

---

### 1.3 运行层（Runtime Layer）- 4 个缺失

#### P0 级（需立即补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-201 | Handler 生命周期管理 | ✅ | P0 |
| ADR-210 | 事件版本化兼容性 | ✅ | P0 |
| ADR-220 | 事件总线集成 | ✅ | P0 |
| ADR-240 | Handler 异常约束 | ✅ | P0 |

**已有测试**：无

---

### 1.4 技术层（Technical Layer）- 4 个缺失

#### P1 级（14天内补充）

| ADR 编号 | 标题 | 是否标注"必须测试" | 优先级 |
|---------|------|-----------------|--------|
| ADR-301 | 集成测试自动化 | ✅ | P1 |
| ADR-340 | 结构化日志监控约束 | ✅ | P1 |
| ADR-350 | 日志可观测性标准 | ✅ | P1 |
| ADR-360 | CI/CD 流水线标准化 | ✅ | P1 |

**已有测试**：无

---

## 二、问题分析

### 2.1 根本原因

1. **历史遗留**：早期 ADR 制定时未建立测试机制
2. **覆盖不均**：宪法层完全覆盖，但其他层级被忽视
3. **优先级问题**：重视业务架构测试，轻视治理流程测试
4. **资源限制**：测试开发需要额外投入

### 2.2 影响分析

| 影响类别 | 严重程度 | 影响范围 |
|---------|---------|---------|
| **架构违规无法自动检测** | 🔴 高 | Runtime/Structure 层 |
| **治理流程无法强制执行** | 🟠 中高 | Governance 层 |
| **文档质量无法自动验证** | 🟡 中 | 全部 ADR |
| **技术标准无法 CI 阻断** | 🟠 中高 | Technical 层 |

### 2.3 风险评估

**🔴 关键风险**：
- Runtime 层（ADR-201/210/220/240）缺失测试导致 Handler 生命周期、事件版本化等核心约束无法验证
- Structure 层（ADR-121）缺失导致 Contract DTO 命名约束无法自动检查

**🟠 高风险**：
- Governance 层关键 ADR（ADR-900/904）缺失导致 ADR 流程和测试最小断言无法验证
- Technical 层完全缺失导致 CI/CD、日志等技术标准无法强制执行

**🟡 中风险**：
- 其他治理层 ADR 缺失影响文档质量和流程规范

---

## 三、纵向修复计划

### 3.1 修复策略

#### 策略 1：分层优先级修复

遵循"从下至上、从关键到次要"的原则：

```
Phase 1 (Week 1-2)：  Runtime 层（P0） - 核心架构约束
Phase 2 (Week 2-3)：  Structure 层（P0/P1） - 结构规范
Phase 3 (Week 3-4)：  Governance 层（P0） - 流程验证
Phase 4 (Month 2)：   Technical 层（P1） - 技术标准
Phase 5 (Month 2-3)： Governance 层（P1-P3） - 完整治理
```

#### 策略 2：测试类型分类

根据 ADR 特点，采用不同测试类型：

| ADR 类型 | 测试类型 | 示例 |
|---------|---------|------|
| 结构约束类 | NetArchTest 静态分析 | ADR-121（DTO命名） |
| 生命周期类 | 集成测试 + Roslyn 分析 | ADR-201（Handler生命周期） |
| 流程治理类 | 文档结构验证 | ADR-900（ADR流程） |
| 技术标准类 | CI 脚本 + 静态分析 | ADR-360（CI/CD标准） |

#### 策略 3：渐进式覆盖

1. **先覆盖标注"必须架构测试覆盖"的 ADR**（16 个）
2. **再覆盖可自动化验证的约束**（结构、命名类）
3. **最后覆盖流程治理类 ADR**（需人工审查辅助）

---

### 3.2 分阶段实施计划

#### Phase 1：Runtime 层核心约束（Week 1-2）

**目标**：补充所有 Runtime 层架构测试（4 个）

| 任务 | ADR | 测试类型 | 工作量 | 责任人 |
|-----|-----|---------|-------|--------|
| Handler 生命周期测试 | ADR-201 | 集成测试 + Roslyn | 2天 | Backend Team |
| 事件版本化测试 | ADR-210 | 静态分析 + 集成测试 | 2天 | Backend Team |
| 事件总线集成测试 | ADR-220 | 集成测试 | 1.5天 | Backend Team |
| Handler 异常约束测试 | ADR-240 | 静态分析 | 1天 | Backend Team |

**交付物**：
- [ ] `ADR_0201_Architecture_Tests.cs`
- [ ] `ADR_0210_Architecture_Tests.cs`
- [ ] `ADR_0220_Architecture_Tests.cs`
- [ ] `ADR_0240_Architecture_Tests.cs`

**成功标准**：
- ✅ 所有 4 个测试文件创建并通过
- ✅ 测试覆盖 ADR 中标注"必须架构测试覆盖"的所有条款
- ✅ 测试失败消息明确引用 ADR 条款编号

---

#### Phase 2：Structure 层结构规范（Week 2-3）

**目标**：补充所有 Structure 层架构测试（5 个）

| 任务 | ADR | 测试类型 | 工作量 | 责任人 |
|-----|-----|---------|-------|--------|
| 领域事件命名测试 | ADR-120 | NetArchTest | 1天 | Backend Team |
| Contract DTO 命名测试 | ADR-121 | NetArchTest | 1天 | Backend Team |
| 测试组织命名测试 | ADR-122 | NetArchTest | 1天 | Testing Team |
| Repository 分层测试 | ADR-123 | NetArchTest | 1天 | Backend Team |
| Endpoint 命名测试 | ADR-124 | NetArchTest | 1天 | Backend Team |

**交付物**：
- [ ] `ADR_0120_Architecture_Tests.cs`
- [ ] `ADR_0121_Architecture_Tests.cs`
- [ ] `ADR_0122_Architecture_Tests.cs`
- [ ] `ADR_0123_Architecture_Tests.cs`
- [ ] `ADR_0124_Architecture_Tests.cs`

**成功标准**：
- ✅ 所有 5 个测试文件创建并通过
- ✅ 命名约束可静态验证
- ✅ 测试可在 CI 中自动运行

---

#### Phase 3：Governance 层流程验证（Week 3-4）

**目标**：补充 Governance 层 P0/P1 架构测试（9 个）

| 任务 | ADR | 测试类型 | 工作量 | 责任人 |
|-----|-----|---------|-------|--------|
| ADR 流程验证 | ADR-900 | 文档结构测试 | 2天 | Doc Team |
| 测试断言语义验证 | ADR-904 | 测试元数据分析 | 1.5天 | Testing Team |
| 警告约束语义测试 | ADR-901 | 文档验证 | 1天 | Doc Team |
| ADR 模板结构测试 | ADR-902 | 文档结构测试 | 1天 | Doc Team |
| 执行级别分类测试 | ADR-905 | 元数据分析 | 1天 | Testing Team |
| 分析器映射测试 | ADR-906 | CI 配置验证 | 1天 | DevOps Team |
| 关系管理测试 | ADR-940 | 关系一致性验证 | 2天 | Doc Team |
| 标题层级测试 | ADR-946 | 文档结构测试 | 0.5天 | Doc Team |
| 关系章节解析测试 | ADR-947 | 文档解析测试 | 1天 | Doc Team |

**交付物**：
- [ ] `ADR_900_Architecture_Tests.cs` 等 9 个测试文件
- [ ] 文档验证工具更新

**成功标准**：
- ✅ 核心流程可自动验证
- ✅ ADR 模板约束可强制执行

---

#### Phase 4：Technical 层技术标准（Month 2）

**目标**：补充所有 Technical 层架构测试（4 个）

| 任务 | ADR | 测试类型 | 工作量 | 责任人 |
|-----|-----|---------|-------|--------|
| 集成测试自动化验证 | ADR-301 | 测试配置验证 | 1.5天 | Testing Team |
| 日志约束测试 | ADR-340 | 静态分析 + 集成测试 | 2天 | Backend Team |
| 可观测性标准测试 | ADR-350 | CI 配置验证 | 1.5天 | DevOps Team |
| CI/CD 标准化测试 | ADR-360 | CI 配置验证 | 2天 | DevOps Team |

**交付物**：
- [ ] `ADR_0301_Architecture_Tests.cs`
- [ ] `ADR_0340_Architecture_Tests.cs`
- [ ] `ADR_0350_Architecture_Tests.cs`
- [ ] `ADR_0360_Architecture_Tests.cs`

---

#### Phase 5：Governance 层完整覆盖（Month 2-3）

**目标**：补充 Governance 层剩余 P2/P3 测试（13 个）

分批实施，每周完成 3-4 个测试文件。

**交付物**：
- [ ] `ADR_945_Architecture_Tests.cs` ~ `ADR_990_Architecture_Tests.cs`（13 个）

---

### 3.3 工作量估算

| 阶段 | 测试数量 | 总工作量 | 时间范围 |
|-----|---------|---------|---------|
| Phase 1 (Runtime) | 4 | 6.5 人天 | Week 1-2 |
| Phase 2 (Structure) | 5 | 5 人天 | Week 2-3 |
| Phase 3 (Governance P0/P1) | 9 | 11 人天 | Week 3-4 |
| Phase 4 (Technical) | 4 | 7 人天 | Month 2 |
| Phase 5 (Governance P2/P3) | 13 | 15 人天 | Month 2-3 |
| **总计** | **35** | **44.5 人天** | **~3 月** |

---

## 四、测试开发指南

### 4.1 测试文件命名规范

```
src/tests/ArchitectureTests/ADR/ADR_{编号}_Architecture_Tests.cs
```

示例：
- `ADR_0201_Architecture_Tests.cs`（Runtime）
- `ADR_0121_Architecture_Tests.cs`（Structure）
- `ADR_900_Architecture_Tests.cs`（Governance）

### 4.2 测试类结构模板

```csharp
namespace Zss.BilliardHall.ArchitectureTests.ADR;

/// <summary>
/// ADR-{编号}：{标题} - 架构测试
/// 依据：docs/adr/{层级}/ADR-{编号}-{名称}.md
/// </summary>
[Category("ArchitectureTests")]
[Category("ADR-{编号}")]
public class ADR_{编号}_Architecture_Tests
{
    // 【必须架构测试覆盖】的条款测试
    [Test]
    public void ADR_{编号}_{序号}_{约束名称}_Must_Be_Satisfied()
    {
        // Arrange
        // Act
        // Assert
        // 失败消息必须引用 ADR-{编号}.{序号}
    }
    
    // 可选约束测试
    [Test]
    public void ADR_{编号}_{序号}_{建议名称}_Should_Be_Followed()
    {
        // ...
    }
}
```

### 4.3 测试断言规范

遵循 **ADR-904**（架构测试最小断言语义）：

```csharp
// ✅ 正确：明确、可追溯
Assert.That(violations, Is.Empty, 
    $"违反 ADR-0201.1：Handler 必须使用 Scoped 生命周期。" +
    $"发现违规：{string.Join(", ", violations)}");

// ❌ 错误：模糊、无追溯
Assert.That(result, Is.True, "Test failed");
```

### 4.4 测试类型选择指南

| 约束类型 | 推荐测试工具 | 示例 ADR |
|---------|------------|----------|
| 命名约束 | NetArchTest | ADR-120/121/124 |
| 依赖约束 | NetArchTest | ADR-0001/0002 |
| 结构约束 | NetArchTest + Reflection | ADR-0003/123 |
| 生命周期约束 | Roslyn + 集成测试 | ADR-201 |
| 事件约束 | 集成测试 + 静态分析 | ADR-210/220 |
| 文档结构 | 自定义文档解析 | ADR-900/902/946 |
| CI 配置 | Shell 脚本 + 配置验证 | ADR-360 |

---

## 五、持续监控机制

### 5.1 新增 ADR 时强制要求

修改 **ADR 审查清单**（ADR-930），增加：

- [ ] **架构测试覆盖检查**
  - [ ] 如果 ADR 包含"【必须架构测试覆盖】"标注，是否已创建对应测试文件？
  - [ ] 测试文件命名是否符合规范？
  - [ ] 测试是否覆盖所有标注条款？

### 5.2 CI 自动检查

在 `.github/workflows/adr-test-coverage-check.yml` 中增加：

```yaml
name: ADR Test Coverage Check

on:
  pull_request:
    paths:
      - 'docs/adr/**/*.md'
  schedule:
    - cron: '0 0 * * 1'  # 每周一检查

jobs:
  check-coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Scan ADR Test Gaps
        run: ./scripts/check-adr-test-coverage.sh
        
      - name: Fail if coverage below threshold
        run: |
          coverage=$(./scripts/get-test-coverage.sh)
          if [ $coverage -lt 80 ]; then
            echo "❌ ADR 测试覆盖率 $coverage% < 80%"
            exit 1
          fi
```

### 5.3 定期审查

- **月度**：检查新增 ADR 是否同步测试
- **季度**：审查测试覆盖率，确保 ≥ 80%

---

## 六、成功标准

整改完成后，应达到以下标准：

| 指标 | 目标值 | 当前值 | 验证方式 |
|-----|-------|--------|---------|
| **整体测试覆盖率** | ≥ 80% | 24% | 自动化扫描 |
| **Runtime 层覆盖率** | 100% | 0% | Phase 1 验证 |
| **Structure 层覆盖率** | 100% | 0% | Phase 2 验证 |
| **Governance P0/P1 覆盖率** | 100% | 12% | Phase 3 验证 |
| **Technical 层覆盖率** | 100% | 0% | Phase 4 验证 |
| **新增 ADR 同步测试率** | 100% | - | CI 强制检查 |

---

## 七、风险与缓解措施

### 7.1 风险识别

| 风险 | 概率 | 影响 | 缓解措施 |
|-----|------|------|---------|
| 工作量超预期 | 中 | 高 | 分阶段实施，允许延期但保证质量 |
| 测试编写技能不足 | 中 | 中 | 提供培训和模板，Code Review 把关 |
| 部分约束难以自动化 | 高 | 中 | 转为人工审查项，记录在 ADR 中 |
| 历史代码违反新测试 | 高 | 高 | 先添加测试但允许破例，制定偿还计划 |

### 7.2 应对策略

1. **渐进式覆盖**：不要求一次性全部通过，允许暂时破例
2. **优先级调整**：根据实际情况动态调整 Phase 优先级
3. **技能培训**：组织测试开发培训，建立最佳实践库
4. **持续改进**：每个 Phase 结束后总结经验，优化流程

---

## 八、建议与下一步行动

### 关键建议

1. **立即启动 Phase 1（Runtime 层）**
   - Runtime 层缺失影响最大，必须优先补充
   - 预计 2 周内完成，不应延期

2. **建立测试开发流程**
   - 制定详细的测试编写指南
   - 建立测试 Code Review 机制
   - 创建测试模板库

3. **同步更新 ADR 审查流程**
   - 将架构测试作为 ADR 提交的必要条件
   - 在 PR 模板中增加测试覆盖检查项

4. **设置里程碑和检查点**
   - 每个 Phase 结束后进行验收
   - 月度报告测试覆盖率进展

### 下一步行动

- [ ] **Week 1**：启动 Phase 1，补充 Runtime 层测试
- [ ] **Week 2**：完成 Phase 1 验收，启动 Phase 2
- [ ] **Week 3**：完成 Phase 2，启动 Phase 3
- [ ] **Week 4**：完成 Phase 3 验收
- [ ] **Month 2**：完成 Phase 4 和 Phase 5 部分
- [ ] **Month 3**：完成所有测试，达到 80% 覆盖率目标

---

## 附录 A：完整缺失清单

### 治理层（22 个）

1. ADR-900 - ADR 流程
2. ADR-901 - 警告约束语义
3. ADR-902 - ADR 模板结构契约
4. ADR-904 - 架构测试最小断言语义
5. ADR-905 - 执行级别分类
6. ADR-906 - 分析器 CI 门映射协议
7. ADR-930 - 代码审查合规性（已有测试，疑似遗漏）
8. ADR-940 - ADR 关系可追溯性管理
9. ADR-945 - ADR 时间线演进视图
10. ADR-946 - ADR 标题层级语义约束
11. ADR-947 - 关系章节结构解析安全
12. ADR-950 - 指南与 FAQ 文档治理
13. ADR-951 - 案例仓库管理
14. ADR-952 - 工程标准 ADR 边界
15. ADR-955 - 文档搜索与可发现性
16. ADR-960 - 入职文档治理
17. ADR-965 - 入职交互式学习路径
18. ADR-970 - 自动化日志集成标准
19. ADR-975 - 文档质量监控
20. ADR-980 - ADR 生命周期同步
21. ADR-990 - 文档演进路线图

### 结构层（5 个）

1. ADR-120 - 领域事件命名约定
2. ADR-121 - Contract DTO 命名组织
3. ADR-122 - 测试组织命名
4. ADR-123 - Repository 接口分层
5. ADR-124 - Endpoint 命名约束

### 运行层（4 个）

1. ADR-201 - Handler 生命周期管理
2. ADR-210 - 事件版本化兼容性
3. ADR-220 - 事件总线集成
4. ADR-240 - Handler 异常约束

### 技术层（4 个）

1. ADR-301 - 集成测试自动化
2. ADR-340 - 结构化日志监控约束
3. ADR-350 - 日志可观测性标准
4. ADR-360 - CI/CD 流水线标准化

---

## 附录 B：快速开始指南

### 对于开发者

1. **查看自己负责的 ADR**
   ```bash
   grep -r "maintainer.*你的名字" docs/adr/
   ```

2. **检查是否缺少测试**
   ```bash
   ./scripts/check-adr-test-coverage.sh
   ```

3. **使用模板创建测试**
   - 参考 `ADR_0001_Architecture_Tests.cs`
   - 按照 §4.2 模板结构编写

4. **运行测试验证**
   ```bash
   dotnet test --filter "Category=ADR-{你的编号}"
   ```

### 对于团队负责人

1. **分配任务**：根据 Phase 计划分配责任人
2. **跟踪进度**：使用本文档 Phase 检查清单
3. **组织 Review**：每个 Phase 结束后组织验收会议
4. **更新报告**：在本文档中更新完成状态

---

## 联系与反馈

**报告维护者**：架构委员会 / Testing Team  
**生成时间**：2026-01-29  
**反馈渠道**：GitHub Issues (label: `adr-test-coverage`)

**相关资源**：
- 📘 [ADR-0000：架构测试与 CI 治理宪法](../adr/governance/ADR-0000-architecture-tests.md)
- 📘 [ADR-904：架构测试最小断言语义](../adr/governance/ADR-904-architecturetests-minimum-assertion-semantics.md)
- 📘 [ADR-930：代码审查合规性](../adr/governance/ADR-930-code-review-compliance.md)

---

**报告状态**：✅ Final  
**基于 ADR**：ADR-0000、ADR-904、ADR-930  
**下次更新**：Phase 1 完成后（预计 Week 2）
