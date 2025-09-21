# create-epics.ps1
# PowerShell helper to create roadmap Epics as GitHub Issues using gh CLI
# Usage:
# 1) Ensure you are in the repo directory and logged in: `gh auth login`
# 2) Run: `pwsh .\scripts\create-epics.ps1`
# Optional args: -Draft (adds label 'draft')

param(
    [switch]$Draft
)

$repoArg = "" # leave empty to use current repo

function CreateEpic($title, $labels, $body) {
    Write-Host "Creating issue: $title"
    $args = @('issue','create','--title',$title)
    foreach ($l in $labels) { $args += @('--label',$l) }
    if ($Draft) { $args += @('--label','draft') }
    $args += @('--body',$body)
    if ($repoArg -and $repoArg.Trim() -ne '') { $args += $repoArg }
    gh @args
}

# Sprint 0
$s0Title = "Sprint 0：基线准备与架构冻结"
$s0Labels = @("epic","roadmap","sprint0")
$s0Body = @"
目标：清晰最小范围、搭建技术基座与数据/事件契约。

关键交付：
- 架构草图（C4 L2）评审通过
- schema.sql v1 合并并可重复迁移
- README 启动文档
- Backlog 完成初步估点
"@
CreateEpic $s0Title $s0Labels $s0Body

# V0.1
$v01Title = "V0.1：可用闭环（开台→计费→支付→关台）"
$v01Labels = @("epic","roadmap","v0.1")
$v01Body = @"
目标：在真/模拟设备下完成 20+ 会话闭环，保证 P0 埋点与数据可追溯。

退出标准：
- 20 次闭环成功
- P0 埋点缺失率 <5%
- 核心查询无全表扫描（Explain 验证）
- 并发场景单元/集成测试通过
"@
CreateEpic $v01Title $v01Labels $v01Body

# V0.2
$v02Title = "V0.2：体验增强（预约/套餐/告警→工单）"
$v02Labels = @("epic","roadmap","v0.2")
$v02Body = @"
目标：加入预约、会员/套餐与基础告警工单链路。

退出标准：
- 预约→开台抽测 10/10
- 套餐扣减对账 20 条无误
- 告警→工单链路 <60s
- 在线率报表生成
"@
CreateEpic $v02Title $v02Labels $v02Body

# V0.3
$v03Title = "V0.3：增长与多门店（分摊支付/多门店隔离/基础营销）"
$v03Labels = @("epic","roadmap","v0.3")
$v03Body = @"
目标：支持分摊支付、多门店隔离与基础营销策略。

退出标准：
- 分摊误差≈0
- 越权事件=0
- 首单/活动指标达成
"@
CreateEpic $v03Title $v03Labels $v03Body

Write-Host "Done. Check the created issues in your repository or gh output above."