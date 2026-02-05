namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 架构测试断言消息构建器
/// 提供统一的断言消息格式模板，确保所有架构测试的错误消息保持一致性
/// 
/// 标准格式（基于 ARCHITECTURE-TEST-GUIDELINES.md）：
/// ❌ ADR-XXX_Y_Z 违规：<简短问题描述>
/// 
/// 当前状态：<具体违规情况>
/// 
/// 修复建议：
/// 1. <具体步骤 1>
/// 2. <具体步骤 2>
/// 3. <具体步骤 3>
/// 
/// 参考：<ADR 文档路径> §ADR-XXX_Y_Z
/// </summary>
public static class AssertionMessageBuilder
{
    /// <summary>
    /// 构建标准格式的断言消息
    /// </summary>
    /// <param name="ruleId">规则编号（格式：ADR-XXX_Y_Z）</param>
    /// <param name="summary">简短问题描述（一句话说明违规内容）</param>
    /// <param name="currentState">当前状态/违规情况的具体描述</param>
    /// <param name="remediationSteps">修复建议步骤列表</param>
    /// <param name="adrReference">ADR 文档路径（相对于仓库根目录）</param>
    /// <param name="includeClauseReference">是否在参考中包含章节引用（§ADR-XXX_Y_Z）</param>
    /// <returns>格式化的断言消息</returns>
    public static string Build(
        string ruleId,
        string summary,
        string currentState,
        IEnumerable<string> remediationSteps,
        string adrReference,
        bool includeClauseReference = false)
    {
        var sb = new StringBuilder();
        
        // 1. 违规标题（必需）
        sb.AppendLine($"❌ {ruleId} 违规：{summary}");
        sb.AppendLine();
        
        // 2. 当前状态（必需）
        sb.AppendLine($"当前状态：{currentState}");
        sb.AppendLine();
        
        // 3. 修复建议（必需，使用编号列表）
        sb.AppendLine("修复建议：");
        int stepNumber = 1;
        foreach (var step in remediationSteps)
        {
            sb.AppendLine($"{stepNumber}. {step}");
            stepNumber++;
        }
        sb.AppendLine();
        
        // 4. 参考（必需）
        if (includeClauseReference)
        {
            sb.Append($"参考：{adrReference} §{ruleId}");
        }
        else
        {
            sb.Append($"参考：{adrReference}");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// 构建标准格式的断言消息（包含违规类型列表）
    /// 当有多个违规项需要列举时使用
    /// </summary>
    /// <param name="ruleId">规则编号（格式：ADR-XXX_Y_Z）</param>
    /// <param name="summary">简短问题描述</param>
    /// <param name="failingTypes">违规类型列表</param>
    /// <param name="remediationSteps">修复建议步骤列表</param>
    /// <param name="adrReference">ADR 文档路径</param>
    /// <param name="includeClauseReference">是否在参考中包含章节引用</param>
    /// <returns>格式化的断言消息</returns>
    public static string BuildWithViolations(
        string ruleId,
        string summary,
        IEnumerable<string> failingTypes,
        IEnumerable<string> remediationSteps,
        string adrReference,
        bool includeClauseReference = false)
    {
        var violations = failingTypes.Any()
            ? string.Join("\n", failingTypes.Select(t => $"  - {t}"))
            : "（无违规类型详情）";
            
        return Build(
            ruleId,
            summary,
            $"违规类型：\n{violations}",
            remediationSteps,
            adrReference,
            includeClauseReference);
    }

    /// <summary>
    /// 构建标准格式的断言消息（包含问题分析）
    /// 当需要解释问题背景和影响时使用
    /// </summary>
    /// <param name="ruleId">规则编号（格式：ADR-XXX_Y_Z）</param>
    /// <param name="summary">简短问题描述</param>
    /// <param name="currentState">当前状态/违规情况</param>
    /// <param name="problemAnalysis">问题分析（解释问题背景和影响）</param>
    /// <param name="remediationSteps">修复建议步骤列表</param>
    /// <param name="adrReference">ADR 文档路径</param>
    /// <param name="includeClauseReference">是否在参考中包含章节引用</param>
    /// <returns>格式化的断言消息</returns>
    public static string BuildWithAnalysis(
        string ruleId,
        string summary,
        string currentState,
        string problemAnalysis,
        IEnumerable<string> remediationSteps,
        string adrReference,
        bool includeClauseReference = false)
    {
        var sb = new StringBuilder();
        
        // 1. 违规标题（必需）
        sb.AppendLine($"❌ {ruleId} 违规：{summary}");
        sb.AppendLine();
        
        // 2. 当前状态（必需）
        sb.AppendLine($"当前状态：{currentState}");
        sb.AppendLine();
        
        // 3. 问题分析（可选）
        sb.AppendLine($"问题分析：");
        sb.AppendLine(problemAnalysis);
        sb.AppendLine();
        
        // 4. 修复建议（必需）
        sb.AppendLine("修复建议：");
        int stepNumber = 1;
        foreach (var step in remediationSteps)
        {
            sb.AppendLine($"{stepNumber}. {step}");
            stepNumber++;
        }
        sb.AppendLine();
        
        // 5. 参考（必需）
        if (includeClauseReference)
        {
            sb.Append($"参考：{adrReference} §{ruleId}");
        }
        else
        {
            sb.Append($"参考：{adrReference}");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// 构建简化格式的断言消息（仅包含必需字段）
    /// 适用于简单的文件存在性检查等场景
    /// </summary>
    /// <param name="ruleId">规则编号</param>
    /// <param name="summary">简短问题描述</param>
    /// <param name="currentState">当前状态</param>
    /// <param name="remediation">单个修复建议（会自动转换为编号列表）</param>
    /// <param name="adrReference">ADR 文档路径</param>
    /// <returns>格式化的断言消息</returns>
    public static string BuildSimple(
        string ruleId,
        string summary,
        string currentState,
        string remediation,
        string adrReference)
    {
        return Build(
            ruleId,
            summary,
            currentState,
            new[] { remediation },
            adrReference,
            includeClauseReference: false);
    }

    /// <summary>
    /// 从 NetArchTest 结果构建断言消息
    /// </summary>
    /// <param name="ruleId">规则编号</param>
    /// <param name="summary">简短问题描述</param>
    /// <param name="failingTypeNames">失败的类型完整名称列表</param>
    /// <param name="remediationSteps">修复建议步骤</param>
    /// <param name="adrReference">ADR 文档路径</param>
    /// <returns>格式化的断言消息</returns>
    public static string BuildFromArchTestResult(
        string ruleId,
        string summary,
        IEnumerable<string?>? failingTypeNames,
        IEnumerable<string> remediationSteps,
        string adrReference)
    {
        var violations = failingTypeNames?.Where(t => t != null).Select(t => t!)?.Any() == true
            ? failingTypeNames.Where(t => t != null).Select(t => t!)
            : new[] { "（无违规类型详情）" };
            
        return BuildWithViolations(
            ruleId,
            summary,
            violations,
            remediationSteps,
            adrReference,
            includeClauseReference: false);
    }
}
