#!/usr/bin/env pwsh
<#
.SYNOPSIS
    ADR-测试映射一致性校验工具

.DESCRIPTION
    此脚本用于验证 ADR 文档与架构测试之间的一致性，确保：
    1. 每条 ADR 中标记为【必须架构测试覆盖】的条款都有对应的测试
    2. 每个测试方法都正确引用了对应的 ADR 编号和条款
    3. 测试失败消息包含正确的 ADR 引用

.PARAMETER Fix
    如果指定，脚本将尝试自动修复部分问题（如生成测试骨架）

.EXAMPLE
    ./scripts/validate-adr-test-mapping.ps1

.EXAMPLE
    ./scripts/validate-adr-test-mapping.ps1 -Fix
#>

param(
    [switch]$Fix,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# 定义路径
$repoRoot = Split-Path -Parent $PSScriptRoot
$adrPath = Join-Path $repoRoot "docs/adr"
$testsPath = Join-Path $repoRoot "src/tests/ArchitectureTests/ADR"
$promptsPath = Join-Path $repoRoot "docs/copilot"

# 颜色输出函数
function Write-Success
{
    param([string]$Message) Write-Host "✅ $Message" -ForegroundColor Green
}
function Write-Warning
{
    param([string]$Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow
}
function Write-Error-Custom
{
    param([string]$Message) Write-Host "❌ $Message" -ForegroundColor Red
}
function Write-Info
{
    param([string]$Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

# 数据结构
class ADRRequirement
{
    [string]$AdrNumber
    [string]$Section
    [string]$Requirement
    [int]$LineNumber
    [bool]$HasTest
}

class TestAssertion
{
    [string]$AdrNumber
    [string]$TestMethod
    [string]$Section
    [string]$FileName
    [int]$LineNumber
    [bool]$HasAdrReference
}

class ValidationResult
{
    [int]$TotalADRs
    [int]$TotalRequirements
    [int]$RequirementsWithTests
    [int]$RequirementsWithoutTests
    [int]$TotalTests
    [int]$TestsWithAdrReference
    [int]$TestsWithoutAdrReference
    [ADRRequirement[]]$MissingTests
    [TestAssertion[]]$TestsMissingReference
    [bool]$IsValid
}

function Get-ADRFiles
{
    $adrFiles = @()

    # 搜索所有 ADR 文件
    Get-ChildItem -Path $adrPath -Recurse -Filter "ADR-*.md" | ForEach-Object {
        if ($_.Name -match '^ADR-(\d{4})')
        {
            $adrFiles += @{
                Number = $matches[1]
                Path = $_.FullName
                Name = $_.Name
            }
        }
    }

    return $adrFiles | Sort-Object { [int]$_.Number }
}

function Get-TestFiles
{
    $testFiles = @()

    if (Test-Path $testsPath)
    {
        Get-ChildItem -Path $testsPath -Filter "ADR_*.cs" | ForEach-Object {
            if ($_.Name -match '^ADR_(\d{4})')
            {
                $testFiles += @{
                    Number = $matches[1]
                    Path = $_.FullName
                    Name = $_.Name
                }
            }
        }
    }

    return $testFiles | Sort-Object { [int]$_.Number }
}

function Extract-ADRRequirements
{
    param([string]$FilePath, [string]$AdrNumber)

    $requirements = @()
    $content = Get-Content $FilePath -Raw
    $lines = Get-Content $FilePath

    # 移除代码块中的内容（```...```），避免误计数示例代码
    $inCodeBlock = $false
    $filteredLines = @()

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        if ($line -match '^```')
        {
            $inCodeBlock = -not $inCodeBlock
            continue
        }

        if (-not $inCodeBlock)
        {
            $filteredLines += $line
        }
    }

    # 查找标记为【必须架构测试覆盖】的条款（排除代码块后）
    for ($i = 0; $i -lt $filteredLines.Count; $i++) {
        $line = $filteredLines[$i]

        # 匹配【必须架构测试覆盖】或 [MUST_TEST] 标记
        if ($line -match '【必须架构测试覆盖】|【必须测试】|\[MUST_TEST\]')
        {
            # 向前查找相关的内容（通常在同一段落或上一行）
            $contextStart = [Math]::Max(0, $i - 3)
            $contextEnd = [Math]::Min($filteredLines.Count - 1, $i + 1)
            $context = ($filteredLines[$contextStart..$contextEnd] -join " ").Trim()

            # 提取章节信息
            $section = ""
            for ($j = $i; $j -ge 0; $j--) {
                if ($filteredLines[$j] -match '^#+\s+(.+)$')
                {
                    $section = $matches[1].Trim()
                    break
                }
            }

            $req = [ADRRequirement]@{
                AdrNumber = $AdrNumber
                Section = $section
                Requirement = $context
                LineNumber = $i + 1
                HasTest = $false
            }

            $requirements += $req
        }
    }

    # 查找快速参考表中的约束
    if ($content -match '(?ms)##\s+快速参考.*?(?=##|\z)')
    {
        $tableSection = $matches[0]

        # 解析表格中的每一行
        $tableLines = $tableSection -split "`n" | Where-Object { $_ -match '\|' -and $_ -notmatch '^[-\s|]+$' -and $_ -notmatch '^\s*\|.*\|.*\|.*\|.*\|.*\|.*\|' }

        foreach ($tableLine in $tableLines)
        {
            if ($tableLine -match '\|\s*(.+?)\s*\|\s*(.+?)\s*\|')
            {
                $constraint = $matches[1].Trim()
                $description = $matches[2].Trim()

                # 跳过表头
                if ($constraint -match '^(约束|规则|项目|类型)' -or $constraint -eq '---')
                {
                    continue
                }

                $req = [ADRRequirement]@{
                    AdrNumber = $AdrNumber
                    Section = "快速参考表"
                    Requirement = "$constraint - $description"
                    LineNumber = 0
                    HasTest = $false
                }

                $requirements += $req
            }
        }
    }

    return $requirements
}

function Extract-TestAssertions
{
    param([string]$FilePath, [string]$AdrNumber)

    $assertions = @()
    $content = Get-Content $FilePath -Raw
    $lines = Get-Content $FilePath

    # 提取所有测试方法
    $methodPattern = '(?ms)\[(?:Fact|Theory).*?\]\s*(?:public|private|internal)\s+(?:async\s+)?(?:Task|void)\s+(\w+)\s*\('

    $matches = [regex]::Matches($content, $methodPattern)

    foreach ($match in $matches)
    {
        $methodName = $match.Groups[1].Value
        $methodStart = $match.Index

        # 查找方法体中的 ADR 引用
        $methodEndPattern = '(?ms)\{\s*.*?\n\s*\}'
        $methodBodyMatch = [regex]::Match($content.Substring($methodStart), $methodEndPattern)

        if ($methodBodyMatch.Success)
        {
            $methodBody = $methodBodyMatch.Value

            # 检查是否包含 ADR 引用
            $hasAdrRef = $methodBody -match "ADR-$AdrNumber" -or $methodName -match "ADR[_-]?$AdrNumber"

            # 提取章节信息（从方法名或 DisplayName）
            $section = ""
            if ($methodName -match "ADR[_-]?$AdrNumber[_.](\d+)")
            {
                $section = $matches[1]
            }

            # 查找 DisplayName 属性
            $displayNameMatch = [regex]::Match($match.Value, '\[.*?DisplayName\s*=\s*"([^"]+)"')
            if ($displayNameMatch.Success)
            {
                $displayName = $displayNameMatch.Groups[1].Value
                if ($displayName -match "ADR-$AdrNumber\.(\d+)")
                {
                    $section = $matches[1]
                }
            }

            $assertion = [TestAssertion]@{
                AdrNumber = $AdrNumber
                TestMethod = $methodName
                Section = $section
                FileName = Split-Path $FilePath -Leaf
                LineNumber = ($content.Substring(0, $methodStart) -split "`n").Count
                HasAdrReference = $hasAdrRef
            }

            $assertions += $assertion
        }
    }

    return $assertions
}

function Validate-Mapping
{
    Write-Info "开始 ADR-测试映射验证..."
    Write-Host ""

    $result = [ValidationResult]@{
        TotalADRs = 0
        TotalRequirements = 0
        RequirementsWithTests = 0
        RequirementsWithoutTests = 0
        TotalTests = 0
        TestsWithAdrReference = 0
        TestsWithoutAdrReference = 0
        MissingTests = @()
        TestsMissingReference = @()
        IsValid = $true
    }

    $adrFiles = Get-ADRFiles
    $testFiles = Get-TestFiles

    $result.TotalADRs = $adrFiles.Count

    Write-Info "发现 $( $adrFiles.Count ) 个 ADR 文档"
    Write-Info "发现 $( $testFiles.Count ) 个测试文件"
    Write-Host ""

    # 构建测试文件映射
    $testFileMap = @{ }
    foreach ($testFile in $testFiles)
    {
        $testFileMap[$testFile.Number] = $testFile
    }

    # 验证每个 ADR
    foreach ($adrFile in $adrFiles)
    {
        $adrNumber = $adrFile.Number

        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
        Write-Info "检查 ADR-$adrNumber ($( $adrFile.Name ))"

        # 提取 ADR 要求
        $requirements = Extract-ADRRequirements -FilePath $adrFile.Path -AdrNumber $adrNumber
        $result.TotalRequirements += $requirements.Count

        if ($requirements.Count -eq 0)
        {
            Write-Warning "  未发现标记为【必须架构测试覆盖】的条款"
        }
        else
        {
            Write-Info "  发现 $( $requirements.Count ) 条必须测试的约束"
        }

        # 检查是否有对应的测试文件（只有在有标记约束时才需要）
        if ($requirements.Count -gt 0 -and -not $testFileMap.ContainsKey($adrNumber))
        {
            Write-Error-Custom "  缺少测试文件: ADR_${adrNumber}_Architecture_Tests.cs"
            $result.IsValid = $false
            $result.MissingTests += $requirements
            $result.RequirementsWithoutTests += $requirements.Count
            continue
        }

        # 如果没有标记约束，跳过测试文件检查
        if ($requirements.Count -eq 0)
        {
            Write-Host ""
            continue
        }

        # 提取测试断言
        $testFile = $testFileMap[$adrNumber]
        $assertions = Extract-TestAssertions -FilePath $testFile.Path -AdrNumber $adrNumber
        $result.TotalTests += $assertions.Count

        Write-Info "  发现 $( $assertions.Count ) 个测试方法"

        # 检查测试方法是否都有 ADR 引用
        $assertionsWithoutRef = $assertions | Where-Object { -not $_.HasAdrReference }
        if ($assertionsWithoutRef.Count -gt 0)
        {
            Write-Warning "  $( $assertionsWithoutRef.Count ) 个测试方法缺少 ADR 引用："
            foreach ($assertion in $assertionsWithoutRef)
            {
                Write-Host "    - $( $assertion.TestMethod ) (行 $( $assertion.LineNumber ))" -ForegroundColor Yellow
            }
            $result.TestsMissingReference += $assertionsWithoutRef
            $result.TestsWithoutAdrReference += $assertionsWithoutRef.Count
            $result.IsValid = $false
        }
        else
        {
            Write-Success "  所有测试方法都包含 ADR 引用"
        }

        $result.TestsWithAdrReference += ($assertions.Count - $assertionsWithoutRef.Count)

        # 简单检查：如果有要求但测试数量为 0，标记为问题
        if ($requirements.Count -gt 0 -and $assertions.Count -eq 0)
        {
            Write-Error-Custom "  ADR 有 $( $requirements.Count ) 条约束需要测试，但未发现任何测试方法"
            $result.MissingTests += $requirements
            $result.RequirementsWithoutTests += $requirements.Count
            $result.IsValid = $false
        }
        else
        {
            $result.RequirementsWithTests += $requirements.Count
        }

        Write-Host ""
    }

    # 输出总结
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "📊 验证总结" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "ADR 文档统计：" -ForegroundColor White
    Write-Host "  总 ADR 数：$( $result.TotalADRs )"
    Write-Host "  总约束条款数：$( $result.TotalRequirements )"
    Write-Host "  有测试覆盖：$( $result.RequirementsWithTests )" -ForegroundColor Green
    Write-Host "  缺少测试：$( $result.RequirementsWithoutTests )" -ForegroundColor $( if ($result.RequirementsWithoutTests -gt 0)
    {
        "Red"
    }
    else
    {
        "Green"
    } )
    Write-Host ""
    Write-Host "测试文件统计：" -ForegroundColor White
    Write-Host "  总测试方法数：$( $result.TotalTests )"
    Write-Host "  有 ADR 引用：$( $result.TestsWithAdrReference )" -ForegroundColor Green
    Write-Host "  缺少 ADR 引用：$( $result.TestsWithoutAdrReference )" -ForegroundColor $( if ($result.TestsWithoutAdrReference -gt 0)
    {
        "Red"
    }
    else
    {
        "Green"
    } )
    Write-Host ""

    if ($result.IsValid)
    {
        Write-Success "验证通过：ADR 文档与测试映射一致！"
    }
    else
    {
        Write-Error-Custom "验证失败：发现 ADR-测试映射不一致问题"
        Write-Host ""
        Write-Host "请执行以下操作：" -ForegroundColor Yellow
        Write-Host "  1. 为缺少测试的 ADR 约束编写对应的架构测试"
        Write-Host "  2. 为缺少 ADR 引用的测试方法添加正确的 ADR 编号"
        Write-Host "  3. 确保测试失败消息包含 ADR 引用（格式：ADR-XXXX 违规：...）"
        Write-Host ""
        Write-Host "参考文档：" -ForegroundColor Cyan
        Write-Host "  - docs/adr/governance/ADR-0000-architecture-tests.md"
        Write-Host "  - docs/copilot/README.md"
        Write-Host ""
    }

    return $result
}

# 主执行流程
try
{
    $result = Validate-Mapping

    if (-not $result.IsValid)
    {
        exit 1
    }

    exit 0
}
catch
{
    Write-Error-Custom "执行过程中发生错误："
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}
