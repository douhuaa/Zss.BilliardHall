# ArchitectureTests（架构自动化测试）

## 目的

这组测试的目的是把 **ADR-0001 至 ADR-0005 的核心静态约束** 写成可执行规则，确保架构规范能够被自动化检查并在 CI 中执行。

所有架构决策文档 (ADR) 都已映射为独立的测试类，实现了 **可执行的架构宪法**。

---

## 核心设计原则

### 架构测试一对一映射规则

**规则**：每个 ADR 必须有且仅有一个对应的测试类，每条规则必须细化为独立的测试方法。

**优势**：
- **隔离性强**：每条规则单独测试，保证测试覆盖面和准确性，规避变动互相影响
- **易于追踪和维护**：变更某一规则时，仅需维护相关类和方法，减少全局性修改风险
- **融合 ADR 文档链路**：测试与架构决策文档一一映射，方便自动化筛查和合规审查
- **便于团队协作**：不同开发者可并行负责不同规则的测试类，降低冲突几率
- **提升 CI/CD 效率**：测试失败精确定位到具体规则，缩短排查和修复时间

**命名规范**：
- 测试类：`ADR_{编号}_Architecture_Tests`（如 `ADR_0001_Architecture_Tests`）
- 测试方法：使用 `DisplayName` 属性标注 `ADR-{编号}.{子编号}: {规则描述}`
- 示例：`[Fact(DisplayName = "ADR-0001.1: 模块不应相互引用")]`

**组织结构**：
- 使用 `#region` 分组相关规则
- 每个测试方法包含：
  - 清晰的错误消息，包含 ADR 编号
  - 具体的违规类型或位置
  - 详细的修复建议
  - 相关文档引用

**强制执行**：
- ADR-0000 测试自动验证一对一映射关系
- 禁止跳过测试（Skip）
- 测试失败必须包含 ADR 编号

---

## 测试组织结构

### 三层测试架构（重要变更）

从 2026-01-25 开始，架构测试采用三层分级架构：

```
/tests/ArchitectureTests/
  ├─ Governance/    （宪法层）- 治理原则验证，不可妥协
  ├─ Enforcement/   （执法层）- 可执行硬约束，失败 = CI 阻断
  ├─ Heuristics/    （启发层）- 风格建议，永不失败构建
  └─ ADR/           （传统）- 每个 ADR 对应一个测试类
```

#### 三层设计哲学

&gt; "文档治理 ≠ 纯规则校验"  
&gt; "把所有检查都塞进一个 xUnit Test，是架构治理失败的早期症状。"

| 层级 | 本质 | 失败策略 | 示例 |
|------|------|---------|------|
| **Governance** | 宪法级规则 | ❌ 不允许破例 | 裁决权归属、文档分级定义 |
| **Enforcement** | 可执行硬约束 | ⚠️ 允许登记破例 | README 禁用词、权威声明要求 |
| **Heuristics** | 风格/质量启发 | ✅ 永不失败 | 文档长度建议、示例完整性 |

**为什么需要三层？**

- **Governance**: 定义什么是"合法的治理边界"，而不是怎么执行
- **Enforcement**: 把宪法结论变成可执行规则，机械执行
- **Heuristics**: 品味和建议，避免"要么放水、要么内耗"

#### 重构案例：ADR-0008

ADR-0008（文档治理宪法）是第一个完成三层拆分的测试：

- **Governance 层**: `ADR_0008_Governance_Tests.cs` - 验证治理边界定义
- **Enforcement 层**:
  - `DocumentationDecisionLanguageTests.cs` - README 裁决语言检查
  - `DocumentationAuthorityDeclarationTests.cs` - Instructions/Agents 权威声明
  - `SkillsJudgmentLanguageTests.cs` - Skills 判断性语言检查
  - `AdrStructureTests.cs` - ADR 结构验证
- **Heuristics 层**: `DocumentationStyleHeuristicsTests.cs` - 风格建议

---

### ADR 目录（核心测试套件）

位于 `ADR/` 子目录下，每个 ADR 文档对应一个测试类：

#### ADR-0000: 架构测试元规则

- **测试类**: `ADR_0000_Architecture_Tests`
- **目的**: 确保每条 ADR 都有唯一对应的架构测试类
- **核心约束**: ADR 与测试类的一一映射关系

#### ADR-0001: 模块化单体与垂直切片架构

- **测试类**: `ADR_0001_Architecture_Tests`
- **测试数量**: 11 个测试
- **核心约束**:
  - 模块隔离（模块间不能互相引用）
  - 垂直切片组织（禁止传统分层命名空间）
  - 契约使用规则（Command Handler 不依赖 IQuery）
  - Handler 自包含（不依赖横向 Service）
  - Platform 层限制（不包含业务逻辑）
  - 契约是简单数据结构（不含业务方法）

#### ADR-0002: Platform / Application / Host 三层启动体系

- **测试类**: `ADR_0002_Architecture_Tests`
- **测试数量**: 13 个测试
- **核心约束**:
  - Platform 不依赖 Application/Host
  - Application 不依赖 Host/Modules
  - Host 不依赖 Modules/不包含业务逻辑
  - Bootstrapper 入口点验证
  - Program.cs 简洁性（≤50 行）
  - 三层依赖方向验证

#### ADR-0003: 命名空间与项目边界规范

- **测试类**: `ADR_0003_Architecture_Tests`
- **测试数量**: 9 个测试
- **核心约束**:
  - 所有类型命名空间以 Zss.BilliardHall 开头
  - Platform/Application/Modules/Host 类型命名空间规范
  - Directory.Build.props 存在性和配置
  - 禁止不规范命名空间模式

#### ADR-0004: 中央包管理 (CPM) 规范

- **测试类**: `ADR_0004_Architecture_Tests`
- **测试数量**: 9 个测试
- **核心约束**:
  - Directory.Packages.props 存在性和配置
  - CPM 启用和传递依赖固定
  - 项目不手动指定包版本
  - 包分组约束
  - 测试框架版本一致性

#### ADR-0005: 应用内交互模型与执行边界

- **测试类**: `ADR_0005_Architecture_Tests`
- **测试数量**: 12 个测试
- **核心约束**:
  - Handler 命名约定（Command/Query/Event）
  - Handler 不依赖 ASP.NET 类型
  - Handler 无状态约束
  - 模块间异步通信
  - Command/Query 分离
  - Endpoint 业务逻辑检查

---

## 测试统计

- **ADR 测试类**: 6 个（ADR-0000 至 ADR-0005）
- **覆盖率**: 100% ADR 约束覆盖

### 架构演进说明

传统测试类（PlatformDependencyTests、ModuleIsolationTests、NamespaceTests、ContractUsageTests、VerticalSliceArchitectureTests、HostIsolationTests、InfrastructureTests、PlatformLayerTests）已被淘汰，理由如下：

1. **重复覆盖**：ADR 测试已完整覆盖模块隔离、命名空间、契约、Platform/Host/Module 依赖等所有核心约束
2. **维护成本**：单一测试套件降低了维护负担，架构约束变更时只需修改 ADR 测试
3. **CI 效率**：减少冗余测试执行，降低 CI 时间和误报风险

所有架构约束现在通过 ADR 测试统一执行和维护。

---

## 本地运行

在解决方案根目录运行：

```bash
# 先编译（保证模块程序集存在）
dotnet build

# 再跑架构测试
dotnet test src/tests/ArchitectureTests
```

如需指定配置（CI 使用 Release）：

```bash
dotnet build -c Release
export Configuration=Release
dotnet test src/tests/ArchitectureTests -c Release
```

## CI 集成

架构测试已集成到 GitHub Actions 工作流中（`.github/workflows/architecture-tests.yml`），在以下情况自动运行：

- Push 到 `main` 分支
- 针对 `main` 分支的 Pull Request

如果架构测试失败，CI 将阻断合并，确保架构规范得到严格执行。

## 常见问题

### 问题：测试失败提示"未找到模块程序集"

**原因**：模块尚未构建，测试无法加载 DLL。

**解决方法**：

```bash
dotnet build
dotnet test src/tests/ArchitectureTests
```

### 问题：本地通过但 CI 失败

**原因**：本地使用 Debug 配置，CI 使用 Release 配置。

**解决方法**：

```bash
dotnet build -c Release
export Configuration=Release
dotnet test src/tests/ArchitectureTests -c Release
```

### 问题：架构测试报告违规

**处理步骤**：

1. 查看测试输出，了解违规类型和位置
2. 根据修复建议调整代码结构
3. 重新运行测试验证修复

## 扩展建议

### 已实现的 ADR 测试覆盖

✅ **ADR-0000**: 架构测试元规则  
✅ **ADR-0001**: 模块化单体与垂直切片架构  
✅ **ADR-0002**: Platform / Application / Host 三层启动体系  
✅ **ADR-0003**: 命名空间与项目边界规范  
✅ **ADR-0004**: 中央包管理 (CPM) 规范  
✅ **ADR-0005**: 应用内交互模型与执行边界

### 未来增强方向

1. **引入 Roslyn Analyzer**：做语义级别的静态检查
  - 详见 [ADR-0005 执行级别分类](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)
  - Level 2 规则建议使用 Roslyn Analyzer 实现
2. **添加更多规则**：
  - 异常处理规范（使用 DomainException）
  - 数据库事务边界约束
  - 日志记录规范
3. **格式化失败信息**：在 PR 模板中强制 ARCH-VIOLATION 字段
  - 已实现：见 [.github/PULL_REQUEST_TEMPLATE.md](../../.github/PULL_REQUEST_TEMPLATE.md)
  - 破例记录：见 [docs/summaries/governance/arch-violations.md](../../docs/summaries/governance/arch-violations.md)
4. **性能测试**：确保架构测试运行时间控制在合理范围内（当前 ~1s）
5. **覆盖率报告**：生成 ADR 约束覆盖率报告

---

## 架构治理体系（Architecture Governance）

本测试套件是完整架构治理体系的一部分：

### 📜 架构宪法层

**ADR-0000 至 ADR-0005 构成系统的"架构宪法层"**，不可被后续 ADR 推翻：

- 详见 [架构宪法层文档](../../docs/adr/ARCHITECTURE-CONSTITUTIONAL-LAYER.md)
- 这些 ADR 只能细化，不能削弱
- 破例需要架构委员会审批

### 🛡️ 三层防御体系

架构约束通过三层机制执行：

1. **Level 1 - 静态可执行**（当前测试覆盖）
  - NetArchTest 自动化检查
  - CI 自动阻断
  - 零容忍

2. **Level 2 - 语义半自动**（部分覆盖，建议增强）
  - 当前：启发式检查（建议性）
  - 建议：Roslyn Analyzer（自定义分析器）
  - 详见 [ADR-0005 执行级别分类](../../docs/adr/constitutional/ADR-0005-Enforcement-Levels.md)

3. **Level 3 - 人工 Gate**（流程控制）
  - PR 模板强制架构违规声明
  - 架构师 Code Review
  - 破例记录在 [arch-violations.md](../../docs/summaries/governance/arch-violations.md)

### 🔍 反作弊规则

ADR-0000 现已包含反作弊机制，确保架构测试不能被"形式化"：

- ✅ 测试类必须包含实质性测试（不能是空壳）
- ✅ 禁止跳过架构测试（禁止使用 `Skip` 属性）
- ✅ 测试 DisplayName 必须包含 ADR 编号（便于追溯）

**重要：反作弊规则的设计哲学**

> **这些规则是治理工具，不是审判工具。**

反作弊规则使用启发式检测（如 IL 字节数、Skip 检测），可能产生误报。我们的处理原则：

- ✅ **误报优先修规则**：如果反作弊规则误伤了合理的测试代码，应修改规则本身
- ❌ **不要求业务代码妥协**：不应为了通过反作弊检测而修改合理的架构测试
- 📝 **记录误报案例**：所有误报都应记录，用于改进规则精度

如果遇到误报，请在 Issue 中报告，并在 PR 中说明情况。

---

## CI 集成

架构测试已集成到 GitHub Actions CI 流程中（`.github/workflows/architecture-tests.yml`）：

- **触发条件**：Push 或 Pull Request 到 main 分支
- **测试环境**：Ubuntu Latest + .NET 10.0.x
- **失败阻断**：如果任何架构测试失败，CI 将返回非 0 退出码，阻止 PR 合并

CI 工作流程：

1. Checkout 代码
2. 设置 .NET SDK
3. 恢复依赖 (`dotnet restore`)
4. 构建所有项目（排除 docs）
5. 运行架构测试 (`dotnet test`)

---

后续可以增强架构测试：

1. **引入 Roslyn Analyzer**：做语义级别的静态检查
2. **添加更多规则**：如异步方法命名约定、异常处理规范等
3. **格式化失败信息**：在 PR 模板中强制 ARCH-VIOLATION 字段
4. **性能测试**：确保架构测试运行时间控制在合理范围内

---

## 架构决策的可执行性

本测试套件实现了 **ADR 作为可执行宪法** 的理念：

1. **一一对应**: 每条 ADR 文档都有唯一对应的测试类
2. **可验证**: 所有架构约束都转化为自动化测试
3. **CI 集成**: 架构测试失败 = 构建失败 = PR 阻断
4. **可追溯**: 测试失败时明确指出违反的 ADR 编号和修复建议
5. **可演进**: 新增 ADR 时必须同时添加对应的测试类

> **注意**：这不是一个 MVP 实现，而是完整的 ADR 到测试的映射，确保架构决策得到严格执行。

