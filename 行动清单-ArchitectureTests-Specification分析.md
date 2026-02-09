# ArchitectureTests.Specification 现状分析 - 行动清单

**日期**: 2026-02-09  
**仓库**: douhuaa/Zss.BilliardHall  
**分析类型**: 静态分析（无代码变更）

---

## 🎯 核心发现（5 项）

### 1. ❌ ArchitectureTests.Specification 目录不存在
- 仓库中完全没有此目录或命名空间
- 无 `.Specification` 相关代码文件
- **可能是计划中但未实现的功能**

### 2. ✅ 当前架构测试组织完善
- 位置: `/src/tests/ArchitectureTests/`
- 采用三层架构（自 2026-01-25）：
  - `Governance/` - 宪法层（1 文件）
  - `Enforcement/` - 执法层（4 文件）
  - `Heuristics/` - 启发层（1 文件）
  - `ADR/` - 传统层（26 文件）

### 3. ❌ 无 Adr907 相关内容
- 搜索整个仓库未发现
- 现有 ADR 编号: 0-8, 120-124, 201-360, 900-930
- 907 不在已实现范围内

### 4. ✅ 构建成功，有 8 个警告
- 警告全部来自 `/src/tools/ArchitectureAnalyzers/`
- ArchitectureTests 项目本身无问题
- 类型: RS1036 (3个), RS2008 (3个), CS8604 (2个)

### 5. ⚠️ 发现技术债务
- 硬编码配置分散
- 大型测试类（最大 582 行）
- 反射使用频繁（172 次）

---

## 📊 文件清单（33 个 C# 文件）

### ADR 测试（26 个文件，~4,500 行）
```
ADR_0000 (313行) - 架构测试元规则
ADR_0001 (265行) - 模块隔离、垂直切片
ADR_0002 (498行) - Platform/Application/Host 边界
ADR_0003 (336行) - 命名空间规范
ADR_0004 (432行) - 中央包管理 (CPM)
ADR_0005 (539行) - Handler 规则、CQRS
ADR_0006 (261行) - 术语与编号规范
ADR_0007 (323行) - Agent 行为与权限
ADR_0008 (~120行) - 文档治理
ADR_0120 (347行) - 领域事件命名
ADR_0121 (423行) - 数据契约规范
ADR_0122, 0123, 0124 - 查询/命令/事件对象
ADR_0201, 0210, 0220, 0240 - 生命周期/事务/异常
ADR_0301, 0340, 0350, 0360 - 数据库/Repository/实体/值对象
ADR_0900 (239行) - ADR-Test-Prompts 映射
ADR_0930, 0910, 0920 - 测试/Instructions/Agents 规范
```

### Enforcement 层（4 个文件，~467 行）
```
AdrStructureTests.cs (100行) - ADR 必需章节验证
DocumentationDecisionLanguageTests.cs (145行) - README 裁决语言
DocumentationAuthorityDeclarationTests.cs (142行) - 权威声明
SkillsJudgmentLanguageTests.cs (80行) - 判断性语言
```

### Governance 层（1 个文件，~180 行）
```
ADR_0008_Governance_Tests.cs - 治理边界定义
```

### Heuristics 层（1 个文件，~210 行）
```
DocumentationStyleHeuristicsTests.cs - 风格建议（永不失败）
```

### 辅助类（1 个文件，269 行）
```
TestData.cs - ModuleAssemblyData, HostAssemblyData
```

---

## ⚠️ 潜在问题识别

### 高风险问题（需立即处理）
1. **硬编码配置分散** - 路径、TFM、依赖白名单散布在多个文件
2. **错误处理不足** - DLL 加载失败仅记录日志，测试可能无声失败
3. **ArchitectureAnalyzers 警告** - 8 个警告可能影响 Roslyn 分析器稳定性

### 中风险问题（需计划处理）
1. **反射开销大** - 每次测试加载所有程序集（~3-5 秒）
2. **大型测试类** - ADR_920 (582行), ADR_0005 (539行) 需拆分
3. **文档扫描重复** - 多个测试类重复扫描相同文档

### 低风险问题（可延迟处理）
1. **代码重复** - `FindRepositoryRoot()` 在多个文件中重复
2. **文档格式不一致** - ADR 使用多种标题变体（"状态" vs "地位"）

---

## 📋 可执行行动清单（按优先级）

### 🔴 第一阶段：紧急修复（1-2 天）

#### 任务 1.1：修复 ArchitectureAnalyzers 警告
- [ ] 在 `ArchitectureAnalyzers.csproj` 添加 `<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>`
- [ ] 在 `CrossModuleCallAnalyzer.cs` 第 68、69 行添加 null 检查
- [ ] 创建 `AnalyzerReleases.Unshipped.md` 和 `AnalyzerReleases.Shipped.md`
- [ ] 运行 `dotnet build` 确认警告消失

**预期结果**: 8 个警告 → 0 个警告

#### 任务 1.2：改进错误处理
- [ ] 修改 `TestData.cs` 第 83 行：将 `Debug.WriteLine()` 改为 `throw new InvalidOperationException()`
- [ ] 添加详细错误消息（包含路径、配置、修复建议）
- [ ] 运行测试验证错误消息清晰度

**预期结果**: 测试失败时立即明确报错，而非无声失败

---

### 🟡 第二阶段：架构改进（3-5 天）

#### 任务 2.1：创建配置抽象层
- [ ] 创建 `src/tests/ArchitectureTests/Configuration/ArchitectureTestConfiguration.cs`
- [ ] 定义静态属性：`ModulesPath`, `HostPath`, `AdrDocsPath`, `SupportedTfms`, `AllowedDependencies`
- [ ] 重构 `TestData.cs` 使用新配置
- [ ] 重构所有 ADR 测试类中的硬编码路径
- [ ] 运行全部测试确认无回归

**预期结果**: 配置集中管理，易于维护

#### 任务 2.2：引入程序集缓存机制
- [ ] 创建 `src/tests/ArchitectureTests/Infrastructure/AssemblyCache.cs`
- [ ] 实现缓存逻辑：检查缓存有效性 → 读取或重新扫描 → 保存缓存
- [ ] 缓存文件位置：`%TEMP%/ArchitectureTests.AssemblyCache.json`
- [ ] 修改 `TestData.cs` 使用缓存
- [ ] 性能测试：对比缓存前后的测试启动时间

**预期结果**: 首次运行后，后续测试启动时间减少 50-70%

#### 任务 2.3：拆分大型测试类
- [ ] 拆分 `ADR_920_Architecture_Tests.cs` (582行) → 3-4 个 partial class
  - `ADR_920.AgentStructure.cs` - Agent 结构测试
  - `ADR_920.SkillsValidation.cs` - Skills 验证测试
  - `ADR_920.BehaviorConstraints.cs` - 行为约束测试
- [ ] 拆分 `ADR_0005_Architecture_Tests.cs` (539行) → 3 个 partial class
  - `ADR_0005.Handler.cs` - Handler 相关测试
  - `ADR_0005.Endpoint.cs` - Endpoint 相关测试
  - `ADR_0005.Contract.cs` - Contract 相关测试
- [ ] 拆分 `ADR_0002_Architecture_Tests.cs` (498行) → 3 个 partial class
  - `ADR_0002.Platform.cs` - Platform 层测试
  - `ADR_0002.Application.cs` - Application 层测试
  - `ADR_0002.Host.cs` - Host 层测试
- [ ] 运行全部测试确认无回归

**预期结果**: 单个文件不超过 200 行，提高可读性

---

### 🟢 第三阶段：质量提升（持续）

#### 任务 3.1：场景数据驱动化
- [ ] 创建 `src/tests/ArchitectureTests/Services/DocumentationScanService.cs`
- [ ] 实现延迟加载的文档扫描服务
- [ ] 重构 Enforcement/Governance/Heuristics 层使用新服务
- [ ] 性能测试：对比重构前后的文档扫描次数

**预期结果**: 文档扫描从 N 次 → 1 次（共享结果）

#### 任务 3.2：提取共享测试基类
- [ ] 创建 `src/tests/ArchitectureTests/ArchitectureTestBase.cs`
- [ ] 移动 `FindRepositoryRoot()` 到基类
- [ ] 添加常用辅助方法（如 `GetAdrPath()`, `GetTestPath()`）
- [ ] 重构测试类继承基类

**预期结果**: 消除代码重复，提高可维护性

#### 任务 3.3：文档格式标准化
- [ ] 在 `AdrStructureTests.cs` 中使用正则表达式匹配章节
- [ ] 支持多种标题变体：`^\*\*(?:状态|Status|地位)\*\*\s*[:：]`
- [ ] 运行测试确认无误报

**预期结果**: 更灵活的文档格式检查，减少误报

---

## ❓ 需人工确认的关键问题（7 个）

### Q1. ArchitectureTests.Specification 的预期用途
**问题**: 仓库中完全没有此目录，这是：
- [ ] 选项 A: 计划中但未实现的功能？
- [ ] 选项 B: 历史遗留术语，已被三层架构替代？
- [ ] 选项 C: 误解，实际想查询其他内容？

**需要决定**: 
- 是否创建此目录？
- 如果创建，用途是什么？
- 如果不创建，是否更新文档删除引用？

---

### Q2. 生产 RuleSet 的实际类型名
**问题**: 项目使用 `NetArchTest.Rules`，无自定义 `RuleSet`。是否需要：
- [ ] 选项 A: 继续使用 NetArchTest 内置 API（推荐）
- [ ] 选项 B: 创建自定义 `ArchitectureRuleSet` 基类
- [ ] 选项 C: 引入其他框架（如 ArchUnitNET）

**建议**: 选项 A（当前方案已足够）

---

### Q3. 测试中使用反射退化策略的接受度
**问题**: TestData.cs 大量使用反射，性能开销较大。接受：
- [ ] 策略 A: 保持现状（简单但慢，首次 ~3-5s）
- [ ] 策略 B: 引入缓存（推荐，后续 ~0.5-1s）
- [ ] 策略 C: 迁移到 Roslyn Analyzers（最佳但工作量大）

**建议**: 短期策略 B，长期考虑策略 C

---

### Q4. 场景数据文件格式偏好
**问题**: 如果外部化测试场景数据，偏好：
- [ ] JSON - 标准、类型安全、C# 支持好
- [ ] YAML - 可读性强、支持注释

**建议**: JSON（更好的工具支持）

---

### Q5. ADR 编号跳跃的原因
**问题**: 现有 ADR 编号跳跃（0-8, 120-124, 201-360, 900-930），缺失段是：
- [ ] 保留用于未来扩展？
- [ ] 已废弃/删除？
- [ ] 按主题分组（1xx=事件, 2xx=生命周期, 3xx=数据）？

**需要明确**: ADR 编号规范，指导未来创建

---

### Q6. Adr907 的实际含义
**问题**: 问题陈述提到 "Adr907"，但仓库中不存在。可能是：
- [ ] 拼写错误（ADR-0907）？
- [ ] 已计划但未创建的 ADR？
- [ ] 外部文档引用？

**需要明确**: 
- Adr907 是否应该存在？
- 如果应该存在，预期内容是什么？

---

### Q7. 三层架构的演进方向
**问题**: 当前三层（Governance/Enforcement/Heuristics）+ ADR 并存，是否：
- [ ] 选项 A: 保持现状（4 个目录）
- [ ] 选项 B: 增加第 4 层（如 Performance）
- [ ] 选项 C: 将 ADR 测试按层级归类

**潜在问题**: ADR 与三层架构并存可能导致重复

---

## 📈 预期收益

### 短期收益（第一阶段完成后）
- ✅ 消除所有编译警告
- ✅ 测试失败时错误消息更清晰
- ✅ 提高开发者信心

### 中期收益（第二阶段完成后）
- ✅ 测试启动时间减少 50-70%
- ✅ 配置维护成本降低 80%
- ✅ 代码可读性提高（单文件 <200 行）

### 长期收益（第三阶段完成后）
- ✅ 消除代码重复
- ✅ 文档扫描次数减少 N 倍
- ✅ 更灵活的扩展性

---

## 📝 实施建议

### 推荐路径
1. **立即开始**: 第一阶段（紧急修复），解决编译警告和错误处理
2. **2 周内完成**: 第二阶段（架构改进），创建配置抽象和缓存
3. **持续优化**: 第三阶段（质量提升），根据团队反馈调整

### 风险控制
- ✅ 每个任务完成后运行全部测试
- ✅ 使用 feature branch，PR 审查后合并
- ✅ 保留原有代码的注释，便于回滚

### 需要的资源
- **开发时间**: 约 8-10 天（分散到 2-3 周）
- **测试时间**: 每个阶段 1-2 天
- **Code Review**: 每个 PR 约 30-60 分钟

---

## 📚 相关文档

### 生成的报告文件
- `/tmp/ArchitectureTests-Specification-Analysis-Report.md` - 详细技术分析报告（英文）
- `/tmp/行动清单-ArchitectureTests-Specification分析.md` - 本文件（中文行动清单）

### 仓库内相关文档
- `src/tests/ArchitectureTests/README.md` - 架构测试说明
- `docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md` - 文档治理宪法
- `.github/instructions/testing.instructions.md` - 测试编写指令

---

**报告状态**: ✅ 完成  
**下一步**: 等待人工确认 7 个关键问题，然后开始第一阶段实施  
**联系人**: 架构委员会 / @douhuaa

