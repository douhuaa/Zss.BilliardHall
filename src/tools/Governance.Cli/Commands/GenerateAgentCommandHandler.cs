using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 生成 Agent Instructions YAML
/// </summary>
public sealed class GenerateAgentCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IAgentInstructionGenerator _instructionGenerator;

    public GenerateAgentCommandHandler(
        IFileSystem fileSystem,
        IAgentInstructionGenerator instructionGenerator)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _instructionGenerator = instructionGenerator ?? throw new ArgumentNullException(nameof(instructionGenerator));
    }

    public async Task<int> ExecuteAsync(string outputDirectory, int? adrNumber = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("🔧 生成 Agent Instructions");

            // 确保输出目录存在
            if (!_fileSystem.DirectoryExists(outputDirectory))
            {
                Console.WriteLine($"📁 创建输出目录: {outputDirectory}");
                _fileSystem.CreateDirectory(outputDirectory);
            }

            // 获取要处理的 RuleSet
            var ruleSets = adrNumber.HasValue
                ? new List<ArchitectureRuleSet> { RuleSetRegistry.GetStrict(adrNumber.Value) }
                : RuleSetRegistry.GetAllRuleSets().ToList();

            Console.WriteLine($"📊 将生成 {ruleSets.Count} 个 Agent Instructions");

            var successCount = 0;
            foreach (var ruleSet in ruleSets)
            {
                try
                {
                    Console.WriteLine($"\n🔨 处理 ADR-{ruleSet.AdrNumber:D3}");

                    // 生成 Instructions
                    var options = new InstructionGenerationOptions
                    {
                        AgentPrefix = "AG",
                        StartInstructionNumber = 1,
                        IncludeApiExamples = true,
                        IncludeConstraintChecks = true,
                        IncludeTestCommands = true,
                        IncludeGuidelines = true
                    };

                    var yaml = _instructionGenerator.GenerateInstructions(ruleSet, options);

                    // 写入文件
                    var fileName = $"ADR-{ruleSet.AdrNumber:D3}-agent-instructions.yaml";
                    var filePath = Path.Combine(outputDirectory, fileName);

                    await _fileSystem.WriteAllTextAsync(filePath, yaml, cancellationToken);
                    Console.WriteLine($"✅ 已生成: {fileName}");

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  跳过 ADR-{ruleSet.AdrNumber:D3}: {ex.Message}");
                }
            }

            Console.WriteLine($"\n✅ 完成！成功生成 {successCount}/{ruleSets.Count} 个文件");
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
}
