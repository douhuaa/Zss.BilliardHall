namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_930;

/// <summary>
/// ADR-930_1: PR 必填信息规范
/// 验证代码审查与 ADR 合规自检流程的规范
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-930_1_1: PR 必须填写变更类型和影响范围
/// - ADR-930_1_2: ADR 相关 PR 自检
/// - ADR-930_1_3: 架构测试失败处理
/// - ADR-930_1_4: 责任人审查与记录
/// - ADR-930_1_5: 架构破例标注和记录
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-930-code-review-compliance.md
/// </summary>
public sealed class ADR_930_1_Architecture_Tests
{
    /// <summary>
    /// ADR-930_1_1: PR 必须填写变更类型和影响范围
    /// 验证 PR 模板存在性和基本结构（§1.1）
    ///
    /// 注意：完整的 PR 必填字段验证需要 GitHub Actions / PR 模板机制执行（L2）
    /// 此测试仅验证 PR 模板文件的存在性和基本结构
    /// </summary>
    [Fact(DisplayName = "ADR-930_1_1: PR 模板必须定义必填字段")]
    public void ADR_930_1_1_PR_Template_Must_Define_Required_Fields()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        // 检查 PR 模板是否存在（GitHub 标准位置）
        var prTemplatePaths = new[]
        {
            Path.Combine(repoRoot, ".github/pull_request_template.md"),
            Path.Combine(repoRoot, ".github/PULL_REQUEST_TEMPLATE.md"),
            Path.Combine(repoRoot, "docs/pull_request_template.md")
        };

        var prTemplateExists = prTemplatePaths.Any(File.Exists);

        prTemplateExists.Should().BeTrue(
            $"❌ ADR-930_1_1 违规：未找到 PR 模板文件\n\n" +
            $"根据 ADR-930_1_1：所有 PR 必须填写变更类型和影响范围，需要 PR 模板确保执行。\n\n" +
            $"修复建议：\n" +
            $"  1. 在 .github/ 目录下创建 pull_request_template.md\n" +
            $"  2. 模板必须包含：\n" +
            $"     - 变更类型（feat/fix/docs/refactor/test/chore）\n" +
            $"     - 影响模块/层\n" +
            $"     - ADR 相关性（如涉及 ADR，标注编号）\n\n" +
            $"参考：docs/adr/governance/ADR-930-code-review-compliance.md §1.1");

        // 验证模板内容（如果存在）
        var existingTemplate = prTemplatePaths.FirstOrDefault(File.Exists);
        if (existingTemplate != null)
        {
            var content = FileSystemTestHelper.ReadFileContent(existingTemplate);

            // 验证模板包含变更类型提示
            var hasChangeTypeGuidance = content.Contains("变更类型", StringComparison.OrdinalIgnoreCase) ||
                                       content.Contains("change type", StringComparison.OrdinalIgnoreCase) ||
                                       content.Contains("feat", StringComparison.OrdinalIgnoreCase) ||
                                       content.Contains("fix", StringComparison.OrdinalIgnoreCase);

            var missingMessage = AssertionMessageBuilder.BuildContentMissingMessage(
                ruleId: "ADR-930_1_1",
                filePath: existingTemplate,
                missingContent: "变更类型字段",
                remediationSteps: new[]
                {
                    "在 PR 模板中添加变更类型选择或说明区域",
                    "包含：feat/fix/docs/refactor/test/chore"
                },
                adrReference: "docs/adr/governance/ADR-930-code-review-compliance.md");

            hasChangeTypeGuidance.Should().BeTrue(missingMessage);
        }
    }

    /// <summary>
    /// ADR-930_1_2: ADR 相关 PR 自检
    /// 验证 ADR 文档完整性，确保变更 ADR 时有明确指引（§1.2）
    ///
    /// 注意：实际的 Copilot 自检执行需要在 PR 流程中通过 code_review 工具完成（L2）
    /// 此测试验证 ADR 文档本身的合规性，确保自检有规可循
    /// </summary>
    [Fact(DisplayName = "ADR-930_1_2: ADR 文档必须可被自检")]
    public void ADR_930_1_2_ADR_Documents_Must_Be_Self_Checkable()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-930-code-review-compliance.md");

        var fileMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-930_1_2",
            filePath: adrFile,
            fileDescription: "ADR-930 代码审查合规文档",
            remediationSteps: new[]
            {
                "确保 ADR-930 文档存在以定义自检规则",
                "文档必须包含 Copilot 自检要求"
            },
            adrReference: "docs/adr/governance/ADR-930-code-review-compliance.md");

        File.Exists(adrFile).Should().BeTrue(fileMessage);

        var content = FileSystemTestHelper.ReadFileContent(adrFile);

        // 验证文档包含自检要求
        var missingMessage1 = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-930_1_2",
            filePath: adrFile,
            missingContent: "Copilot 自检",
            remediationSteps: new[]
            {
                "在 ADR-930 中定义 Copilot 自检要求",
                "明确自检流程和检查项"
            },
            adrReference: "docs/adr/governance/ADR-930-code-review-compliance.md");

        content.Should().Contain("Copilot 自检", missingMessage1);

        var missingMessage2 = AssertionMessageBuilder.BuildContentMissingMessage(
            ruleId: "ADR-930_1_2",
            filePath: adrFile,
            missingContent: "code_review",
            remediationSteps: new[]
            {
                "在 ADR-930 中引用 code_review 工具",
                "说明工具在自检流程中的作用"
            },
            adrReference: "docs/adr/governance/ADR-930-code-review-compliance.md");

        content.Should().Contain("code_review", missingMessage2);
    }

    /// <summary>
    /// ADR-930_1_3: 架构测试失败处理
    /// 验证架构测试项目存在且可执行（§1.3）
    ///
    /// 注意：实际的测试失败阻断由 CI 执行（L1）
    /// 此测试确保 ArchitectureTests 项目存在且可被发现
    /// </summary>
    [Fact(DisplayName = "ADR-930_1_3: ArchitectureTests 项目必须存在")]
    public void ADR_930_1_3_ArchitectureTests_Project_Must_Exist()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        // 查找 ArchitectureTests 项目文件
        var testProjectPath = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ArchitectureTests.csproj");

        File.Exists(testProjectPath).Should().BeTrue(
            $"❌ ADR-930_1_3 违规：ArchitectureTests 项目不存在：{testProjectPath}\n\n" +
            $"根据 ADR-930_1_3：架构测试失败必须能够阻断 CI，需要独立的 ArchitectureTests 项目。\n\n" +
            $"修复建议：\n" +
            $"  1. 创建独立的 ArchitectureTests 项目\n" +
            $"  2. 确保项目被 CI 流程执行\n" +
            $"  3. 配置测试失败时阻断合并\n\n" +
            $"参考：docs/adr/governance/ADR-930-code-review-compliance.md §1.3\n" +
            $"参考：docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
    }

    /// <summary>
    /// ADR-930_1_4: 责任人审查与记录
    /// 验证 CODEOWNERS 文件存在性（§1.4）
    ///
    /// 注意：实际的责任人审查由 GitHub Branch Protection 执行（L2）
    /// 此测试确保 CODEOWNERS 文件存在以定义审查责任人
    /// </summary>
    [Fact(DisplayName = "ADR-930_1_4: CODEOWNERS 必须定义审查责任人")]
    public void ADR_930_1_4_CODEOWNERS_Must_Define_Reviewers()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        // 检查 CODEOWNERS 文件
        var codeownersPath = Path.Combine(repoRoot, "CODEOWNERS");
        var githubCodeownersPath = Path.Combine(repoRoot, ".github/CODEOWNERS");

        var codeownersExists = File.Exists(codeownersPath) || File.Exists(githubCodeownersPath);

        codeownersExists.Should().BeTrue(
            $"❌ ADR-930_1_4 违规：未找到 CODEOWNERS 文件\n\n" +
            $"根据 ADR-930_1_4：每个 PR 必须至少有一名责任人审查，需要 CODEOWNERS 定义。\n\n" +
            $"修复建议：\n" +
            $"  1. 在仓库根目录或 .github/ 目录下创建 CODEOWNERS 文件\n" +
            $"  2. 为关键路径定义责任人（如 docs/adr/** 指定架构师）\n" +
            $"  3. 配合 GitHub Branch Protection 启用必需审查\n\n" +
            $"参考：docs/adr/governance/ADR-930-code-review-compliance.md §1.4");
    }

    /// <summary>
    /// ADR-930_1_5: 架构破例标注和记录
    /// 验证破例治理机制文档存在（§1.5）
    ///
    /// 注意：实际的破例标注和批准需要人工执行（L2）
    /// 此测试确保破例治理规则已定义
    /// </summary>
    [Fact(DisplayName = "ADR-930_1_5: 破例治理规则必须已定义")]
    public void ADR_930_1_5_Exception_Governance_Must_Be_Defined()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;

        // 检查 ADR-900（定义破例治理）
        var adr900Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-900-architecture-tests.md");

        File.Exists(adr900Path).Should().BeTrue(
            $"❌ ADR-930_1_5 违规：破例治理规则（ADR-900）不存在\n\n" +
            $"根据 ADR-930_1_5：架构破例必须有明确的标注和记录机制。\n\n" +
            $"修复建议：\n" +
            $"  1. 确保 ADR-900 存在并定义破例治理机制\n" +
            $"  2. 破例必须标注 [Architecture Exception]\n" +
            $"  3. 破例必须包含批准人和理由\n\n" +
            $"参考：docs/adr/governance/ADR-930-code-review-compliance.md §1.5\n" +
            $"参考：docs/adr/governance/ADR-900-architecture-tests.md");

        var content = File.ReadAllText(adr900Path);

        // 验证 ADR-900 包含破例治理内容
        var hasExceptionGuidance = content.Contains("破例", StringComparison.OrdinalIgnoreCase) ||
                                   content.Contains("exception", StringComparison.OrdinalIgnoreCase);

        hasExceptionGuidance.Should().BeTrue(
            $"❌ ADR-930_1_5 违规：ADR-900 未定义破例治理机制\n\n" +
            $"修复建议：在 ADR-900 中明确定义架构破例的标注和批准流程\n\n" +
            $"参考：docs/adr/governance/ADR-930-code-review-compliance.md §1.5");
    }

}
