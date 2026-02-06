namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-360: CI/CD Pipeline 流程标准化
/// 参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md
/// </summary>
public sealed class ADR_360_Architecture_Tests
{
    [Fact(DisplayName = "ADR-360_1_1: GitHub Workflows 配置文件应存在")]
    public void GitHub_Workflows_Configuration_Should_Exist()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        var workflowsDir = Path.Combine(repoRoot, ".github", "workflows");
        Directory.Exists(workflowsDir).Should().BeTrue($"❌ ADR-360_1_1 违规: GitHub Workflows 目录不存在\n\n目录路径：{workflowsDir}\n\n问题分析：\n项目必须包含 .github/workflows 目录来定义 CI/CD 流程\n\n修复建议：\n1. 在项目根目录创建 .github/workflows 目录\n2. 添加至少一个 workflow 配置文件（*.yml 或 *.yaml）\n3. 配置自动化测试、构建和部署流程\n\n参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md（§1.1）");

        // 验证至少有一个 workflow 文件
        var workflowFiles = Directory.GetFiles(workflowsDir, "*.yml")
            .Concat(Directory.GetFiles(workflowsDir, "*.yaml"))
            .ToList();

        (workflowFiles.Count > 0).Should().BeTrue($"❌ ADR-360_1_1 违规: Workflows 目录中没有配置文件\n\n目录路径：{workflowsDir}\n\n问题分析：\n.github/workflows 目录必须包含至少一个 workflow 配置文件\n\n修复建议：\n1. 创建 CI workflow 文件（如 ci.yml）\n2. 配置自动测试和构建流程\n3. 示例：.github/workflows/ci.yml\n\n参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md（§1.1）");
    }

    [Fact(DisplayName = "ADR-360_1_2: PR 模板应存在")]
    public void Pull_Request_Template_Should_Exist()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        var prTemplate = Path.Combine(repoRoot, ".github", "PULL_REQUEST_TEMPLATE.md");
        File.Exists(prTemplate).Should().BeTrue($"❌ ADR-360_1_2 违规: PR 模板文件不存在\n\n文件路径：{prTemplate}\n\n问题分析：\n项目必须包含 PR 模板文件以规范 Pull Request 的描述\n\n修复建议：\n1. 在 .github 目录创建 PULL_REQUEST_TEMPLATE.md 文件\n2. 定义 PR 必填字段（目的、变更、测试等）\n3. 参考现有项目的 PR 模板格式\n\n参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md（§1.2）");
    }

    [Fact(DisplayName = "ADR-360_1_3: 架构测试项目应存在并可被 CI 执行")]
    public void Architecture_Tests_Should_Be_Executable()
    {
        // 验证架构测试项目可以被发现和执行
        var currentAssembly = typeof(ADR_360_Architecture_Tests).Assembly;
        currentAssembly.Should().NotBeNull($"❌ ADR-360_1_3 违规: 无法获取当前测试程序集\n\n问题分析：\n无法通过反射获取架构测试程序集，可能表示测试环境配置有问题\n\n修复建议：\n1. 检查测试项目配置是否正确\n2. 确保测试项目可以正常编译和加载\n3. 验证测试运行环境的设置\n\n参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md（§1.3）");
        currentAssembly.GetName().Name.Should().Be("ArchitectureTests", $"❌ ADR-360_1_3 违规: 架构测试程序集名称不正确\n\n当前名称：{currentAssembly.GetName().Name}\n期望名称：ArchitectureTests\n\n问题分析：\n架构测试项目必须命名为 ArchitectureTests 以符合约定\n\n修复建议：\n1. 将测试项目重命名为 ArchitectureTests\n2. 更新项目引用和配置\n3. 确保 CI 脚本使用正确的项目名称\n\n参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md（§1.3）");
    }
}
