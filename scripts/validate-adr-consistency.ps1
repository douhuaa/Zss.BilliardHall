# ADR 编号/目录/内容三元一致性验证工具
#
# 此脚本用于验证 ADR 文档的编号、目录和内容的一致性

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# 定义路径
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$AdrPath = Join-Path $RepoRoot "docs\adr"

# 统计变量
$script:TotalAdrs = 0
$script:ValidAdrs = 0
$script:InvalidAdrs = 0
$script:IsValid = $true

# ADR 层级编号范围定义
$TierRanges = @{
    "constitutional" = @{ Start = 1; End = 99; Special = @() }
    "structure" = @{ Start = 100; End = 199; Special = @() }
    "runtime" = @{ Start = 200; End = 299; Special = @() }
    "technical" = @{ Start = 300; End = 399; Special = @() }
    "governance" = @{ Start = 900; End = 999; Special = @(0) }
}

# 输出函数
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }

# 解析编号范围
function Test-InRange {
    param(
        [int]$Number,
        [hashtable]$Range
    )
    
    # 检查特殊编号
    if ($Range.Special -contains $Number) {
        return $true
    }
    
    # 检查范围
    return ($Number -ge $Range.Start -and $Number -le $Range.End)
}

# 查找 ADR 文件
function Get-AdrFiles {
    Get-ChildItem -Path $AdrPath -Recurse -Filter "ADR-*.md" | Sort-Object FullName
}

# 提取 ADR 编号
function Get-AdrNumber {
    param([string]$FilePath)
    
    $fileName = Split-Path -Leaf $FilePath
    if ($fileName -match '^ADR[-_]?(\d{4})') {
        return [int]$matches[1]
    }
    return $null
}

# 提取目录层级
function Get-Tier {
    param([string]$FilePath)
    
    $dir = Split-Path -Parent $FilePath
    return Split-Path -Leaf $dir
}

# 验证 ADR 元数据
function Test-Metadata {
    param([string]$FilePath)
    
    $content = Get-Content -Path $FilePath -Raw
    $errors = @()
    
    # 检查必需的元数据字段
    if ($content -notmatch '\*\*状态\*\*[：:]' -and $content -notmatch '\*\*Status\*\*:') {
        $errors += "缺少状态字段"
    }
    
    if ($content -notmatch '\*\*级别\*\*[：:]' -and $content -notmatch '\*\*Level\*\*:') {
        $errors += "缺少级别字段"
    }
    
    # 检查编号格式
    $number = Get-AdrNumber -FilePath $FilePath
    if ($number -eq $null) {
        $errors += "无法提取编号"
    }
    
    return $errors
}

# 检查跳号
function Test-NumberGaps {
    $files = Get-AdrFiles
    $numbers = @()
    
    foreach ($file in $files) {
        $number = Get-AdrNumber -FilePath $file.FullName
        if ($number -ne $null) {
            $numbers += $number
        }
    }
    
    $sorted = $numbers | Sort-Object
    $gaps = @()
    
    for ($i = 0; $i -lt $sorted.Count - 1; $i++) {
        $current = $sorted[$i]
        $next = $sorted[$i + 1]
        $diff = $next - $current
        
        # 跳过跨层级的检查
        if ($diff -gt 1 -and [Math]::Floor($current / 100) -eq [Math]::Floor($next / 100)) {
            $gaps += "$current 到 $next (跳过 $($diff - 1) 个编号)"
        }
    }
    
    if ($gaps.Count -eq 0) {
        Write-Success "编号连续性检查通过"
    } else {
        Write-Warning "发现编号跳号："
        foreach ($gap in $gaps) {
            Write-Host "    $gap"
        }
    }
}

# 主验证函数
function Test-AdrConsistency {
    Write-Info "开始 ADR 三元一致性验证..."
    Write-Host ""
    
    $adrFiles = Get-AdrFiles
    $script:TotalAdrs = $adrFiles.Count
    
    Write-Info "发现 $($script:TotalAdrs) 个 ADR 文档"
    Write-Host ""
    
    foreach ($adrFile in $adrFiles) {
        $adrNumber = Get-AdrNumber -FilePath $adrFile.FullName
        $tier = Get-Tier -FilePath $adrFile.FullName
        $filename = $adrFile.Name
        
        if ($adrNumber -eq $null) {
            continue
        }
        
        Write-Host ("━" * 60) -ForegroundColor Gray
        Write-Info "检查 ADR-$($adrNumber.ToString('0000')) ($filename)"
        
        $hasError = $false
        
        # 1. 检查编号格式
        $numberStr = $adrNumber.ToString('0000')
        if ($numberStr.Length -eq 4) {
            Write-Success "  编号格式正确：$numberStr"
        } else {
            Write-Error "  编号格式错误：应为4位数字"
            $hasError = $true
        }
        
        # 2. 检查目录与编号范围一致性
        if ($TierRanges.ContainsKey($tier)) {
            if (Test-InRange -Number $adrNumber -Range $TierRanges[$tier]) {
                $range = $TierRanges[$tier]
                Write-Success "  目录位置正确：$tier (范围: $($range.Start)-$($range.End))"
            } else {
                Write-Error "  目录位置错误：ADR-$numberStr 不在 $tier 的编号范围内"
                $hasError = $true
            }
        } else {
            Write-Warning "  未知目录层级：$tier"
        }
        
        # 3. 检查元数据
        $metadataErrors = Test-Metadata -FilePath $adrFile.FullName
        if ($metadataErrors.Count -eq 0) {
            Write-Success "  元数据完整"
        } else {
            Write-Error "  元数据问题：$($metadataErrors -join ', ')"
            $hasError = $true
        }
        
        # 4. 检查文件命名规范
        if ($filename -match '^ADR-\d{4}-.+\.md$') {
            Write-Success "  文件命名符合规范"
        } else {
            Write-Warning "  文件命名可能不符合规范"
        }
        
        if ($hasError) {
            $script:InvalidAdrs++
            $script:IsValid = $false
        } else {
            $script:ValidAdrs++
        }
        
        Write-Host ""
    }
    
    # 检查编号跳号
    Write-Host ("━" * 60) -ForegroundColor Gray
    Test-NumberGaps
    Write-Host ""
    
    # 输出总结
    Write-Host ("━" * 60) -ForegroundColor Gray
    Write-Host ""
    Write-Host "📊 验证总结" -ForegroundColor Cyan
    Write-Host ("━" * 60) -ForegroundColor Gray
    Write-Host ""
    Write-Host "ADR 文档统计："
    Write-Host "  总 ADR 数：$($script:TotalAdrs)"
    Write-Host "  有效 ADR：" -NoNewline
    Write-Host "$($script:ValidAdrs)" -ForegroundColor Green
    Write-Host "  无效 ADR：" -NoNewline
    if ($script:InvalidAdrs -gt 0) {
        Write-Host "$($script:InvalidAdrs)" -ForegroundColor Red
    } else {
        Write-Host "$($script:InvalidAdrs)" -ForegroundColor Green
    }
    Write-Host ""
    
    if ($script:IsValid) {
        Write-Success "验证通过：所有 ADR 文档编号、目录、内容一致！"
        return 0
    } else {
        Write-Error "验证失败：发现 ADR 一致性问题"
        Write-Host ""
        Write-Host "请执行以下操作：" -ForegroundColor Yellow
        Write-Host "  1. 修正编号格式错误（确保为4位数字）"
        Write-Host "  2. 将 ADR 移动到正确的目录层级"
        Write-Host "  3. 补充缺失的元数据字段"
        Write-Host "  4. 确保文件命名符合规范"
        Write-Host ""
        Write-Host "参考文档：" -ForegroundColor Cyan
        Write-Host "  - docs/adr/constitutional/ADR-006-terminology-numbering-constitution.md"
        Write-Host "  - docs/adr/governance/ADR-0900-adr-process.md"
        Write-Host ""
        return 1
    }
}

# 主执行
$exitCode = Test-AdrConsistency
exit $exitCode
