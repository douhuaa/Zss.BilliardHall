using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 更新文档命令处理器
/// 基于 RuleSetRegistry 更新文档索引和交叉引用
/// </summary>
public sealed class UpdateDocumentationCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly string _repositoryRoot;

    public UpdateDocumentationCommandHandler(
        IFileSystem fileSystem,
        string? repositoryRoot = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repositoryRoot = repositoryRoot ?? FindRepositoryRoot();
    }

    public async Task<int> ExecuteAsync(
        string? docType = null,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("📝 更新文档索引");

            // 从 RuleSetRegistry 获取所有规则集
            Console.WriteLine("📖 从 RuleSetRegistry 读取规则集...");
            var allRuleSets = RuleSetRegistry.GetAllRuleSets().ToList();
            
            Console.WriteLine($"✅ 成功加载 {allRuleSets.Count} 个规则集");
            Console.WriteLine($"   ADR 编号范围: {string.Join(", ", RuleSetRegistry.GetAllAdrNumbers())}");

            // 确定文档根目录
            var docsPath = Path.Combine(_repositoryRoot, "docs");
            
            if (!_fileSystem.DirectoryExists(docsPath))
            {
                Console.WriteLine($"❌ 未找到文档目录: {docsPath}");
                return 1;
            }

            Console.WriteLine($"📂 文档根目录: {docsPath}");

            // 根据文档类型执行不同的更新操作
            var operation = docType?.ToLowerInvariant() switch
            {
                "adr" or null => "更新 ADR 索引",
                "summary" => "更新摘要文档",
                _ => throw new ArgumentException($"未知的文档类型: {docType}")
            };

            Console.WriteLine($"📋 操作: {operation}");

            // 示例：更新 ADR 索引
            if (docType == null || docType.Equals("adr", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateAdrIndexAsync(allRuleSets, docsPath, dryRun, cancellationToken);
            }

            Console.WriteLine("✅ 文档更新完成");
            
            if (dryRun)
            {
                Console.WriteLine();
                Console.WriteLine("💡 这是 dry-run 模式，未实际修改文件");
                Console.WriteLine("   移除 --dry-run 参数以实际写入文件");
            }

            return 0;
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

    private async Task UpdateAdrIndexAsync(
        List<ArchitectureRuleSet> ruleSets,
        string docsPath,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("🔨 更新 ADR 索引...");

        // 这里应该：
        // 1. 读取现有的 ADR 索引文件（如 docs/adr/README.md）
        // 2. 根据 RuleSetRegistry 中的规则集生成新的索引条目
        // 3. 合并或替换现有索引
        // 4. 写回文件

        // 简化实现：仅生成一个规则集列表作为示例
        var indexContent = GenerateAdrIndexContent(ruleSets);

        Console.WriteLine($"📄 生成的索引包含 {ruleSets.Count} 个 ADR 条目");
        
        if (dryRun)
        {
            Console.WriteLine();
            Console.WriteLine("--- 索引内容预览 (前20行) ---");
            var lines = indexContent.Split('\n').Take(20);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("--- 预览结束 ---");
        }
        else
        {
            var indexPath = Path.Combine(docsPath, "adr", "ADR-INDEX-AUTO-GENERATED.md");
            await _fileSystem.WriteAllTextAsync(indexPath, indexContent, cancellationToken);
            Console.WriteLine($"✅ 已写入: {indexPath}");
        }
    }

    private static string GenerateAdrIndexContent(List<ArchitectureRuleSet> ruleSets)
    {
        var content = new System.Text.StringBuilder();
        
        content.AppendLine("# ADR 索引（自动生成）");
        content.AppendLine();
        content.AppendLine("> ⚠️ **本文件由 RuleSetRegistry 自动生成，请勿手动编辑**");
        content.AppendLine();
        content.AppendLine($"生成时间：{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine();
        content.AppendLine("## 规则集列表");
        content.AppendLine();
        content.AppendLine("| ADR 编号 | 规则数 | 条款数 |");
        content.AppendLine("|---------|--------|--------|");

        foreach (var ruleSet in ruleSets.OrderBy(rs => rs.AdrNumber))
        {
            content.AppendLine($"| ADR-{ruleSet.AdrNumber:D3} | {ruleSet.RuleCount} | {ruleSet.ClauseCount} |");
        }

        content.AppendLine();
        content.AppendLine("## 按层级分类");
        content.AppendLine();

        // 宪法层
        var constitutional = ruleSets.Where(rs => rs.AdrNumber >= 1 && rs.AdrNumber <= 8).ToList();
        if (constitutional.Any())
        {
            content.AppendLine("### 宪法层（Constitutional）");
            foreach (var rs in constitutional.OrderBy(rs => rs.AdrNumber))
            {
                content.AppendLine($"- ADR-{rs.AdrNumber:D3}：{rs.RuleCount} 条规则，{rs.ClauseCount} 个条款");
            }
            content.AppendLine();
        }

        // 治理层
        var governance = ruleSets.Where(rs => rs.AdrNumber >= 900 && rs.AdrNumber <= 999).ToList();
        if (governance.Any())
        {
            content.AppendLine("### 治理层（Governance）");
            foreach (var rs in governance.OrderBy(rs => rs.AdrNumber))
            {
                content.AppendLine($"- ADR-{rs.AdrNumber:D3}：{rs.RuleCount} 条规则，{rs.ClauseCount} 个条款");
            }
            content.AppendLine();
        }

        // 运行时层
        var runtime = ruleSets.Where(rs => rs.AdrNumber >= 201 && rs.AdrNumber <= 240).ToList();
        if (runtime.Any())
        {
            content.AppendLine("### 运行时层（Runtime）");
            foreach (var rs in runtime.OrderBy(rs => rs.AdrNumber))
            {
                content.AppendLine($"- ADR-{rs.AdrNumber:D3}：{rs.RuleCount} 条规则，{rs.ClauseCount} 个条款");
            }
            content.AppendLine();
        }

        // 结构层
        var structure = ruleSets.Where(rs => rs.AdrNumber >= 120 && rs.AdrNumber <= 124).ToList();
        if (structure.Any())
        {
            content.AppendLine("### 结构层（Structure）");
            foreach (var rs in structure.OrderBy(rs => rs.AdrNumber))
            {
                content.AppendLine($"- ADR-{rs.AdrNumber:D3}：{rs.RuleCount} 条规则，{rs.ClauseCount} 个条款");
            }
            content.AppendLine();
        }

        content.AppendLine("---");
        content.AppendLine();
        content.AppendLine("**数据来源**：`RuleSetRegistry`（唯一真相来源）");

        return content.ToString();
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
