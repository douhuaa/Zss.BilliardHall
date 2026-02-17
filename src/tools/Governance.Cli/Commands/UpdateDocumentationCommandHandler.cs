using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 更新文档索引命令处理器
/// 自动更新ADR索引，按ADR编号排序（不使用硬编码分类）
/// </summary>
public sealed class UpdateDocumentationCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IRuleSetQueryService _ruleSetQueryService;

    public UpdateDocumentationCommandHandler(
        IFileSystem fileSystem,
        IRuleSetQueryService? ruleSetQueryService = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _ruleSetQueryService = ruleSetQueryService ?? new RuleSetQueryService();
    }

    /// <summary>
    /// 执行更新文档索引命令
    /// </summary>
    /// <param name="targetPath">目标路径（如 "docs/adr"）</param>
    /// <returns>退出代码：0表示成功，1表示失败</returns>
    public async Task<int> ExecuteAsync(string targetPath = "docs/adr")
    {
        try
        {
            Console.WriteLine($"📚 更新文档索引: {targetPath}");

            if (!_fileSystem.DirectoryExists(targetPath))
            {
                Console.WriteLine($"❌ 目录不存在: {targetPath}");
                return 1;
            }

            // 生成ADR索引
            var indexContent = GenerateAdrIndexContent();

            // 更新README.md
            var readmePath = Path.Combine(targetPath, "README.md");
            if (_fileSystem.FileExists(readmePath))
            {
                var existingContent = await _fileSystem.ReadAllTextAsync(readmePath);
                var updatedContent = MergeIndexContent(existingContent, indexContent);
                
                await _fileSystem.WriteAllTextAsync(readmePath, updatedContent);
                Console.WriteLine($"✅ 已更新: {readmePath}");
            }
            else
            {
                // 创建新的README
                var newContent = CreateNewReadme(indexContent);
                await _fileSystem.WriteAllTextAsync(readmePath, newContent);
                Console.WriteLine($"✅ 已创建: {readmePath}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 执行失败: {ex.Message}");
            return 1;
        }
    }

    private string GenerateAdrIndexContent()
    {
        var allRuleSets = _ruleSetQueryService.GetAllRuleSets()
            .OrderBy(rs => rs.AdrNumber)
            .ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## ADR 索引");
        sb.AppendLine();
        sb.AppendLine("| ADR编号 | 规则数 | 条款数 | 严重程度 | 作用域 |");
        sb.AppendLine("|---------|--------|--------|----------|--------|");

        foreach (var ruleSet in allRuleSets)
        {
            var summary = _ruleSetQueryService.CreateSummary(ruleSet);
            var severities = string.Join(", ", summary.Severities.Select(s => s.ToString()));
            var scopes = string.Join(", ", summary.Scopes.Select(s => s.ToString()));

            sb.AppendLine($"| {summary.FormattedId} | {summary.RuleCount} | {summary.ClauseCount} | {severities} | {scopes} |");
        }

        sb.AppendLine();
        sb.AppendLine($"**总计**: {allRuleSets.Count} 个 ADR");
        sb.AppendLine();

        return sb.ToString();
    }

    private string MergeIndexContent(string existingContent, string newIndexContent)
    {
        // 查找并替换"## ADR 索引"章节
        var indexSectionStart = existingContent.IndexOf("## ADR 索引", StringComparison.Ordinal);
        
        if (indexSectionStart >= 0)
        {
            // 找到下一个二级标题或文件结尾
            var nextSectionStart = existingContent.IndexOf("\n## ", indexSectionStart + 1, StringComparison.Ordinal);
            
            if (nextSectionStart >= 0)
            {
                // 替换索引章节，保留下一个章节的标题
                return existingContent.Substring(0, indexSectionStart) +
                       newIndexContent +
                       existingContent.Substring(nextSectionStart);
            }
            else
            {
                // 索引章节在文件末尾
                return existingContent.Substring(0, indexSectionStart) + newIndexContent;
            }
        }
        else
        {
            // 没有找到索引章节，追加到文件末尾
            return existingContent + "\n\n" + newIndexContent;
        }
    }

    private string CreateNewReadme(string indexContent)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Architecture Decision Records (ADR)");
        sb.AppendLine();
        sb.AppendLine("本目录包含项目的所有架构决策记录。");
        sb.AppendLine();
        sb.AppendLine(indexContent);
        
        return sb.ToString();
    }
}
