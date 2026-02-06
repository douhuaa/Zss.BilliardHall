namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Examples;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.DecisionLanguage;

/// <summary>
/// DecisionLanguage 使用示例
/// 演示如何使用新的裁决语言模型进行架构规则解析和判定
/// </summary>
public static class DecisionLanguageUsageExamples
{
    /// <summary>
    /// 示例 1：基本的裁决语言解析
    /// </summary>
    public static void Example1_BasicParsing()
    {
        // 解析不同级别的裁决语言
        var mustResult = ArchitectureTestSpecification.DecisionLanguage.Parse("必须遵循模块边界");
        Console.WriteLine($"Must: Level={mustResult.Level}, IsBlocking={mustResult.IsBlocking}");
        // 输出: Must: Level=Must, IsBlocking=True

        var mustNotResult = ArchitectureTestSpecification.DecisionLanguage.Parse("禁止跨模块直接调用");
        Console.WriteLine($"MustNot: Level={mustNotResult.Level}, IsBlocking={mustNotResult.IsBlocking}");
        // 输出: MustNot: Level=MustNot, IsBlocking=True

        var shouldResult = ArchitectureTestSpecification.DecisionLanguage.Parse("应该优先使用 Handler 模式");
        Console.WriteLine($"Should: Level={shouldResult.Level}, IsBlocking={shouldResult.IsBlocking}");
        // 输出: Should: Level=Should, IsBlocking=False
    }

    /// <summary>
    /// 示例 2：在架构测试中使用三态判定
    /// </summary>
    public static void Example2_ThreeStateDecision()
    {
        var sentence = "必须实现 IHandler 接口";
        var decision = ArchitectureTestSpecification.DecisionLanguage.Parse(sentence);

        if (decision.IsBlocking)
        {
            // ❌ 阻断级别 - 直接失败测试
            Console.WriteLine("BLOCKED: 违反了强制性架构约束");
            // throw new ArchitectureViolationException(sentence);
        }
        else if (decision.IsDecision)
        {
            // ⚠️ 警告级别 - 记录但不阻断
            Console.WriteLine("WARNING: 不符合推荐做法");
        }
        else
        {
            // ✅ 允许 - 无裁决语言或符合规范
            Console.WriteLine("ALLOWED: 无架构约束");
        }
    }

    /// <summary>
    /// 示例 3：使用辅助方法快速判定
    /// </summary>
    public static void Example3_HelperMethods()
    {
        var text = "禁止在 Endpoint 中编写业务逻辑";

        // 快速判断是否为阻断级别
        if (ArchitectureTestSpecification.DecisionLanguage.IsBlockingDecision(text))
        {
            Console.WriteLine("这是一个阻断级别的规则");
        }

        // 判断是否包含任何裁决语言
        if (ArchitectureTestSpecification.DecisionLanguage.HasDecisionLanguage(text))
        {
            Console.WriteLine("检测到裁决语言");
        }
    }

    /// <summary>
    /// 示例 4：批量处理 ADR 文档规则
    /// </summary>
    public static void Example4_BatchProcessing()
    {
        var adrRules = new[]
        {
            "必须使用 Handler 模式处理命令",
            "应该考虑使用 CQRS 分离读写",
            "禁止直接访问数据库连接",
            "推荐使用依赖注入管理生命周期"
        };

        var blockingRules = 0;
        var warningRules = 0;
        var informationalRules = 0;

        foreach (var rule in adrRules)
        {
            var decision = ArchitectureTestSpecification.DecisionLanguage.Parse(rule);

            if (decision.IsBlocking)
            {
                blockingRules++;
                Console.WriteLine($"[BLOCKING] {rule}");
            }
            else if (decision.IsDecision)
            {
                warningRules++;
                Console.WriteLine($"[WARNING] {rule}");
            }
            else
            {
                informationalRules++;
                Console.WriteLine($"[INFO] {rule}");
            }
        }

        Console.WriteLine($"\n统计：阻断={blockingRules}, 警告={warningRules}, 信息={informationalRules}");
    }

    /// <summary>
    /// 示例 5：处理无裁决语言的文本
    /// </summary>
    public static void Example5_HandlingNone()
    {
        var descriptiveText = "Handler 模式是一种常见的设计模式";
        var result = ArchitectureTestSpecification.DecisionLanguage.Parse(descriptiveText);

        if (result == DecisionResult.None)
        {
            Console.WriteLine("这是描述性文本，不包含裁决语言");
        }

        // 或者使用 IsDecision 属性
        if (!result.IsDecision)
        {
            Console.WriteLine("未检测到裁决语言");
        }
    }

    /// <summary>
    /// 示例 6：访问规则配置
    /// </summary>
    public static void Example6_AccessingRules()
    {
        var rules = ArchitectureTestSpecification.DecisionLanguage.Rules;

        Console.WriteLine($"配置了 {rules.Count} 条裁决规则：");
        foreach (var rule in rules)
        {
            Console.WriteLine($"- Level: {rule.Level}");
            Console.WriteLine($"  Keywords: {string.Join(", ", rule.Keywords)}");
            Console.WriteLine($"  IsBlocking: {rule.IsBlocking}");
        }
    }
}
