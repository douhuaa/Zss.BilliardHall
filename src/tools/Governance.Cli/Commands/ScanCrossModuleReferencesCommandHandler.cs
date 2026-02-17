using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 扫描跨模块引用命令处理器
/// 仅提取引用事实，不做严重性判定（由Agent根据ADR判定）
/// </summary>
public sealed class ScanCrossModuleReferencesCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IRuleSetQueryService _ruleSetQueryService;

    public ScanCrossModuleReferencesCommandHandler(
        IFileSystem fileSystem,
        IRuleSetQueryService? ruleSetQueryService = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _ruleSetQueryService = ruleSetQueryService ?? new RuleSetQueryService();
    }

    /// <summary>
    /// 执行扫描跨模块引用命令
    /// </summary>
    /// <param name="sourceModule">源模块名称（如 "Orders"）</param>
    /// <param name="includeTests">是否包含测试代码</param>
    /// <returns>退出代码：0表示成功，1表示失败</returns>
    public async Task<int> ExecuteAsync(string sourceModule, bool includeTests = false)
    {
        try
        {
            Console.WriteLine($"🔍 扫描跨模块引用: {sourceModule}");

            // 验证源模块存在
            var modulePath = $"src/Modules/{sourceModule}";
            if (!_fileSystem.DirectoryExists(modulePath))
            {
                Console.WriteLine($"❌ 模块不存在: {modulePath}");
                return 1;
            }

            // 扫描引用
            var references = await ScanReferencesAsync(modulePath, includeTests);

            // 输出结果
            if (references.Count == 0)
            {
                Console.WriteLine("✅ 未发现跨模块引用");
                return 0;
            }

            Console.WriteLine($"📊 发现 {references.Count} 个跨模块引用:");
            Console.WriteLine();

            // 按目标模块分组
            var groupedReferences = references
                .GroupBy(r => r.TargetModule)
                .OrderBy(g => g.Key);

            foreach (var group in groupedReferences)
            {
                Console.WriteLine($"📦 目标模块: {group.Key}");
                foreach (var reference in group.OrderBy(r => r.FilePath))
                {
                    Console.WriteLine($"   📄 {reference.FilePath}");
                    Console.WriteLine($"      命名空间: {reference.TargetNamespace}");
                    Console.WriteLine($"      层级: {reference.TargetLayer}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("💡 说明:");
            Console.WriteLine("  - 此工具仅提取引用事实，不做严重性判定");
            Console.WriteLine("  - 请根据 ADR-001（模块边界规则）判定这些引用是否合规");
            Console.WriteLine("  - 合规的跨模块通信方式：");
            Console.WriteLine("    1. 使用 Contracts 命名空间");
            Console.WriteLine("    2. 使用领域事件（异步）");
            Console.WriteLine("    3. 使用原始类型（ID）传递");

            return references.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 执行失败: {ex.Message}");
            return 1;
        }
    }

    private async Task<List<CrossModuleReference>> ScanReferencesAsync(string modulePath, bool includeTests)
    {
        var references = new List<CrossModuleReference>();

        // 获取所有.cs文件
        var files = GetCSharpFiles(modulePath, includeTests);

        foreach (var file in files)
        {
            var content = await _fileSystem.ReadAllTextAsync(file);
            
            // 提取using语句
            var usingStatements = ExtractUsingStatements(content);

            // 检查是否引用了其他模块
            foreach (var usingStatement in usingStatements)
            {
                var crossModuleRef = AnalyzeUsingStatement(usingStatement, file);
                if (crossModuleRef != null)
                {
                    references.Add(crossModuleRef);
                }
            }
        }

        return references;
    }

    private IEnumerable<string> GetCSharpFiles(string modulePath, bool includeTests)
    {
        var allFiles = _fileSystem.GetFiles(modulePath, "*.cs", SearchOption.AllDirectories).AsEnumerable();

        if (!includeTests)
        {
            // 过滤掉测试文件
            allFiles = allFiles.Where(f => !f.Contains("/Tests/") && !f.Contains("\\Tests\\"));
        }

        return allFiles;
    }

    private IEnumerable<string> ExtractUsingStatements(string content)
    {
        // 简单的using语句提取（可以改进为使用Roslyn）
        const string usingKeyword = "using ";
        var lines = content.Split('\n');
        var usingStatements = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith(usingKeyword) && trimmed.EndsWith(";"))
            {
                var usingNamespace = trimmed
                    .Substring(usingKeyword.Length)
                    .TrimEnd(';')
                    .Trim();
                usingStatements.Add(usingNamespace);
            }
        }

        return usingStatements;
    }

    private CrossModuleReference? AnalyzeUsingStatement(string usingNamespace, string filePath)
    {
        // 检查是否是模块引用
        const string modulePrefix = "Zss.BilliardHall.Modules.";
        if (!usingNamespace.StartsWith(modulePrefix))
        {
            return null;
        }

        // 提取目标模块名
        var parts = usingNamespace.Substring(modulePrefix.Length).Split('.');
        if (parts.Length == 0)
        {
            return null;
        }

        var targetModule = parts[0];
        
        // 提取当前模块名
        var currentModule = ExtractModuleFromPath(filePath);
        if (currentModule == targetModule)
        {
            // 同一模块内的引用，不算跨模块
            return null;
        }

        // 判定层级
        var targetLayer = parts.Length > 1 ? parts[1] : "Unknown";

        return new CrossModuleReference(
            FilePath: filePath,
            SourceModule: currentModule,
            TargetModule: targetModule,
            TargetNamespace: usingNamespace,
            TargetLayer: targetLayer);
    }

    private string ExtractModuleFromPath(string filePath)
    {
        // 从路径中提取模块名: src/Modules/{ModuleName}/...
        var parts = filePath.Split('/', '\\');
        var modulesIndex = Array.FindIndex(parts, p => p.Equals("Modules", StringComparison.OrdinalIgnoreCase));
        
        if (modulesIndex >= 0 && modulesIndex + 1 < parts.Length)
        {
            return parts[modulesIndex + 1];
        }

        return "Unknown";
    }
}

/// <summary>
/// 跨模块引用记录
/// </summary>
public sealed record CrossModuleReference(
    string FilePath,
    string SourceModule,
    string TargetModule,
    string TargetNamespace,
    string TargetLayer);
