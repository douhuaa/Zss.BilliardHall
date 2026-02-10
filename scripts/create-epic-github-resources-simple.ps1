# GitHub Epic 创建脚本（简化版）
# Epic: RuleSet-as-Source-of-Truth
# 创建日期: 2026-02-10

Write-Host "================================================" -ForegroundColor Cyan
Write-Host " GitHub Epic 创建脚本" -ForegroundColor Cyan
Write-Host " Epic: RuleSet-as-Source-of-Truth" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$repo = "douhuaa/Zss.BilliardHall"

# ============================================================
# 步骤 1: 创建标签
# ============================================================

Write-Host "步骤 1: 创建标签" -ForegroundColor Yellow
Write-Host ""

$labels = @(
    @{ name = "epic:ruleset-sot"; color = "B60205"; description = "RuleSet-as-Source-of-Truth Epic" },
    @{ name = "phase-0"; color = "0E8A16"; description = "阶段 0: 治理与准备" },
    @{ name = "phase-1"; color = "0E8A16"; description = "阶段 1: 构建生成工具" },
    @{ name = "phase-2"; color = "0E8A16"; description = "阶段 2: Agent/Skills 集成" },
    @{ name = "phase-3"; color = "0E8A16"; description = "阶段 3: 批量生成测试" },
    @{ name = "phase-4"; color = "0E8A16"; description = "阶段 4: 重新生成 ADR" },
    @{ name = "phase-5"; color = "0E8A16"; description = "阶段 5: CI 集成" },
    @{ name = "phase-6"; color = "0E8A16"; description = "阶段 6: 验证与优化" },
    @{ name = "milestone-1"; color = "1D76DB"; description = "Milestone 1: Tools & API Integration" },
    @{ name = "milestone-2"; color = "1D76DB"; description = "Milestone 2: Test & Documentation Generation" },
    @{ name = "milestone-3"; color = "1D76DB"; description = "Milestone 3: CI Integration & Validation" },
    @{ name = "priority:high"; color = "D93F0B"; description = "高优先级" },
    @{ name = "priority:medium"; color = "FBCA04"; description = "中优先级" },
    @{ name = "type:epic"; color = "5319E7"; description = "Epic 类型" },
    @{ name = "type:feature"; color = "84B6EB"; description = "Feature 类型" },
    @{ name = "type:task"; color = "C5DEF5"; description = "Task 类型" },
    @{ name = "type:refactor"; color = "FBCA04"; description = "Refactor 类型" },
    @{ name = "type:test"; color = "BFD4F2"; description = "Test 类型" },
    @{ name = "type:documentation"; color = "0075CA"; description = "Documentation 类型" },
    @{ name = "type:optimization"; color = "FEF2C0"; description = "Optimization 类型" }
)

foreach ($label in $labels) {
    try {
        $body = $label | ConvertTo-Json -Compress
        gh api -X POST "/repos/$repo/labels" -f name=$($label.name) -f color=$($label.color) -f description=$($label.description) | Out-Null
        Write-Host "  ✅ 创建标签: $($label.name)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  标签已存在或创建失败: $($label.name)" -ForegroundColor Yellow
    }
}

Write-Host ""

# ============================================================
# 步骤 2: 创建 Milestones
# ============================================================

Write-Host "步骤 2: 创建 Milestones" -ForegroundColor Yellow
Write-Host ""

# Milestone 1
Write-Host "创建 Milestone 1..." -ForegroundColor Cyan
$m1Body = @"
{
  "title": "Tools & API Integration",
  "state": "open",
  "description": "实现 3 个核心代码生成器，并完成所有 Agent/Skills 的 RuleSet API 集成\n\n阶段 1: 构建生成工具（4-5天）\n阶段 2: Agent/Skills 集成 RuleSet API（3-4天）\n\n关键交付物:\n- 3 个代码生成器\n- 18 个更新的 Agent/Skills\n- 单元测试套件（覆盖率 > 80%）",
  "due_on": "2026-02-19T23:59:59Z"
}
"@

try {
    $milestone1 = $m1Body | gh api -X POST "/repos/$repo/milestones" --input - | ConvertFrom-Json
    Write-Host "  ✅ Milestone #$($milestone1.number): $($milestone1.title)" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠️  创建失败（可能已存在）" -ForegroundColor Yellow
    $milestone1 = $null
}

Write-Host ""

# Milestone 2
Write-Host "创建 Milestone 2..." -ForegroundColor Cyan
$m2Body = @"
{
  "title": "Test & Documentation Generation",
  "state": "open",
  "description": "为所有 RuleSet 生成业务规则测试，并重新生成 ADR Decision 章节\n\n阶段 3: 批量生成测试套件（3-4天）\n阶段 4: 重新生成 ADR Decision 章节（2-3天）\n\n关键交付物:\n- 200-300 个新测试\n- 43 个更新的 ADR 文档",
  "due_on": "2026-02-26T23:59:59Z"
}
"@

try {
    $milestone2 = $m2Body | gh api -X POST "/repos/$repo/milestones" --input - | ConvertFrom-Json
    Write-Host "  ✅ Milestone #$($milestone2.number): $($milestone2.title)" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠️  创建失败（可能已存在）" -ForegroundColor Yellow
    $milestone2 = $null
}

Write-Host ""

# Milestone 3
Write-Host "创建 Milestone 3..." -ForegroundColor Cyan
$m3Body = @"
{
  "title": "CI Integration & Validation",
  "state": "open",
  "description": "实现 CI 自动验证，端到端测试，性能优化，完成文档更新\n\n阶段 5: CI 自动验证一致性（2-3天）\n阶段 6: 验证与优化（3-4天）\n\n关键交付物:\n- 2 个一致性检查器\n- CI 流程更新\n- 性能基准\n- 完整文档",
  "due_on": "2026-03-05T23:59:59Z"
}
"@

try {
    $milestone3 = $m3Body | gh api -X POST "/repos/$repo/milestones" --input - | ConvertFrom-Json
    Write-Host "  ✅ Milestone #$($milestone3.number): $($milestone3.title)" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠️  创建失败（可能已存在）" -ForegroundColor Yellow
    $milestone3 = $null
}

Write-Host ""

# ============================================================
# 步骤 3: 创建 Epic Issue
# ============================================================

Write-Host "步骤 3: 创建 Epic Issue" -ForegroundColor Yellow
Write-Host ""

$epicBody = Get-Content "docs/governance/epic-issue-body.md" -Raw

try {
    $epicIssue = gh issue create `
        --title "Epic: RuleSet-as-Source-of-Truth" `
        --label "epic:ruleset-sot,priority:high,type:epic" `
        --body $epicBody `
        | Select-String -Pattern "#(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }
    
    Write-Host "  ✅ Epic Issue #$epicIssue 创建成功" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Epic Issue 创建失败: $_" -ForegroundColor Red
    $epicIssue = $null
}

Write-Host ""

# ============================================================
# 步骤 4: 创建子任务 Issues
# ============================================================

Write-Host "步骤 4: 创建子任务 Issues" -ForegroundColor Yellow
Write-Host ""

# 获取 Milestone 编号
$m1Number = if ($milestone1) { $milestone1.number } else { "" }

# Issue 0.1
Write-Host "创建 Issue 0.1..." -ForegroundColor Cyan
$issue01Body = @"
## 任务描述
创建 Epic Issue、Milestones、GitHub Project 看板

## 检查清单
- [x] 创建 Milestone 1: Tools & API Integration
- [x] 创建 Milestone 2: Test & Documentation Generation
- [x] 创建 Milestone 3: CI Integration & Validation
- [x] 创建 Epic Issue
- [x] 创建标签
- [ ] 创建 GitHub Project 看板
- [ ] 配置看板列和自动化

## 验收标准
- ✅ 3 个 Milestones 创建成功
- ✅ Epic Issue 创建成功
- ✅ 标签创建成功
- ⏳ GitHub Project 待创建

## 时间估算
- 1 小时

## 关联
- Epic: #$epicIssue
"@

try {
    $issue01 = gh issue create `
        --title "[Phase 0] 创建 GitHub Epic 基础设施" `
        --label "phase-0,priority:high,type:task,milestone-1" `
        --milestone "$m1Number" `
        --body $issue01Body `
        | Select-String -Pattern "#(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }
    
    Write-Host "  ✅ Issue #$issue01 创建成功" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Issue 0.1 创建失败: $_" -ForegroundColor Red
}

Write-Host ""

# ============================================================
# 总结
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "创建总结" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "✅ 已创建以下资源:" -ForegroundColor Green
Write-Host ""

if ($milestone1) {
    Write-Host "📊 Milestones:" -ForegroundColor Cyan
    Write-Host "  - Milestone #$($milestone1.number): $($milestone1.title)" -ForegroundColor White
}
if ($milestone2) {
    Write-Host "  - Milestone #$($milestone2.number): $($milestone2.title)" -ForegroundColor White
}
if ($milestone3) {
    Write-Host "  - Milestone #$($milestone3.number): $($milestone3.title)" -ForegroundColor White
}
Write-Host ""

if ($epicIssue) {
    Write-Host "🎯 Epic Issue:" -ForegroundColor Cyan
    Write-Host "  - Issue #${epicIssue}: Epic: RuleSet-as-Source-of-Truth" -ForegroundColor White
    Write-Host "  - URL: https://github.com/$repo/issues/$epicIssue" -ForegroundColor White
}
Write-Host ""

Write-Host "🔗 下一步:" -ForegroundColor Yellow
Write-Host "  1. 访问 GitHub 查看创建的资源" -ForegroundColor White
Write-Host "  2. 创建 GitHub Project 看板（需要在 Web UI 手动创建）" -ForegroundColor White
Write-Host "  3. 继续创建其他子任务 Issues（见 GITHUB-EPIC-SETUP-SCRIPT.md）" -ForegroundColor White
Write-Host "  4. 更新 PR #365，关联 Epic" -ForegroundColor White
Write-Host ""

Write-Host "脚本执行完成！" -ForegroundColor Green
