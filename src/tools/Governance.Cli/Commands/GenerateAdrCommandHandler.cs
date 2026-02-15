using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 生成 ADR Decision 章节并合并到现有 ADR 文档
/// </summary>
public sealed class GenerateAdrCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IAdrDecisionGenerator _decisionGenerator;
    private readonly IAdrDocumentMerger _documentMerger;
    private readonly IPathValidator _pathValidator;

    public GenerateAdrCommandHandler(
        IFileSystem fileSystem,
        IAdrDecisionGenerator decisionGenerator,
        IAdrDocumentMerger documentMerger,
        IPathValidator? pathValidator = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _decisionGenerator = decisionGenerator ?? throw new ArgumentNullException(nameof(decisionGenerator));
        _documentMerger = documentMerger ?? throw new ArgumentNullException(nameof(documentMerger));
        _pathValidator = pathValidator ?? new AdrFilePathValidator();
    }

    public async Task<int> ExecuteAsync(string adrNumberOrId, string adrFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"🔧 生成 ADR Decision 章节: {adrNumberOrId}");

            // 解析 ADR 编号
            var adrNumber = ParseAdrNumber(adrNumberOrId);
            if (adrNumber == null)
            {
                Console.WriteLine($"❌ 无效的 ADR 编号格式: {adrNumberOrId}");
                return 1;
            }

            // 路径安全检查：防止路径遍历攻击
            if (!_pathValidator.IsPathSafe(adrFilePath, out var errorMessage))
            {
                Console.WriteLine($"❌ 路径安全检查失败: {errorMessage}");
                return 1;
            }

            // 从 RuleSetRegistry 获取 RuleSet
            Console.WriteLine($"📖 从 RuleSetRegistry 读取 ADR-{adrNumber:D3} 规则集...");
            var ruleSet = RuleSetRegistry.Get(adrNumber.Value);
            if (ruleSet == null)
            {
                Console.WriteLine($"❌ 未找到 ADR-{adrNumber:D3} 的规则集");
                return 1;
            }

            Console.WriteLine($"✅ 成功加载规则集: ADR-{ruleSet.AdrNumber:D3}");
            Console.WriteLine($"   规则数: {ruleSet.RuleCount}");

            // 生成 Decision 章节
            Console.WriteLine("🔨 生成 Decision 章节...");
            var options = new DecisionGenerationOptions
            {
                IncludeSectionHeader = true,
                IncludeWarningNote = true,
                EscapeMarkdown = true,
                HeaderLevelOffset = 0
            };
            var decisionContent = _decisionGenerator.GenerateDecisionSection(ruleSet, options);

            // 检查 ADR 文件是否存在
            if (!_fileSystem.FileExists(adrFilePath))
            {
                Console.WriteLine($"❌ ADR 文件不存在: {adrFilePath}");
                return 1;
            }

            // 读取现有 ADR 文档
            Console.WriteLine($"📂 读取现有 ADR 文档: {adrFilePath}");
            var existingContent = await _fileSystem.ReadAllTextAsync(adrFilePath, cancellationToken);

            // 合并 Decision 章节
            Console.WriteLine("🔀 合并 Decision 章节到 ADR 文档...");
            var mergedContent = _documentMerger.MergeDecisionSection(existingContent, decisionContent);

            // 写回文件
            await _fileSystem.WriteAllTextAsync(adrFilePath, mergedContent, cancellationToken);
            Console.WriteLine($"✅ 成功更新 ADR 文档: {adrFilePath}");

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

    private static int? ParseAdrNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // 尝试直接解析数字
        if (int.TryParse(input, out var number))
            return number;

        // 尝试解析 "ADR-XXX" 格式
        if (input.StartsWith("ADR-", StringComparison.OrdinalIgnoreCase))
        {
            var numberPart = input[4..];
            if (int.TryParse(numberPart, out var parsedNumber))
                return parsedNumber;
        }

        return null;
    }
}
