namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_900;

/// <summary>
/// ADR-900_3: 破例治理机制
/// 验证架构破例的记录、到期和偿还机制
///
/// 测试覆盖映射（严格遵循 ADR-900 v4.0 Rule/Clause 体系）：
/// - ADR-900_3_1: 破例强制要求 → ADR_900_3_1_Exception_Requirements_Must_Be_Met
/// - ADR-900_3_2: CI 自动监控机制 → ADR_900_3_2_CI_Must_Monitor_Exceptions
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_3_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-900_3_1: 破例强制要求
    /// 验证任何架构破例都必须满足记录完整性要求
    /// </summary>
    [Fact(DisplayName = "ADR-900_3_1: 架构破例必须满足强制要求")]
    public void ADR_900_3_1_Exception_Requirements_Must_Be_Met()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");

        // 检查是否存在 arch-violations.md 文件
        var violationsFile = Path.Combine(repoRoot, "arch-violations.md");
        var hasViolationsFile = File.Exists(violationsFile);

        if (hasViolationsFile)
        {
            var content = File.ReadAllText(violationsFile);

            // 验证破例记录格式
            content.Should().Contain("| ADR |",
                $"❌ ADR-900_3_1 违规：arch-violations.md 缺少 ADR 列\n\n" +
                $"修复建议：\n" +
                $"  使用标准表格格式记录破例：\n" +
                $"  | ADR | 规则 | 到期版本 | 负责人 | 偿还计划 | 状态 |\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §3.1");

            content.Should().Contain("到期版本",
                $"❌ ADR-900_3_1 违规：arch-violations.md 缺少到期版本记录\n\n" +
                $"修复建议：\n" +
                $"  为每个破例指定明确的到期版本\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §3.1");

            content.Should().Contain("负责人",
                $"❌ ADR-900_3_1 违规：arch-violations.md 缺少负责人记录\n\n" +
                $"修复建议：\n" +
                $"  为每个破例指定明确的责任人\n\n" +
                $"参考：docs/adr/governance/ADR-900-architecture-tests.md §3.1");
        }
        // 如果没有破例文件，这是好的（没有破例）
    }

    /// <summary>
    /// ADR-900_3_2: CI 自动监控机制
    /// 验证 CI 必须自动扫描破例状态并在过期时失败构建
    /// </summary>
    [Fact(DisplayName = "ADR-900_3_2: CI 必须自动监控破例状态")]
    public void ADR_900_3_2_CI_Must_Monitor_Exceptions()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");

        // 检查是否存在 CI 配置文件
        var ciFiles = new[]
        {
            Path.Combine(repoRoot, ".github", "workflows", "ci.yml"),
            Path.Combine(repoRoot, ".github", "workflows", "ci.yaml"),
            Path.Combine(repoRoot, "azure-pipelines.yml"),
            Path.Combine(repoRoot, ".gitlab-ci.yml")
        };

        var ciFile = ciFiles.FirstOrDefault(File.Exists);

        ciFile.Should().NotBeNull(
            $"❌ ADR-900_3_2 违规：未找到 CI 配置文件\n\n" +
            $"支持的 CI 文件：\n" +
            $"- .github/workflows/ci.yml\n" +
            $"- .github/workflows/ci.yaml\n" +
            $"- azure-pipelines.yml\n" +
            $"- .gitlab-ci.yml\n\n" +
            $"修复建议：\n" +
            $"  配置 CI 流水线以监控 arch-violations.md 中的过期破例\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §3.2");

        var ciContent = File.ReadAllText(ciFile);

        // 检查是否包含破例监控逻辑
        var hasExceptionMonitoring = ciContent.Contains("arch-violations") ||
                                   ciContent.Contains("violations") ||
                                   ciContent.Contains("exception");

        hasExceptionMonitoring.Should().BeTrue(
            $"❌ ADR-900_3_2 违规：CI 配置未包含破例监控\n\n" +
            $"修复建议：\n" +
            $"  在 CI 流水线中添加步骤：\n" +
            $"  - 扫描 arch-violations.md\n" +
            $"  - 检查是否有过期破例\n" +
            $"  - 过期破例导致构建失败\n\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md §3.2");
    }
}
