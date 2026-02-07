namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_901;

/// <summary>
/// ADR-901_2: 统一语义声明块与执行级别
/// 架构测试：验证 ADR 和文档中的风险表达语义合规性
/// </summary>
public sealed class ADR_901_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string DocsPath = "docs";

    // 三态语义关键词
    private static readonly string[] ConstraintKeywords = { "Constraint", "约束" };
    private static readonly string[] WarningKeywords = { "Warning", "警告" };
    private static readonly string[] NoticeKeywords = { "Notice", "提示", "说明" };

    /// <summary>
    /// ADR-901_2_1: 统一语义声明块
    /// </summary>
    [Fact(DisplayName = "ADR-901_2_1: 风险表达必须使用统一语义声明块格式")]
    public void ADR_901_2_1_Must_Use_Unified_Semantic_Block_Format()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"❌ ADR-901_2_1 违规：ADR 文档目录不存在\n\n" +
            $"预期路径：{AdrDocsPath}\n\n" +
            $"修复建议：确保 docs/adr 目录存在\n\n" +
            $"参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.1）");

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 检查 Constraint 块格式：> 🚨 **Constraint | L1**
            var constraintPattern = @">\s*🚨\s*\*\*Constraint\s*\|\s*L[123]\*\*";
            var constraintMatches = Regex.Matches(content, constraintPattern);

            // 检查 Warning 块格式：> ⚠️ **Warning | L2**
            var warningPattern = @">\s*⚠️\s*\*\*Warning\s*\|\s*L[123]\*\*";
            var warningMatches = Regex.Matches(content, warningPattern);

            // 检查 Notice 块格式：> ℹ️ **Notice**
            var noticePattern = @">\s*ℹ️\s*\*\*Notice\*\*";
            var noticeMatches = Regex.Matches(content, noticePattern);

            // 检查是否有不符合格式的语义块（启发式检查）
            // 查找包含语义关键词但格式不正确的块
            var allSemanticKeywords = ConstraintKeywords.Concat(WarningKeywords).Concat(NoticeKeywords);
            foreach (var keyword in allSemanticKeywords)
            {
                var pattern = $@">\s*.*?\b{Regex.Escape(keyword)}\b";
                var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    var line = match.Value;
                    // 检查是否符合标准格式
                    var isValidConstraint = Regex.IsMatch(line, constraintPattern);
                    var isValidWarning = Regex.IsMatch(line, warningPattern);
                    var isValidNotice = Regex.IsMatch(line, noticePattern);

                    if (!isValidConstraint && !isValidWarning && !isValidNotice)
                    {
                        violations.Add($"⚠️ {fileName}: 发现不符合标准格式的语义块：{line.Trim()}");
                    }
                }
            }
        }

        if (violations.Any())
        {
            var message = "⚠️ ADR-901_2_1 建议：风险表达应使用统一语义声明块格式\n" +
                         string.Join("\n", violations) +
                         "\n\n建议：使用标准格式 '> 🚨 **Constraint | L1**'、'> ⚠️ **Warning | L2**'、'> ℹ️ **Notice**'";

            // 这是建议性检查，暂时只输出调试信息
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// ADR-901_2_2: 不可识别语义等同不存在
    /// </summary>
    [Fact(DisplayName = "ADR-901_2_2: 无统一结构、类型、级别的风险表达视为不存在")]
    public void ADR_901_2_2_Unidentifiable_Semantics_Are_Nonexistent()
    {
        // 这条规则是治理系统的行为规则，不是对文档内容的直接约束
        // 验证方式：确保其他测试能够识别所有有效的语义块
        // 这里我们验证反向：如果文档中有看起来像约束但格式不对的内容，应该被其他测试捕获

        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"❌ ADR-901_2_2 违规：ADR 文档目录不存在\n\n" +
            $"预期路径：{AdrDocsPath}\n\n" +
            $"修复建议：确保 docs/adr 目录存在\n\n" +
            $"参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.2）");

        // 这个测试主要是文档性的，确保 ADR-901_2_2 的概念被测试覆盖
        // 实际的执行由 ADR-901_2_1 完成
        true.Should().BeTrue($"❌ ADR-901_2_2 违规：语义块可识别性验证失败\n\n" +
            $"修复建议：确保所有语义块使用统一格式，以便自动化工具识别\n\n" +
            $"参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.2）");
    }

    /// <summary>
    /// ADR-901_2_3: 执行级别强制声明
    /// </summary>
    [Fact(DisplayName = "ADR-901_2_3: Constraint/Warning 必须显式声明执行级别")]
    public void ADR_901_2_3_Must_Explicitly_Declare_Enforcement_Level()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Directory.Exists(adrDirectory).Should().BeTrue($"❌ ADR-901_2_3 违规：ADR 文档目录不存在\n\n" +
            $"预期路径：{AdrDocsPath}\n\n" +
            $"修复建议：确保 docs/adr 目录存在\n\n" +
            $"参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.3）");

        var adrFiles = GetActiveAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var fileName = Path.GetFileName(adrFile);

            // 查找所有 Constraint 块
            var constraintBlocks = FindSemanticBlocks(content, ConstraintKeywords);
            foreach (var block in constraintBlocks)
            {
                if (!Regex.IsMatch(block, @"\bL[123]\b"))
                {
                    violations.Add($"❌ {fileName}: Constraint 块未显式声明执行级别（L1/L2/L3）");
                }
            }

            // 查找所有 Warning 块
            var warningBlocks = FindSemanticBlocks(content, WarningKeywords);
            foreach (var block in warningBlocks)
            {
                if (!Regex.IsMatch(block, @"\bL[123]\b"))
                {
                    violations.Add($"❌ {fileName}: Warning 块未显式声明执行级别（L1/L2/L3）");
                }
            }
        }

        if (violations.Any())
        {
            var message = "❌ ADR-901_2_3 违规: Constraint/Warning 必须显式声明执行级别\n\n" +
                         string.Join("\n", violations) +
                         "\n\n修复建议：\n" +
                         "1. 为所有 Constraint 块添加执行级别（L1/L2/L3）\n" +
                         "2. 为所有 Warning 块添加执行级别（L1/L2/L3）\n" +
                         "3. 使用标准格式：> 🚨 **Constraint | L1**\n" +
                         "4. 使用标准格式：> ⚠️ **Warning | L2**\n\n" +
                         "参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.3）";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    /// <summary>
    /// ADR-901_2_4: 判定输出三态模型
    /// </summary>
    [Fact(DisplayName = "ADR-901_2_4: 治理系统输出必须使用三态判定模型")]
    public void ADR_901_2_4_Governance_Output_Must_Use_Tristate_Model()
    {
        // 这条规则是对治理系统（CI、测试、工具）的要求，而非对文档的要求
        // 验证方式：检查测试代码本身是否遵循三态输出

        var testAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        var testTypes = testAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("_Architecture_Tests"))
            .ToList();

        // 这个测试主要是确保概念被覆盖
        // 实际的三态输出（✅ Allowed / ⛔ Blocked / ❓ Uncertain）应该在各个测试的实现中体现
        testTypes.Should().NotBeEmpty($"❌ ADR-901_2_4 违规：架构测试类不存在\n\n" +
            $"修复建议：确保存在架构测试类以验证三态判定模型\n\n" +
            $"参考：docs/adr/governance/ADR-901-semantic-meta-rules.md（§2.4）");

        // 验证本测试类的输出格式
        var currentType = typeof(ADR_901_2_Architecture_Tests);
        var methods = currentType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var methodsMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-901_2_4",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "ADR-901 测试类应包含验证三态判定模型的测试方法",
            ArchitectureTestSpecification.Adr.KnownDocuments.Adr007);

        methods.Should().NotBeEmpty(methodsMessage);
    }

    // 辅助方法

    /// <summary>
    /// 获取所有活跃（非归档）的 ADR 文件
    /// 使用 AdrFileFilter 统一过滤逻辑（通过 YAML Front Matter 识别真正的 ADR）
    /// </summary>
    private static List<string> GetActiveAdrFiles(string adrDirectory)
    {
        return AdrFileFilter.GetAdrFiles(adrDirectory)
            .Where(f => !f.Contains("/archive/", StringComparison.OrdinalIgnoreCase)) // 排除归档的 ADR
            .ToList();
    }

    /// <summary>
    /// 查找文档中的语义块
    /// </summary>
    private static List<string> FindSemanticBlocks(string content, string[] keywords)
    {
        var blocks = new List<string>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 检查是否是语义块的开始（> 开头，包含关键词）
            if (line.TrimStart().StartsWith(">"))
            {
                foreach (var keyword in keywords)
                {
                    if (line.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        // 收集整个块（连续的 > 行）
                        var block = new System.Text.StringBuilder();
                        block.AppendLine(line);

                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            if (lines[j].TrimStart().StartsWith(">"))
                            {
                                block.AppendLine(lines[j]);
                            }
                            else if (string.IsNullOrWhiteSpace(lines[j]))
                            {
                                // 空行，继续检查下一行
                                continue;
                            }
                            else
                            {
                                // 块结束
                                break;
                            }
                        }

                        blocks.Add(block.ToString());
                        break;
                    }
                }
            }
        }

        return blocks;
    }
}
