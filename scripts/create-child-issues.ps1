# 批量创建子任务 Issues
# Epic: RuleSet-as-Source-of-Truth

Write-Host "创建剩余的子任务 Issues..." -ForegroundColor Cyan
Write-Host ""

$repo = "douhuaa/Zss.BilliardHall"
$epicIssue = "367"  # 从前面脚本获取

# 获取 Milestone 信息
$milestones = gh api "/repos/$repo/milestones" | ConvertFrom-Json
$m1 = ($milestones | Where-Object { $_.title -eq "Tools & API Integration" }).number
$m2 = ($milestones | Where-Object { $_.title -eq "Test & Documentation Generation" }).number
$m3 = ($milestones | Where-Object { $_.title -eq "CI Integration & Validation" }).number

Write-Host "Milestones:" -ForegroundColor Yellow
Write-Host "  M1 (Tools & API Integration): #$m1" -ForegroundColor White
Write-Host "  M2 (Test & Documentation Generation): #$m2" -ForegroundColor White
Write-Host "  M3 (CI Integration & Validation): #$m3" -ForegroundColor White
Write-Host ""

# ============================================================
# 阶段 1: 构建生成工具
# ============================================================

Write-Host "阶段 1: 构建生成工具" -ForegroundColor Magenta

# Issue 1.1
$issue11Body = @"
## 任务描述
实现 RuleSet → ADR Decision 自动生成器

## 技术规格
- 接口: IAdrDecisionGenerator
- 输入: RuleSet 对象
- 输出: Markdown 格式的 Decision 章节
- 保留: Context/Consequences 内容

## 检查清单
- [ ] 设计接口 IAdrDecisionGenerator
- [ ] 实现 Markdown 格式化器
- [ ] 实现 RuleId 映射逻辑 (ADR-XXX_Y_Z)
- [ ] 实现 Context/Consequences 保留机制
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成内容格式正确
- ✅ RuleId 映射准确
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m1
"@

gh issue create `
    --title "[Phase 1] 实现 ADR Decision 生成器" `
    --label "phase-1,priority:high,type:feature,milestone-1" `
    --milestone "$m1" `
    --body $issue11Body
Write-Host "  ✅ Issue 1.1 创建成功" -ForegroundColor Green

# Issue 1.2
$issue12Body = @"
## 任务描述
实现 RuleSet → ArchitectureTests 自动生成器

## 技术规格
- 接口: IArchitectureTestGenerator
- 输入: RuleSet 对象
- 输出: xUnit 测试类代码
- 框架: NetArchTest.Rules

## 检查清单
- [ ] 设计接口 IArchitectureTestGenerator
- [ ] 实现 xUnit 测试模板
- [ ] 实现 NetArchTest 断言生成
- [ ] 生成测试类/方法命名逻辑
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成测试代码可编译
- ✅ 测试命名符合规范
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m1
"@

gh issue create `
    --title "[Phase 1] 实现测试生成器" `
    --label "phase-1,priority:high,type:feature,milestone-1" `
    --milestone "$m1" `
    --body $issue12Body
Write-Host "  ✅ Issue 1.2 创建成功" -ForegroundColor Green

# Issue 1.3
$issue13Body = @"
## 任务描述
实现 RuleSet → Agent Instructions 自动生成器

## 技术规格
- 接口: IAgentInstructionGenerator
- 输入: RuleSet 对象
- 输出: YAML 格式的 Agent 指令
- 包含: RuleSet API 查询示例

## 检查清单
- [ ] 设计接口 IAgentInstructionGenerator
- [ ] 实现 YAML 格式化器
- [ ] 生成 RuleSet API 查询示例
- [ ] 生成约束检查逻辑
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 接口定义清晰
- ✅ 生成 YAML 格式正确
- ✅ API 查询示例可用
- ✅ 单元测试覆盖率 > 80%
- ✅ 代码审查通过

## 时间估算
- 1-1.5 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m1
"@

gh issue create `
    --title "[Phase 1] 实现 Agent 指令生成器" `
    --label "phase-1,priority:medium,type:feature,milestone-1" `
    --milestone "$m1" `
    --body $issue13Body
Write-Host "  ✅ Issue 1.3 创建成功" -ForegroundColor Green

Write-Host ""

# ============================================================
# 阶段 2: Agent/Skills 集成
# ============================================================

Write-Host "阶段 2: Agent/Skills 集成" -ForegroundColor Magenta

# Issue 2.1
$issue21Body = @"
## 任务描述
重构所有 Agent，使用 RuleSetRegistry API 替代 Markdown 解析

## Agent 列表
1. Architecture Guardian
2. ADR Reviewer
3. Test Generator
4. Module Boundary Checker
5. Handler Pattern Enforcer
6. Documentation Maintainer
7. Expert Dotnet Engineer

## 检查清单
- [ ] Architecture Guardian - 使用 RuleSet 验证变更
- [ ] ADR Reviewer - 查询 RuleSet 检查一致性
- [ ] Test Generator - 基于 RuleSet 生成测试
- [ ] Module Boundary Checker - 查询边界规则
- [ ] Handler Pattern Enforcer - 查询 Handler 约束
- [ ] Documentation Maintainer - 同步 RuleSet 更新
- [ ] Expert Dotnet Engineer - 查询技术规范
- [ ] 移除所有硬编码 ADR 文本引用
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 9 个 Agent 使用 RuleSetRegistry API
- ✅ 移除所有硬编码 ADR 引用
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 2-2.5 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m1
"@

gh issue create `
    --title "[Phase 2] 更新 9 个 Agent 使用 RuleSet API" `
    --label "phase-2,priority:high,type:refactor,milestone-1" `
    --milestone "$m1" `
    --body $issue21Body
Write-Host "  ✅ Issue 2.1 创建成功" -ForegroundColor Green

# Issue 2.2
$issue22Body = @"
## 任务描述
重构所有 Skills，使用 RuleSetRegistry API 替代硬编码规则

## Skills 列表
1. generate-test
2. generate-adr
3. generate-handler
4. generate-endpoint
5. run-architecture-tests
6. scan-cross-module-refs
7. update-documentation

## 检查清单
- [ ] generate-test - 基于 RuleSet 生成测试代码
- [ ] generate-adr - 使用生成器更新 Decision
- [ ] generate-handler - 查询 Handler 约束
- [ ] generate-endpoint - 查询 API 规范
- [ ] run-architecture-tests - 报告 RuleId
- [ ] scan-cross-module-refs - 查询边界规则
- [ ] update-documentation - 同步 RuleSet 变更
- [ ] 移除所有硬编码规则
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 9 个 Skills 使用 RuleSetRegistry API
- ✅ 移除所有硬编码规则
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m1
"@

gh issue create `
    --title "[Phase 2] 更新 9 个 Skills 使用 RuleSet API" `
    --label "phase-2,priority:high,type:refactor,milestone-1" `
    --milestone "$m1" `
    --body $issue22Body
Write-Host "  ✅ Issue 2.2 创建成功" -ForegroundColor Green

Write-Host ""

# ============================================================
# 阶段 3: 批量生成测试
# ============================================================

Write-Host "阶段 3: 批量生成测试" -ForegroundColor Magenta

$issue31Body = @"
## 任务描述
为 38 个未完整覆盖的 RuleSet 生成业务规则测试

## 目标
- 测试覆盖率: 5/43 → 43/43 (100%)
- 测试总数: 321 → 500+
- 新增测试: 200-300 个

## 检查清单
- [ ] 识别 38 个未完整覆盖的 RuleSet
- [ ] 使用测试生成器批量生成
- [ ] 人工审查前 10 个生成的测试
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余测试
- [ ] 运行测试套件
- [ ] 修复失败测试
- [ ] 验证测试覆盖率达到 100%
- [ ] 代码审查

## 验收标准
- ✅ 43/43 RuleSet 均有完整业务规则测试
- ✅ 所有新生成测试通过
- ✅ 测试命名符合 ADR-XXX_Y_Z 规范
- ✅ 测试总数 > 500
- ✅ 代码审查通过

## 时间估算
- 3-4 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m2
"@

gh issue create `
    --title "[Phase 3] 批量生成测试套件" `
    --label "phase-3,priority:high,type:feature,milestone-2" `
    --milestone "$m2" `
    --body $issue31Body
Write-Host "  ✅ Issue 3.1 创建成功" -ForegroundColor Green

Write-Host ""

# ============================================================
# 阶段 4: 重新生成 ADR
# ============================================================

Write-Host "阶段 4: 重新生成 ADR" -ForegroundColor Magenta

$issue41Body = @"
## 任务描述
为 43 个 ADR 重新生成 Decision 章节，保留人工编写的 Context/Consequences

## 目标
- 重新生成: 43 个 ADR Decision 章节
- 格式统一: ADR-XXX_Y_Z
- 版本同步: 与 RuleSet 版本一致

## 检查清单
- [ ] 备份现有 43 个 ADR 文档
- [ ] 使用 Decision 生成器批量生成
- [ ] 人工审查前 5 个生成结果
- [ ] 调整模板（如需要）
- [ ] 批量生成剩余 ADR Decision 章节
- [ ] 验证 RuleId 格式 (ADR-XXX_Y_Z)
- [ ] 验证 Context/Consequences 保留
- [ ] 更新 ADR-RELATIONSHIP-MAP
- [ ] 版本号同步检查
- [ ] 人工审查通过

## 验收标准
- ✅ 43 个 ADR Decision 章节重新生成
- ✅ Context/Consequences 内容保留
- ✅ RuleId 格式正确
- ✅ 版本号与 RuleSet 同步
- ✅ ADR-RELATIONSHIP-MAP 更新
- ✅ 人工审查通过

## 时间估算
- 2-3 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m2
"@

gh issue create `
    --title "[Phase 4] 重新生成 ADR Decision 章节" `
    --label "phase-4,priority:high,type:feature,milestone-2" `
    --milestone "$m2" `
    --body $issue41Body
Write-Host "  ✅ Issue 4.1 创建成功" -ForegroundColor Green

Write-Host ""

# ============================================================
# 阶段 5: CI 集成
# ============================================================

Write-Host "阶段 5: CI 集成" -ForegroundColor Magenta

# Issue 5.1
$issue51Body = @"
## 任务描述
实现 RuleSet ↔ ADR 和 RuleSet ↔ 测试的一致性检查器

## 检查器列表
1. RuleSet ↔ ADR 一致性检查器
2. RuleSet ↔ 测试覆盖度检查器

## 检查清单
- [ ] 设计检查器接口
- [ ] 实现 RuleSet ↔ ADR 一致性检查器
- [ ] 实现 RuleSet ↔ 测试覆盖度检查器
- [ ] 实现差异报告生成
- [ ] 单元测试（覆盖率 > 80%）
- [ ] 集成测试
- [ ] 代码审查

## 验收标准
- ✅ 2 个检查器实现完成
- ✅ 差异报告清晰
- ✅ 单元测试覆盖率 > 80%
- ✅ 集成测试通过
- ✅ 代码审查通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m3
"@

gh issue create `
    --title "[Phase 5] 实现一致性检查器" `
    --label "phase-5,priority:high,type:feature,milestone-3" `
    --milestone "$m3" `
    --body $issue51Body
Write-Host "  ✅ Issue 5.1 创建成功" -ForegroundColor Green

# Issue 5.2
$issue52Body = @"
## 任务描述
将一致性检查集成到 CI Pipeline，PR 合并前强制验证

## 检查清单
- [ ] 更新 .github/workflows/architecture-tests.yml
- [ ] 添加一致性检查步骤
- [ ] 配置 PR 合并前强制验证
- [ ] 配置失败时阻断合并
- [ ] 测试 CI 流程
- [ ] 文档更新

## 验收标准
- ✅ CI 集成完成
- ✅ PR 合并前自动验证
- ✅ 失败时阻断合并
- ✅ 测试 CI 流程通过
- ✅ 文档更新完成

## 时间估算
- 1 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m3
"@

gh issue create `
    --title "[Phase 5] CI 流程集成" `
    --label "phase-5,priority:high,type:feature,milestone-3" `
    --milestone "$m3" `
    --body $issue52Body
Write-Host "  ✅ Issue 5.2 创建成功" -ForegroundColor Green

Write-Host ""

# ============================================================
# 阶段 6: 验证与优化
# ============================================================

Write-Host "阶段 6: 验证与优化" -ForegroundColor Magenta

# Issue 6.1
$issue61Body = @"
## 任务描述
端到端验证整个 RuleSet-as-Source-of-Truth 系统

## 验证场景
1. 创建新 RuleSet
2. 更新现有 RuleSet
3. Agent 查询 RuleSet

## 检查清单
- [ ] 创建新 RuleSet 测试
- [ ] 更新现有 RuleSet 测试
- [ ] Agent 查询功能测试
- [ ] CI 流程完整性测试
- [ ] 性能基准测试
- [ ] 边界情况测试
- [ ] 回归测试

## 验收标准
- ✅ 所有端到端测试通过
- ✅ 性能基准达标
- ✅ 边界情况覆盖
- ✅ 回归测试通过

## 时间估算
- 1.5-2 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m3
"@

gh issue create `
    --title "[Phase 6] 端到端验证" `
    --label "phase-6,priority:high,type:test,milestone-3" `
    --milestone "$m3" `
    --body $issue61Body
Write-Host "  ✅ Issue 6.1 创建成功" -ForegroundColor Green

# Issue 6.2
$issue62Body = @"
## 任务描述
优化 RuleSetRegistry 加载、生成器执行、CI 流程性能

## 性能目标
- RuleSetRegistry 加载时间: < 1s
- 生成器执行时间: < 10s/RuleSet
- CI 执行时间: < 5min

## 检查清单
- [ ] RuleSetRegistry 加载性能测试
- [ ] 生成器性能基准测试
- [ ] CI 执行时间分析
- [ ] 缓存机制设计（如需要）
- [ ] 并发优化（如需要）
- [ ] 性能测试验证
- [ ] 文档更新

## 验收标准
- ✅ 加载时间 < 1s
- ✅ 生成时间 < 10s/RuleSet
- ✅ CI 执行时间 < 5min
- ✅ 性能测试通过
- ✅ 文档更新完成

## 时间估算
- 1-1.5 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m3
"@

gh issue create `
    --title "[Phase 6] 性能优化" `
    --label "phase-6,priority:medium,type:optimization,milestone-3" `
    --milestone "$m3" `
    --body $issue62Body
Write-Host "  ✅ Issue 6.2 创建成功" -ForegroundColor Green

# Issue 6.3
$issue63Body = @"
## 任务描述
更新所有相关文档，包括架构指南、Agent 手册、贡献指南

## 文档清单
1. 架构指南
2. Agent/Skills 手册
3. 贡献指南
4. API 文档

## 检查清单
- [ ] 更新架构指南
- [ ] 更新 Agent/Skills README
- [ ] 编写 RuleSet API 使用手册
- [ ] 更新贡献指南
- [ ] 生成 API 文档
- [ ] 添加使用示例
- [ ] 文档审查

## 验收标准
- ✅ 所有文档更新完成
- ✅ 文档清晰易懂
- ✅ 示例代码可运行
- ✅ 文档审查通过

## 时间估算
- 1-1.5 天

## 关联
- Epic: #$epicIssue
- Milestone: #$m3
"@

gh issue create `
    --title "[Phase 6] 文档更新" `
    --label "phase-6,priority:high,type:documentation,milestone-3" `
    --milestone "$m3" `
    --body $issue63Body
Write-Host "  ✅ Issue 6.3 创建成功" -ForegroundColor Green

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "所有子任务 Issues 创建完成！" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
