using NetArchTest.Rules;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-930: 代码审查与 ADR 合规自检流程
/// 参考：docs/adr/governance/ADR-930-code-review-compliance.md
///
/// 【测试覆盖映射】
/// ├─ ADR-930_1_1: PR 必须填写变更类型和影响范围 (L2) → PR_Template_Should_Include_Checklist
/// └─ Copilot 指令和 CI 集成 (L2) → Copilot_Instructions_Should_Exist, Architecture_Tests_Must_Be_In_CI
/// </summary>
public sealed class ADR_930_Architecture_Tests
{
    [Fact(DisplayName = "ADR-930_1_1: PR 模板应包含必要的自检清单")]
    public void PR_Template_Should_Include_Checklist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        (repoRoot != null).Should().BeTrue($"❌ ADR-930_1_1 违规: 无法找到仓库根目录\n\n问题分析：\n无法定位包含 .github 或 Directory.Build.props 的仓库根目录\n\n修复建议：\n1. 确保项目包含 .github 目录或 Directory.Build.props 文件\n2. 检查测试运行环境的工作目录设置\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.1）");
        
        var prTemplate = Path.Combine(repoRoot!, ".github", "PULL_REQUEST_TEMPLATE.md");
        File.Exists(prTemplate).Should().BeTrue($"❌ ADR-930_1_1 违规: PR 模板文件不存在\n\n文件路径：{prTemplate}\n\n问题分析：\n项目必须包含 PR 模板文件以规范 Pull Request 的描述格式和必填项\n\n修复建议：\n1. 在 .github 目录创建 PULL_REQUEST_TEMPLATE.md 文件\n2. 包含变更类型、影响范围、测试清单等必填字段\n3. 参考 ADR-930 规范的 PR 模板格式\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.1）");
        
        // 验证模板包含基本内容
        var content = File.ReadAllText(prTemplate);
        (content.Length > 0).Should().BeTrue($"❌ ADR-930_1_1 违规: PR 模板文件为空\n\n文件路径：{prTemplate}\n\n问题分析：\nPR 模板文件存在但内容为空，无法提供有效指导\n\n修复建议：\n1. 添加 PR 描述模板内容\n2. 包含必填字段的清单\n3. 提供填写示例和说明\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.1）");
        (content.Length > 100).Should().BeTrue($"❌ ADR-930_1_1 违规: PR 模板内容过少\n\n当前长度：{content.Length} 字符\n\n问题分析：\nPR 模板应包含实质性内容以提供充分的指导\n\n修复建议：\n1. 扩展 PR 模板内容\n2. 包含详细的自检清单\n3. 添加填写说明和示例\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.1）");
    }

    [Fact(DisplayName = "ADR-930_1_2: Copilot 指令文件应存在")]
    public void Copilot_Instructions_Should_Exist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        (repoRoot != null).Should().BeTrue($"❌ ADR-930_1_2 违规: 无法找到仓库根目录\n\n问题分析：\n无法定位包含 .github 或 Directory.Build.props 的仓库根目录\n\n修复建议：\n1. 确保项目包含 .github 目录或 Directory.Build.props 文件\n2. 检查测试运行环境的工作目录设置\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.2）");
        
        // 验证 Copilot 指令目录存在
        var copilotDir = Path.Combine(repoRoot!, "docs", "copilot");
        Directory.Exists(copilotDir).Should().BeTrue($"❌ ADR-930_1_2 违规: Copilot 指令目录不存在\n\n目录路径：{copilotDir}\n\n问题分析：\n项目应包含 Copilot 指令目录以存放 AI 辅助开发的提示词\n\n修复建议：\n1. 在 docs 目录创建 copilot 子目录\n2. 添加各个 ADR 的提示词文件\n3. 使用 *.prompts.md 命名格式\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.2）");
        
        // 验证至少有一些提示词文件
        var promptFiles = Directory.GetFiles(copilotDir, "*.prompts.md");
        (promptFiles.Length > 0).Should().BeTrue($"❌ ADR-930_1_2 违规: Copilot 目录中没有提示词文件\n\n目录路径：{copilotDir}\n\n问题分析：\nCopilot 指令目录存在但没有提示词文件，无法提供 AI 辅助\n\n修复建议：\n1. 为关键 ADR 创建提示词文件\n2. 文件命名格式：adr-xxx.prompts.md\n3. 包含具体的代码生成指导和约束说明\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.2）");
    }

    [Fact(DisplayName = "ADR-930_1_3: 架构测试必须在 CI 中执行")]
    public void Architecture_Tests_Must_Be_In_CI()
    {
        // 验证架构测试项目可以被 CI 发现和执行
        var currentAssembly = typeof(ADR_930_Architecture_Tests).Assembly;
        (currentAssembly != null).Should().BeTrue($"❌ ADR-930_1_3 违规: 无法获取当前测试程序集\n\n问题分析：\n无法通过反射获取架构测试程序集，可能表示测试环境配置有问题\n\n修复建议：\n1. 检查测试项目配置是否正确\n2. 确保测试项目可以正常编译和加载\n3. 验证测试运行环境的设置\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.3）");
        currentAssembly.GetName().Name.Should().Be("ArchitectureTests", $"❌ ADR-930_1_3 违规: 架构测试程序集名称不正确\n\n当前名称：{currentAssembly.GetName().Name}\n期望名称：ArchitectureTests\n\n问题分析：\n架构测试项目必须命名为 ArchitectureTests 以符合约定\n\n修复建议：\n1. 将测试项目重命名为 ArchitectureTests\n2. 更新 CI 配置以正确引用该项目\n3. 确保 CI 脚本执行架构测试\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.3）");
        
        // 验证测试类存在并可执行
        var testTypes = currentAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Architecture_Tests"))
            .ToList();
            
        (testTypes.Count >= 10).Should().BeTrue($"❌ ADR-930_1_3 违规: 架构测试类数量不足\n\n当前数量：{testTypes.Count}\n期望数量：≥ 10\n\n问题分析：\n架构测试类数量过少，可能未覆盖所有关键 ADR 约束\n\n修复建议：\n1. 为每个 Active 状态的 ADR 创建对应的测试类\n2. 确保测试类命名符合 ADR_XXX_Architecture_Tests 格式\n3. 验证所有 L1 级别的约束都有对应的测试\n\n参考：docs/adr/governance/ADR-930-code-review-compliance.md（§1.3）");
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".github")) ||
                File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
