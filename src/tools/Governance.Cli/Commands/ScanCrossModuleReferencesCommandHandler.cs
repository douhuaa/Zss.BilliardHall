using System.Text.Json;
using System.Text.RegularExpressions;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 扫描跨模块引用命令处理器
/// 扫描项目中的跨模块引用，基于 RuleSetRegistry 获取边界规则
/// </summary>
public sealed class ScanCrossModuleReferencesCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IRuleSetQueryService _ruleSetQueryService;
    private readonly string _repositoryRoot;

    private const string ModuleNamespacePrefix = "Zss.BilliardHall.Modules.";
    
    // 模块名提取正则：Zss.BilliardHall.Modules.{ModuleName}.{Layer}.*
    private static readonly Regex ModulePattern = new(
        @"Zss\.BilliardHall\.Modules\.(?<module>[^.]+)\.(?<layer>[^.]+)",
        RegexOptions.Compiled);

    public ScanCrossModuleReferencesCommandHandler(
        IFileSystem fileSystem,
        IRuleSetQueryService? ruleSetQueryService = null,
        string? repositoryRoot = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _ruleSetQueryService = ruleSetQueryService ?? new RuleSetQueryService();
        _repositoryRoot = repositoryRoot ?? FindRepositoryRoot();
    }

    public async Task<int> ExecuteAsync(
        string? sourceModule = null,
        bool includeTests = false,
        bool outputJson = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("🔍 扫描跨模块引用");

            // 从 RuleSetRegistry 获取 ADR-001（模块边界规则）
            Console.WriteLine("📖 从 RuleSetRegistry 读取模块边界规则...");
            var adr001 = _ruleSetQueryService.GetRuleSet(1);
            
            if (adr001 == null)
            {
                Console.WriteLine("⚠️  未找到 ADR-001 规则集，将仅报告引用事实，不进行严重性推导");
            }
            else
            {
                var summary = _ruleSetQueryService.GetRuleSetSummary(adr001);
                Console.WriteLine($"✅ 成功加载 {summary.FormattedAdrId}");
                Console.WriteLine($"   规则数: {summary.RuleCount}");
                Console.WriteLine($"   条款数: {summary.ClauseCount}");
            }

            // 确定要扫描的模块目录
            var modulesPath = Path.Combine(_repositoryRoot, "src", "Modules");
            
            if (!_fileSystem.DirectoryExists(modulesPath))
            {
                Console.WriteLine($"❌ 未找到模块目录: {modulesPath}");
                return 1;
            }

            // 获取所有模块
            var modules = _fileSystem.GetDirectories(modulesPath)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            Console.WriteLine($"📂 找到 {modules.Count} 个模块: {string.Join(", ", modules!)}");

            // 确定要扫描的源模块
            var modulesToScan = string.IsNullOrWhiteSpace(sourceModule)
                ? modules!
                : new List<string> { sourceModule };

            if (!modulesToScan.Any())
            {
                Console.WriteLine("❌ 没有要扫描的模块");
                return 1;
            }

            // 扫描每个模块
            var allViolations = new List<CrossModuleReference>();
            
            foreach (var module in modulesToScan)
            {
                var modulePath = Path.Combine(modulesPath, module);
                
                if (!_fileSystem.DirectoryExists(modulePath))
                {
                    Console.WriteLine($"⚠️  模块目录不存在: {module}");
                    continue;
                }

                Console.WriteLine($"\n🔎 扫描模块: {module}");
                
                var violations = await ScanModuleAsync(module, modulePath, modules!, includeTests, cancellationToken);
                allViolations.AddRange(violations);
                
                if (violations.Count > 0)
                {
                    Console.WriteLine($"   找到 {violations.Count} 个跨模块引用");
                }
            }

            // 输出结果
            Console.WriteLine();
            
            if (outputJson)
            {
                OutputJson(allViolations, sourceModule);
            }
            else
            {
                OutputSummary(allViolations);
            }

            return allViolations.Count > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 执行失败: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   内部异常: {ex.InnerException.Message}");
            }
            return 1;
        }
    }

    private async Task<List<CrossModuleReference>> ScanModuleAsync(
        string sourceModule,
        string modulePath,
        List<string> allModules,
        bool includeTests,
        CancellationToken cancellationToken)
    {
        var violations = new List<CrossModuleReference>();
        
        // 获取所有 C# 文件
        var searchPattern = "*.cs";
        var csFiles = _fileSystem.GetFiles(modulePath, searchPattern, SearchOption.AllDirectories);

        // 过滤测试文件
        if (!includeTests)
        {
            csFiles = csFiles.Where(f => !f.Contains("/tests/", StringComparison.OrdinalIgnoreCase) &&
                                         !f.Contains("\\tests\\", StringComparison.OrdinalIgnoreCase))
                            .ToArray();
        }

        Console.WriteLine($"   文件数: {csFiles.Length}");

        foreach (var file in csFiles)
        {
            var content = await _fileSystem.ReadAllTextAsync(file, cancellationToken);
            var references = ExtractCrossModuleReferences(sourceModule, file, content, allModules);
            violations.AddRange(references);
        }

        return violations;
    }

    private List<CrossModuleReference> ExtractCrossModuleReferences(
        string sourceModule,
        string filePath,
        string content,
        List<string> allModules)
    {
        var references = new List<CrossModuleReference>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // 检查 using 语句
            if (!line.StartsWith("using ") || !line.Contains(ModuleNamespacePrefix))
            {
                continue;
            }

            // 提取命名空间
            var usingMatch = Regex.Match(line, @"using\s+([\w\.]+)\s*;");
            if (!usingMatch.Success)
            {
                continue;
            }

            var ns = usingMatch.Groups[1].Value;
            
            // 检查是否是跨模块引用
            var moduleMatch = ModulePattern.Match(ns);
            if (!moduleMatch.Success)
            {
                continue;
            }

            var targetModule = moduleMatch.Groups["module"].Value;
            var targetLayer = moduleMatch.Groups["layer"].Value;

            // 如果目标模块与源模块不同，就是跨模块引用
            if (targetModule != sourceModule && allModules.Contains(targetModule))
            {
                var reference = new CrossModuleReference
                {
                    File = filePath,
                    Line = i + 1,
                    Type = "DirectReference",
                    SourceModule = sourceModule,
                    TargetModule = targetModule,
                    TargetNamespace = ns,
                    TargetLayer = targetLayer,
                    DerivedSeverity = DetermineSeverity(targetLayer),
                    SeveritySource = "ADR-001_2"
                };

                references.Add(reference);
            }
        }

        return references;
    }

    private static string DetermineSeverity(string targetLayer)
    {
        // 基于 ADR-001 的严重性推导（仅供参考）
        return targetLayer.ToLowerInvariant() switch
        {
            "domain" => "High",      // 直接引用 Domain 层
            "usecases" => "Medium",  // 引用 UseCases 层
            "infrastructure" => "Medium", // 引用 Infrastructure 层
            "contracts" => "Low",    // 引用 Contracts 层（可能允许）
            _ => "Medium"
        };
    }

    private static void OutputSummary(List<CrossModuleReference> violations)
    {
        if (violations.Count == 0)
        {
            Console.WriteLine("✅ 未发现跨模块引用");
            return;
        }

        Console.WriteLine($"📊 跨模块引用汇总");
        Console.WriteLine($"总计: {violations.Count}");
        Console.WriteLine();

        // 按源模块分组
        var groupedBySource = violations.GroupBy(v => v.SourceModule);
        
        foreach (var group in groupedBySource)
        {
            Console.WriteLine($"📦 模块: {group.Key}");
            
            foreach (var violation in group)
            {
                Console.WriteLine($"   → {violation.TargetModule}.{violation.TargetLayer}");
                Console.WriteLine($"     文件: {Path.GetFileName(violation.File)}:{violation.Line}");
                Console.WriteLine($"     命名空间: {violation.TargetNamespace}");
                Console.WriteLine($"     推导严重性: {violation.DerivedSeverity} (依据: {violation.SeveritySource})");
                Console.WriteLine();
            }
        }

        Console.WriteLine("💡 提示:");
        Console.WriteLine("   - 推导严重性基于当前 ADR-001，仅供参考");
        Console.WriteLine("   - 最终判定应由 module-boundary-checker Agent 根据最新 ADR 执行");
        Console.WriteLine("   - 使用 --output-json 获取详细的 JSON 输出");
    }

    private static void OutputJson(List<CrossModuleReference> violations, string? sourceModule)
    {
        var result = new
        {
            sourceModule,
            violations = violations.Select(v => new
            {
                file = v.File,
                line = v.Line,
                type = v.Type,
                sourceModule = v.SourceModule,
                targetModule = v.TargetModule,
                targetNamespace = v.TargetNamespace,
                targetLayer = v.TargetLayer,
                derivedSeverity = v.DerivedSeverity,
                severitySource = v.SeveritySource
            }),
            summary = new
            {
                totalViolations = violations.Count,
                byLayer = violations.GroupBy(v => v.TargetLayer)
                    .ToDictionary(g => g.Key, g => g.Count())
            },
            metadata = new
            {
                scanTimestamp = DateTimeOffset.UtcNow.ToString("o"),
                filesScanned = violations.Select(v => v.File).Distinct().Count()
            }
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine(json);
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"无法找到仓库根目录。当前目录: {currentDirectory}");
    }
}

/// <summary>
/// 跨模块引用信息
/// </summary>
public sealed class CrossModuleReference
{
    public required string File { get; init; }
    public required int Line { get; init; }
    public required string Type { get; init; }
    public required string SourceModule { get; init; }
    public required string TargetModule { get; init; }
    public required string TargetNamespace { get; init; }
    public required string TargetLayer { get; init; }
    public required string DerivedSeverity { get; init; }
    public required string SeveritySource { get; init; }
}
