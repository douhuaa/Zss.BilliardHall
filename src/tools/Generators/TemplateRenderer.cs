namespace Zss.BilliardHall.Generators;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.RuleIdLanguage;

/// <summary>
/// 模板渲染器
/// 负责将中间模型渲染为 C# 源代码（统一 LF 行尾）
/// </summary>
public sealed class TemplateRenderer
{
    private readonly IClauseExecutorFactory _executorFactory;

    public TemplateRenderer(IClauseExecutorFactory executorFactory)
    {
        _executorFactory = Guard.NotNull(executorFactory, nameof(executorFactory));
    }

    /// <summary>
    /// 渲染完整的测试类源代码
    /// </summary>
    public string RenderTestClass(
        ArchitectureRuleSet ruleSet,
        string className,
        string namespaceName,
        TestGenerationOptions options)
    {
        Guard.NotNull(ruleSet, nameof(ruleSet));
        Guard.NotNullOrWhiteSpace(className, nameof(className));
        Guard.NotNullOrWhiteSpace(namespaceName, nameof(namespaceName));
        Guard.NotNull(options, nameof(options));

        var parts = new List<string>
        {
            RenderNamespaceDeclaration(namespaceName),
            ""
        };

        if (options.IncludeComments)
        {
            parts.Add(RenderClassDocComment(ruleSet));
        }

        parts.Add($"public sealed class {className}");
        parts.Add("{");
        parts.Add(RenderTestDataProvider(ruleSet, options));
        parts.Add("");

        if (options.UseTheoryPattern)
        {
            parts.Add(RenderTheoryTestMethod(ruleSet, options));
        }
        else
        {
            parts.Add(RenderFactTestMethods(ruleSet, options));
        }

        parts.Add("}");

        return CodeGenerationHelper.NormalizeNewlines(string.Join("\n", parts));
    }

    /// <summary>
    /// 渲染命名空间声明
    /// </summary>
    private string RenderNamespaceDeclaration(string namespaceName)
    {
        return CodeGenerationHelper.BuildNamespaceDeclaration(namespaceName);
    }

    /// <summary>
    /// 渲染类文档注释
    /// </summary>
    private string RenderClassDocComment(ArchitectureRuleSet ruleSet)
    {
        var lines = new[]
        {
            $"ADR-{ruleSet.AdrNumber:D3} 自动生成架构测试",
            "此文件由 ArchitectureTestGenerator 自动生成",
            "",
            $"包含 {ruleSet.RuleCount} 个规则，{ruleSet.ClauseCount} 个条款",
            "",
            "使用说明：",
            "1. 此测试使用 Theory + MemberData 模式",
            "2. 每个 Clause 对应一个测试数据项",
            "3. 可在测试方法中添加具体的 NetArchTest 断言",
            "4. 建议结合 ExecutionBindings 实现具体验证逻辑"
        };

        return CodeGenerationHelper.BuildXmlDocCommentMultiLine(lines, 0);
    }

    /// <summary>
    /// 渲染测试数据提供器方法
    /// </summary>
    private string RenderTestDataProvider(ArchitectureRuleSet ruleSet, TestGenerationOptions options)
    {
        var indent = options.IndentString;
        var parts = new List<string>();

        if (options.IncludeComments)
        {
            parts.Add($"{indent}/// <summary>");
            parts.Add($"{indent}/// 提供所有条款的测试数据");
            parts.Add($"{indent}/// </summary>");
        }

        parts.Add($"{indent}public static IEnumerable<object[]> GetAllClausesData()");
        parts.Add($"{indent}{{");

        foreach (var clause in ruleSet.Clauses.OrderBy(c => c.Id.RuleNumber).ThenBy(c => c.Id.ClauseNumber))
        {
            var ruleId = clause.Id.RuleNumber;
            var clauseId = clause.Id.ClauseNumber;
            var safeName = CSharpIdentifierHelper.ToValidIdentifier(clause.Condition);
            var displayName = CodeGenerationHelper.EscapeStringLiteral(clause.Condition);

            parts.Add($"{indent}{indent}yield return new object[] {{ {ruleId}, {clauseId}, \"{safeName}\", \"{displayName}\" }};");
        }

        parts.Add($"{indent}}}");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// 渲染 Theory 测试方法
    /// </summary>
    private string RenderTheoryTestMethod(ArchitectureRuleSet ruleSet, TestGenerationOptions options)
    {
        var indent = options.IndentString;
        var parts = new List<string>();

        if (options.IncludeComments)
        {
            parts.Add($"{indent}/// <summary>");
            parts.Add($"{indent}/// 执行 ADR-{ruleSet.AdrNumber:D3} 的所有条款测试");
            parts.Add($"{indent}/// </summary>");
            parts.Add($"{indent}/// <param name=\"ruleId\">规则编号</param>");
            parts.Add($"{indent}/// <param name=\"clauseId\">条款编号</param>");
            parts.Add($"{indent}/// <param name=\"safeName\">安全名称（用于标识）</param>");
            parts.Add($"{indent}/// <param name=\"displayName\">显示名称</param>");
        }

        parts.Add($"{indent}[Theory(DisplayName = \"ADR-{ruleSet.AdrNumber:D3} 条款执行测试\")]");
        parts.Add($"{indent}[MemberData(nameof(GetAllClausesData))]");
        parts.Add($"{indent}public void ExecuteClause(int ruleId, int clauseId, string safeName, string displayName)");
        parts.Add($"{indent}{{");

        if (options.IncludeExampleImplementation)
        {
            parts.Add(RenderExampleImplementation(ruleSet, options, indent + indent));
        }
        else
        {
            parts.Add($"{indent}{indent}// TODO: 实现具体的测试逻辑");
            parts.Add($"{indent}{indent}throw new NotImplementedException($\"需要实现 Rule {{ruleId}} Clause {{clauseId}} 的测试逻辑\");");
        }

        parts.Add($"{indent}}}");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// 渲染示例实现
    /// </summary>
    private string RenderExampleImplementation(ArchitectureRuleSet ruleSet, TestGenerationOptions options, string baseIndent)
    {
        var indent = options.IndentString;
        var parts = new List<string>
        {
            $"{baseIndent}// 获取规则集和条款定义",
            $"{baseIndent}var ruleSet = RuleSetRegistry.GetStrict({ruleSet.AdrNumber});",
            $"{baseIndent}var clause = ruleSet.GetClause(ruleId, clauseId);",
            $"{baseIndent}",
            $"{baseIndent}// 验证条款存在",
            $"{baseIndent}clause.Should().NotBeNull($\"Rule {{ruleId}} Clause {{clauseId}} 应该存在\");",
            $"{baseIndent}",
            $"{baseIndent}// 构建 RuleId",
            $"{baseIndent}var ruleIdStr = $\"ADR-{ruleSet.AdrNumber:D3}_{{ruleId}}_{{clauseId}}\";",
            $"{baseIndent}",
            $"{baseIndent}// 根据执行类型执行不同的验证逻辑",
            $"{baseIndent}switch (clause!.ExecutionType)",
            $"{baseIndent}{{"
        };

        // 为每种执行类型生成对应的 case
        foreach (ClauseExecutionType executionType in Enum.GetValues(typeof(ClauseExecutionType)))
        {
            parts.Add($"{baseIndent}{indent}case ClauseExecutionType.{executionType}:");

            var executor = _executorFactory.GetExecutor(executionType);
            var assertionCode = executor.GenerateAssertionCode(
                new ArchitectureClauseDefinition(
                    ArchitectureRuleId.Clause(ruleSet.AdrNumber, 1, 1),
                    "placeholder",
                    "placeholder",
                    executionType),
                baseIndent + indent + indent);

            parts.Add(assertionCode);
            parts.Add($"{baseIndent}{indent}{indent}break;");
            parts.Add($"{baseIndent}");
        }

        parts.Add($"{baseIndent}{indent}default:");
        parts.Add($"{baseIndent}{indent}{indent}throw new NotSupportedException($\"不支持的执行类型: {{clause.ExecutionType}}\");");
        parts.Add($"{baseIndent}}}");
        parts.Add($"{baseIndent}");
        parts.Add($"{baseIndent}// 验证测试已执行");
        parts.Add($"{baseIndent}// 注意：这是一个占位断言，实际测试应该有具体的验证逻辑");
        parts.Add($"{baseIndent}Assert.True(true, $\"Rule {{ruleId}} Clause {{clauseId}} ({{displayName}}) 已执行\");");
        parts.Add($"{baseIndent}");
        parts.Add($"{baseIndent}// 示例：添加具体的 NetArchTest 断言");
        parts.Add($"{baseIndent}// ExecuteConventionTest(ruleIdStr, clause);");
        parts.Add($"{baseIndent}");
        parts.Add($"{baseIndent}static void ExecuteConventionTest(string ruleId, ArchitectureClauseDefinition clause)");
        parts.Add($"{baseIndent}{{");
        parts.Add($"{baseIndent}{indent}// TODO: 实现具体的 NetArchTest 断言");
        parts.Add($"{baseIndent}{indent}// 示例：");
        parts.Add($"{baseIndent}{indent}// var result = Types.InCurrentDomain()");
        parts.Add($"{baseIndent}{indent}//     .That()");
        parts.Add($"{baseIndent}{indent}//     .ResideInNamespace(\"YourNamespace\")");
        parts.Add($"{baseIndent}{indent}//     .Should()");
        parts.Add($"{baseIndent}{indent}//     .NotDependOnAny(\"ForbiddenNamespace\")");
        parts.Add($"{baseIndent}{indent}//     .GetResult();");
        parts.Add($"{baseIndent}{indent}//");
        parts.Add($"{baseIndent}{indent}// result.IsSuccessful.Should().BeTrue(");
        parts.Add($"{baseIndent}{indent}//     AssertionMessageBuilder.BuildFailureMessage(");
        parts.Add($"{baseIndent}{indent}//         ruleId,");
        parts.Add($"{baseIndent}{indent}//         clause.Condition,");
        parts.Add($"{baseIndent}{indent}//         result.FailingTypeNames));");
        parts.Add($"{baseIndent}}}");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// 渲染 Fact 测试方法（独立方法模式）
    /// </summary>
    private string RenderFactTestMethods(ArchitectureRuleSet ruleSet, TestGenerationOptions options)
    {
        var indent = options.IndentString;
        var parts = new List<string>();

        foreach (var clause in ruleSet.Clauses.OrderBy(c => c.Id.RuleNumber).ThenBy(c => c.Id.ClauseNumber))
        {
            var ruleId = clause.Id.RuleNumber;
            var clauseId = clause.Id.ClauseNumber;
            var safeName = CSharpIdentifierHelper.ToValidIdentifier(clause.Condition);
            var methodName = $"Rule{ruleId}_Clause{clauseId}_{safeName}";

            if (options.IncludeComments)
            {
                parts.Add($"{indent}/// <summary>");
                parts.Add($"{indent}/// ADR-{ruleSet.AdrNumber:D3}_Rule{ruleId}_Clause{clauseId}: {clause.Condition}");
                parts.Add($"{indent}/// </summary>");
            }

            parts.Add($"{indent}[Fact(DisplayName = \"ADR-{ruleSet.AdrNumber:D3}_{ruleId}_{clauseId}: {CodeGenerationHelper.EscapeStringLiteral(clause.Condition)}\")]");
            parts.Add($"{indent}public void {methodName}()");
            parts.Add($"{indent}{{");
            parts.Add($"{indent}{indent}// TODO: 实现 Rule {ruleId} Clause {clauseId} 的测试逻辑");
            parts.Add($"{indent}{indent}throw new NotImplementedException();");
            parts.Add($"{indent}}}");
            parts.Add("");
        }

        return string.Join("\n", parts);
    }
}
