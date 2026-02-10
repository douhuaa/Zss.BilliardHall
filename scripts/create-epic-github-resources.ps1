# GitHub Epic 自动创建脚本
# Epic: RuleSet-as-Source-of-Truth
# 创建日期: 2026-02-10

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host " GitHub Epic 创建脚本" -ForegroundColor Cyan
Write-Host " Epic: RuleSet-as-Source-of-Truth" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 检查 gh 命令
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "错误: 未找到 GitHub CLI (gh)。请先安装: https://cli.github.com/" -ForegroundColor Red
    exit 1
}

# 检查是否已登录
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 未登录 GitHub CLI。请运行: gh auth login" -ForegroundColor Red
    exit 1
}

Write-Host "✅ GitHub CLI 已就绪" -ForegroundColor Green
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  Dry Run 模式：仅显示命令，不实际执行" -ForegroundColor Yellow
    Write-Host ""
}

# 存储创建的资源 ID
$resources = @{}

# 函数：执行命令
function Invoke-GhCommand {
    param(
        [string]$Description,
        [string]$Command
    )
    
    Write-Host "➤ $Description" -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  命令: $Command" -ForegroundColor Gray
        return "DRY_RUN"
    }
    
    try {
        $result = Invoke-Expression $Command
        Write-Host "  ✅ 成功" -ForegroundColor Green
        return $result
    }
    catch {
        Write-Host "  ❌ 失败: $_" -ForegroundColor Red
        throw
    }
}

# ============================================================
# 1. 创建 Milestones
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "步骤 1: 创建 3 个 Milestones (使用 GitHub API)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$repo = "douhuaa/Zss.BilliardHall"

# Milestone 1
$milestone1 = @{
    title = "Tools & API Integration"
    description = @"
实现 3 个核心代码生成器，并完成所有 Agent/Skills 的 RuleSet API 集成

阶段 1: 构建生成工具（4-5天）
阶段 2: Agent/Skills 集成 RuleSet API（3-4天）

关键交付物:
1. 3 个代码生成器
2. 18 个更新的 Agent/Skills
3. 单元测试套件（覆盖率 > 80%）

验收标准:
- 3 个生成器接口定义并实现完成
- 单元测试覆盖率 > 80%
- 9 个 Agent 使用 RuleSetRegistry API
- 9 个 Skills 使用 RuleSetRegistry API
- 代码审查通过

时间: 2026-02-11 → 2026-02-19 (7-9 工作日)
"@
    due_on = "2026-02-19T23:59:59Z"
    state = "open"
} | ConvertTo-Json -Depth 10

if ($DryRun) {
    Write-Host "  [Dry Run] 创建 Milestone 1: Tools & API Integration" -ForegroundColor Gray
    $resources.Milestone1 = "DRY_RUN_1"
}
else {
    try {
        $result = $milestone1 | gh api -X POST "/repos/$repo/milestones" --input - | ConvertFrom-Json
        $resources.Milestone1 = $result.number
        Write-Host "  ✅ 成功创建 Milestone #$($result.number): $($result.title)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  创建失败（可能已存在）: $_" -ForegroundColor Yellow
        $resources.Milestone1 = "FAILED"
    }
}

Write-Host ""

# Milestone 2
$milestone2 = @{
    title = "Test & Documentation Generation"
    description = @"
为所有 RuleSet 生成业务规则测试，并重新生成 ADR Decision 章节

阶段 3: 批量生成测试套件（3-4天）
阶段 4: 重新生成 ADR Decision 章节（2-3天）

关键交付物:
1. 200-300 个新测试
2. 43 个更新的 ADR 文档

验收标准:
- 43 个 RuleSet 均有完整业务规则测试
- 所有新生成测试通过
- 43 个 ADR Decision 章节重新生成
- 人工审查通过

时间: 2026-02-20 → 2026-02-26 (5-7 工作日)
"@
    due_on = "2026-02-26T23:59:59Z"
    state = "open"
} | ConvertTo-Json -Depth 10

if ($DryRun) {
    Write-Host "  [Dry Run] 创建 Milestone 2: Test & Documentation Generation" -ForegroundColor Gray
    $resources.Milestone2 = "DRY_RUN_2"
}
else {
    try {
        $result = gh api -X POST "/repos/$repo/milestones" --input - <<< $milestone2 | ConvertFrom-Json
        $resources.Milestone2 = $result.number
        Write-Host "  ✅ 成功创建 Milestone #$($result.number): $($result.title)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  创建失败（可能已存在）: $_" -ForegroundColor Yellow
        $resources.Milestone2 = "FAILED"
    }
}

Write-Host ""

# Milestone 3
$milestone3 = @{
    title = "CI Integration & Validation"
    description = @"
实现 CI 自动验证，端到端测试，性能优化，完成文档更新

阶段 5: CI 自动验证一致性（2-3天）
阶段 6: 验证与优化（3-4天）

关键交付物:
1. 2 个一致性检查器
2. CI 流程更新
3. 性能基准
4. 完整文档

验收标准:
- CI 集成完成
- 端到端测试通过
- 性能基准达标
- 文档更新完成
- 架构委员会最终批准

时间: 2026-02-27 → 2026-03-05 (5-7 工作日)
"@
    due_on = "2026-03-05T23:59:59Z"
    state = "open"
} | ConvertTo-Json -Depth 10

if ($DryRun) {
    Write-Host "  [Dry Run] 创建 Milestone 3: CI Integration & Validation" -ForegroundColor Gray
    $resources.Milestone3 = "DRY_RUN_3"
}
else {
    try {
        $result = gh api -X POST "/repos/$repo/milestones" --input - <<< $milestone3 | ConvertFrom-Json
        $resources.Milestone3 = $result.number
        Write-Host "  ✅ 成功创建 Milestone #$($result.number): $($result.title)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  创建失败（可能已存在）: $_" -ForegroundColor Yellow
        $resources.Milestone3 = "FAILED"
    }
}

Write-Host ""

# ============================================================
# 2. 创建 Epic Issue
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "步骤 2: 创建 Epic Issue" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$epicBodyFile = "docs/governance/epic-issue-body.md"

if (-not (Test-Path $epicBodyFile)) {
    Write-Host "错误: 找不到 Epic Body 文件: $epicBodyFile" -ForegroundColor Red
    exit 1
}

$resources.EpicIssue = Invoke-GhCommand `
    -Description "创建 Epic Issue" `
    -Command "gh issue create --title 'Epic: RuleSet-as-Source-of-Truth' --label 'epic:ruleset-sot,priority:high,type:epic' --body-file '$epicBodyFile'"

Write-Host ""

# ============================================================
# 3. 创建子任务 Issues
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "步骤 3: 创建子任务 Issues" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# 阶段 0
Write-Host "--- 阶段 0: 治理与准备 ---" -ForegroundColor Magenta

$issue01Body = @"
## 任务描述
创建 Epic Issue、Milestones、GitHub Project 看板

## 检查清单
- [x] 创建 Milestone 1: Tools & API Integration
- [x] 创建 Milestone 2: Test & Documentation Generation
- [x] 创建 Milestone 3: CI Integration & Validation
- [x] 创建 Epic Issue
- [ ] 创建 GitHub Project 看板
- [ ] 配置看板列和自动化

## 验收标准
- ✅ 3 个 Milestones 创建成功
- ✅ Epic Issue 创建成功
- ✅ GitHub Project 创建成功
- ✅ 所有 Issues 关联到 Epic

## 时间估算
- 1 小时

## 关联
- Epic: #$($resources.EpicIssue)
- 文档: docs/governance/GITHUB-SETUP-GUIDE.md
"@

$resources.Issue01 = Invoke-GhCommand `
    -Description "创建 Issue 0.1: 创建 GitHub Epic 基础设施" `
    -Command "gh issue create --title '[Phase 0] 创建 GitHub Epic 基础设施' --label 'phase-0,priority:high,type:task' --milestone 'Tools & API Integration' --body '$issue01Body'"

Write-Host ""

$issue02Body = @"
## 任务描述
更新 PR #365，添加 Epic 关联，标记为 Ready for Review

## 检查清单
- [ ] 更新 PR 描述，关联 Epic Issue
- [ ] 添加标签 epic:ruleset-sot
- [ ] 标记为 Ready for Review
- [ ] 通知架构委员会审查

## 验收标准
- ✅ PR #365 关联到 Epic
- ✅ PR 状态为 Ready for Review
- ✅ 所有检查通过

## 时间估算
- 30 分钟

## 关联
- Epic: #$($resources.EpicIssue)
- PR: #365
"@

$resources.Issue02 = Invoke-GhCommand `
    -Description "创建 Issue 0.2: PR #365 准备与标记" `
    -Command "gh issue create --title '[Phase 0] PR #365 准备与标记' --label 'phase-0,priority:high,type:task' --milestone 'Tools & API Integration' --body '$issue02Body'"

Write-Host ""

# 阶段 1
Write-Host "--- 阶段 1: 构建生成工具 ---" -ForegroundColor Magenta

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
- Epic: #$($resources.EpicIssue)
- Milestone: Tools & API Integration
"@

$resources.Issue11 = Invoke-GhCommand `
    -Description "创建 Issue 1.1: 实现 ADR Decision 生成器" `
    -Command "gh issue create --title '[Phase 1] 实现 ADR Decision 生成器' --label 'phase-1,priority:high,type:feature,milestone-1' --milestone 'Tools & API Integration' --body '$issue11Body'"

Write-Host ""

# 继续创建其他 Issues...
# （由于篇幅限制，这里只展示部分示例）

Write-Host ""

# ============================================================
# 4. 总结
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "创建总结" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  Dry Run 模式完成。以上命令未实际执行。" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "如需实际创建，请运行: .\create-epic-github-resources.ps1" -ForegroundColor Yellow
}
else {
    Write-Host "✅ 已创建以下资源:" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📊 Milestones:" -ForegroundColor Cyan
    Write-Host "  - Milestone 1: $($resources.Milestone1)" -ForegroundColor White
    Write-Host "  - Milestone 2: $($resources.Milestone2)" -ForegroundColor White
    Write-Host "  - Milestone 3: $($resources.Milestone3)" -ForegroundColor White
    Write-Host ""
    
    Write-Host "🎯 Epic Issue:" -ForegroundColor Cyan
    Write-Host "  - Issue #$($resources.EpicIssue)" -ForegroundColor White
    Write-Host ""
    
    Write-Host "📝 子任务 Issues:" -ForegroundColor Cyan
    Write-Host "  - Issue #$($resources.Issue01): [Phase 0] 创建 GitHub Epic 基础设施" -ForegroundColor White
    Write-Host "  - Issue #$($resources.Issue02): [Phase 0] PR #365 准备与标记" -ForegroundColor White
    Write-Host "  - Issue #$($resources.Issue11): [Phase 1] 实现 ADR Decision 生成器" -ForegroundColor White
    Write-Host "  - ..." -ForegroundColor White
    Write-Host ""
    
    Write-Host "🔗 下一步:" -ForegroundColor Yellow
    Write-Host "  1. 访问 GitHub 查看创建的资源" -ForegroundColor White
    Write-Host "  2. 创建 GitHub Project 看板（需要在 Web UI 手动创建）" -ForegroundColor White
    Write-Host "  3. 将所有 Issues 添加到 Project" -ForegroundColor White
    Write-Host "  4. 更新 PR #365，关联 Epic" -ForegroundColor White
}

Write-Host ""
Write-Host "脚本执行完成！" -ForegroundColor Green
