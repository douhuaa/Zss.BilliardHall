namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_1: 日志分类与存储位置
/// 验证自动化工具日志的标准化存储规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_1_1: 日志必须按类型存储在标准位置
/// - ADR-970_1_2: 存储结构验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_1_Architecture_Tests
{
    /// <summary>
    /// ADR-970_1_1: 日志存储位置验证
    /// 验证 ADR-970 定义了日志标准存储位置（§ADR-970_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-970_1_1: 日志必须按类型存储在标准位置")]
    public void ADR_970_1_1_Logs_Must_Be_Stored_In_Standard_Locations()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-970_1_1",
            filePath: adr970Path,
            fileDescription: "ADR-970 文档",
            remediationSteps: new[]
            {
                "创建 ADR-970 文档",
                "定义日志标准存储位置规范"
            },
            adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
        
        File.Exists(adr970Path).Should().BeTrue(fileNotFoundMessage);

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        // 验证定义了存储结构
        var missingContent = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-970_1_1",
            filePath: adr970Path,
            missingContent: "docs/reports/",
            remediationSteps: new[]
            {
                "在 ADR-970 中定义标准日志存储位置",
                "指定 docs/reports/ 目录结构"
            },
            adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
        
        content.Should().Contain("docs/reports/", missingContent);
    }

    /// <summary>
    /// ADR-970_1_2: 存储结构验证
    /// 验证定义了完整的目录结构（§ADR-970_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-970_1_2: 必须定义完整的存储结构")]
    public void ADR_970_1_2_Storage_Structure_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        // 验证包含主要子目录
        var requiredDirs = new[] { "architecture-tests", "dependencies", "security", "builds", "tests" };
        foreach (var dir in requiredDirs)
        {
            var dirMissingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-970_1_2",
                filePath: adr970Path,
                missingContent: dir,
                remediationSteps: new[]
                {
                    $"在 ADR-970 中添加 '{dir}' 目录定义",
                    "确保存储结构完整"
                },
                adrReference: "docs/adr/governance/ADR-970-automation-log-integration-standard.md");
            
            content.Should().Contain(dir, dirMissingMessage);
        }
    }

}
