namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_970;

/// <summary>
/// ADR-970_2: 结构化日志格式
/// 验证日志必须使用 JSON 格式的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-970_2_1: 必须使用 JSON 格式
/// - ADR-970_2_2: 标准 JSON 架构验证
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-970-automation-log-integration-standard.md
/// - version: 2.0
/// </summary>
public sealed class ADR_970_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-970_2_1: 日志必须使用 JSON 格式")]
    public void ADR_970_2_1_Logs_Must_Use_JSON_Format()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        content.Should().Contain("必须使用 JSON 格式",
            $"❌ ADR-970_2_1 违规：ADR-970 必须要求使用 JSON 格式\n\n" +
            $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §2.1");
    }

    [Fact(DisplayName = "ADR-970_2_2: 必须定义标准 JSON 架构")]
    public void ADR_970_2_2_Standard_JSON_Schema_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr970Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-970-automation-log-integration-standard.md");

        var content = FileSystemTestHelper.ReadFileContent(adr970Path);

        var requiredFields = new[] { "type", "timestamp", "source", "version", "status", "summary", "details" };
        foreach (var field in requiredFields)
        {
            content.Should().Contain($"\"{field}\"",
                $"❌ ADR-970_2_2 违规：标准 JSON 架构必须包含 '{field}' 字段\n\n" +
                $"参考：docs/adr/governance/ADR-970-automation-log-integration-standard.md §2.2");
        }
    }

}
