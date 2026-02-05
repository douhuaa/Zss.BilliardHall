namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_907_A;

/// <summary>
/// ADR-907-A_2: 对齐失败策略
/// 验证对齐失败时的处理策略和追踪机制
///
/// 测试覆盖映射（严格遵循 ADR-907-A v1.2 Rule/Clause 体系）：
/// - ADR-907-A_2_1: 部分对齐允许条件 → ADR_907_A_2_1_Partial_Alignment_Conditions
/// - ADR-907-A_2_2: 部分对齐最低要求 → ADR_907_A_2_2_Minimum_Partial_Alignment_Requirements
/// - ADR-907-A_2_3: 部分对齐追踪机制 → ADR_907_A_2_3_Partial_Alignment_Tracking
/// - ADR-907-A_2_4: 部分对齐合并准入 → ADR_907_A_2_4_Partial_Alignment_Merge_Criteria
/// - ADR-907-A_2_5: 对齐失败升级路径 → ADR_907_A_2_5_Alignment_Failure_Escalation
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-907-A-adr-alignment-execution-standard.md
/// </summary>
public sealed class ADR_907_A_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";

    /// <summary>
    /// ADR-907-A_2_1: 部分对齐允许条件
    /// 验证部分对齐的允许情况
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_2_1: 部分对齐必须符合允许条件")]
    public void ADR_907_A_2_1_Partial_Alignment_Conditions()
    {
        // 这个测试主要验证 ADR-907-A 本身定义了部分对齐条件
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-A-adr-alignment-execution-standard.md");

        File.Exists(adr907AFile).Should().BeTrue(
            $"❌ ADR-907-A_2_1 违规：ADR-907-A 文档不存在");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("部分对齐",
            $"❌ ADR-907-A_2_1 违规：ADR-907-A 未定义部分对齐概念");

        content.Should().Contain("Decision 章节为空",
            $"❌ ADR-907-A_2_1 违规：ADR-907-A 未定义部分对齐的允许条件");
    }

    /// <summary>
    /// ADR-907-A_2_2: 部分对齐最低要求
    /// 验证部分对齐的最低标准
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_2_2: 部分对齐必须满足最低要求")]
    public void ADR_907_A_2_2_Minimum_Partial_Alignment_Requirements()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("至少生成 Rule 1",
            $"❌ ADR-907-A_2_2 违规：ADR-907-A 未定义部分对齐的最低要求");

        content.Should().Contain("Enforcement 章节",
            $"❌ ADR-907-A_2_2 违规：ADR-907-A 未要求部分对齐包含 Enforcement 章节");
    }

    /// <summary>
    /// ADR-907-A_2_3: 部分对齐追踪机制
    /// 验证部分对齐的追踪要求
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_2_3: 部分对齐必须有追踪机制")]
    public void ADR_907_A_2_3_Partial_Alignment_Tracking()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("Follow-up Issue",
            $"❌ ADR-907-A_2_3 违规：ADR-907-A 未要求创建 Follow-up Issue");

        content.Should().Contain("关联原 PR",
            $"❌ ADR-907-A_2_3 违规：ADR-907-A 未要求关联原 PR");
    }

    /// <summary>
    /// ADR-907-A_2_4: 部分对齐合并准入
    /// 验证部分对齐的合并条件
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_2_4: 部分对齐合并必须满足准入条件")]
    public void ADR_907_A_2_4_Partial_Alignment_Merge_Criteria()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("Approved: Partial Alignment",
            $"❌ ADR-907-A_2_4 违规：ADR-907-A 未定义部分对齐的批准流程");

        content.Should().Contain("架构委员会明确批准",
            $"❌ ADR-907-A_2_4 违规：ADR-907-A 未要求架构委员会批准");
    }

    /// <summary>
    /// ADR-907-A_2_5: 对齐失败升级路径
    /// 验证对齐失败时的升级处理
    /// </summary>
    [Fact(DisplayName = "ADR-907-A_2_5: 对齐失败必须有升级路径")]
    public void ADR_907_A_2_5_Alignment_Failure_Escalation()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adr907AFile = Path.Combine(repoRoot, AdrDocsPath, "governance", "ADR-907-A-adr-alignment-execution-standard.md");

        var content = File.ReadAllText(adr907AFile);

        content.Should().Contain("连续 2 次尝试",
            $"❌ ADR-907-A_2_5 违规：ADR-907-A 未定义失败重试次数");

        content.Should().Contain("重写 ADR",
            $"❌ ADR-907-A_2_5 违规：ADR-907-A 未定义失败时的重写选项");
    }
}
