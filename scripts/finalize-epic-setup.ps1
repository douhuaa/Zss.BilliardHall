# 更新 Epic Issue 并创建 Project
# Epic: RuleSet-as-Source-of-Truth

Write-Host "更新 Epic Issue #367..." -ForegroundColor Cyan
Write-Host ""

$repo = "douhuaa/Zss.BilliardHall"
$epicIssue = 367

# 子任务 Issues
$childIssues = @(
    @{ id = 368; phase = 0; title = "创建 GitHub Epic 基础设施" },
    @{ id = 369; phase = 1; title = "实现 ADR Decision 生成器" },
    @{ id = 370; phase = 1; title = "实现测试生成器" },
    @{ id = 371; phase = 1; title = "实现 Agent 指令生成器" },
    @{ id = 372; phase = 2; title = "更新 9 个 Agent 使用 RuleSet API" },
    @{ id = 373; phase = 2; title = "更新 9 个 Skills 使用 RuleSet API" },
    @{ id = 374; phase = 3; title = "批量生成测试套件" },
    @{ id = 375; phase = 4; title = "重新生成 ADR Decision 章节" },
    @{ id = 376; phase = 5; title = "实现一致性检查器" },
    @{ id = 377; phase = 5; title = "CI 流程集成" },
    @{ id = 378; phase = 6; title = "端到端验证" },
    @{ id = 379; phase = 6; title = "性能优化" },
    @{ id = 380; phase = 6; title = "文档更新" }
)

# 添加评论到 Epic Issue
$comment = @"
## 📋 子任务清单

### 阶段 0: 治理与准备 ✅
- #368 [Phase 0] 创建 GitHub Epic 基础设施

### 阶段 1: 构建生成工具
- #369 [Phase 1] 实现 ADR Decision 生成器
- #370 [Phase 1] 实现测试生成器
- #371 [Phase 1] 实现 Agent 指令生成器

### 阶段 2: Agent/Skills 集成
- #372 [Phase 2] 更新 9 个 Agent 使用 RuleSet API
- #373 [Phase 2] 更新 9 个 Skills 使用 RuleSet API

### 阶段 3: 批量生成测试
- #374 [Phase 3] 批量生成测试套件

### 阶段 4: 重新生成 ADR
- #375 [Phase 4] 重新生成 ADR Decision 章节

### 阶段 5: CI 集成
- #376 [Phase 5] 实现一致性检查器
- #377 [Phase 5] CI 流程集成

### 阶段 6: 验证与优化
- #378 [Phase 6] 端到端验证
- #379 [Phase 6] 性能优化
- #380 [Phase 6] 文档更新

---

**总计**: 13 个子任务  
**状态**: 📋 待开始
"@

gh issue comment $epicIssue --body $comment
Write-Host "  ✅ Epic Issue 更新成功" -ForegroundColor Green

Write-Host ""

# 为每个子任务添加 Epic 关联
Write-Host "为所有子任务添加 Epic 关联..." -ForegroundColor Cyan
foreach ($issue in $childIssues) {
    $relationComment = "关联 Epic: #$epicIssue"
    try {
        gh issue comment $($issue.id) --body $relationComment
        Write-Host "  ✅ Issue #$($issue.id) 已关联到 Epic" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  Issue #$($issue.id) 关联失败" -ForegroundColor Yellow
    }
}

Write-Host ""

# 创建 GitHub Project (需要手动操作)
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "GitHub Project 创建指南" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "请按以下步骤在 GitHub Web UI 中创建 Project：" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. 访问: https://github.com/douhuaa/Zss.BilliardHall/projects" -ForegroundColor White
Write-Host "2. 点击 'New project' 按钮" -ForegroundColor White
Write-Host "3. 选择 'Board' 模板" -ForegroundColor White
Write-Host "4. 配置项目：" -ForegroundColor White
Write-Host "   - 名称: RuleSet-as-Source-of-Truth" -ForegroundColor Gray
Write-Host "   - 描述: 将 RuleSet 确立为架构治理的唯一真相源" -ForegroundColor Gray
Write-Host "5. 创建以下列：" -ForegroundColor White
Write-Host "   - 📋 To Do (待开始)" -ForegroundColor Gray
Write-Host "   - 🔨 In Progress (进行中)" -ForegroundColor Gray
Write-Host "   - 👀 In Review (代码审查中)" -ForegroundColor Gray
Write-Host "   - ✅ Done (已完成)" -ForegroundColor Gray
Write-Host "6. 添加所有 Issues (#367-#380) 到 Project" -ForegroundColor White
Write-Host "7. 配置自动化规则（可选）" -ForegroundColor White
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "完成！" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 创建总结:" -ForegroundColor Cyan
Write-Host "  - Epic Issue: #367" -ForegroundColor White
Write-Host "  - 子任务 Issues: #368-#380 (13个)" -ForegroundColor White
Write-Host "  - Milestones: 3个" -ForegroundColor White
Write-Host "  - 标签: 20个" -ForegroundColor White
Write-Host ""
Write-Host "🔗 相关链接:" -ForegroundColor Cyan
Write-Host "  - Epic Issue: https://github.com/douhuaa/Zss.BilliardHall/issues/367" -ForegroundColor White
Write-Host "  - Milestones: https://github.com/douhuaa/Zss.BilliardHall/milestones" -ForegroundColor White
Write-Host "  - Issues: https://github.com/douhuaa/Zss.BilliardHall/issues?q=label:epic:ruleset-sot" -ForegroundColor White
Write-Host ""
