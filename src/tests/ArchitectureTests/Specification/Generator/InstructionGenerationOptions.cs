namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator;

/// <summary>
/// Agent 指令生成选项
/// 配置如何从 RuleSet 生成 Agent Instructions
/// </summary>
public sealed class InstructionGenerationOptions
{
    /// <summary>
    /// Agent 前缀（例如：AG, TG, HP）
    /// 用于生成指令 ID（格式：{AgentPrefix}-{Number}）
    /// </summary>
    public string AgentPrefix { get; set; } = "GEN";

    /// <summary>
    /// Agent 名称
    /// </summary>
    public string AgentName { get; set; } = "Generated Agent";

    /// <summary>
    /// 起始指令编号
    /// </summary>
    public int StartInstructionNumber { get; set; } = 1;

    /// <summary>
    /// 是否包含 RuleSet API 查询示例
    /// </summary>
    public bool IncludeApiExamples { get; set; } = true;

    /// <summary>
    /// 是否包含约束检查逻辑
    /// </summary>
    public bool IncludeConstraintChecks { get; set; } = true;

    /// <summary>
    /// 是否包含测试命令
    /// </summary>
    public bool IncludeTestCommands { get; set; } = true;

    /// <summary>
    /// 是否包含指导原则
    /// </summary>
    public bool IncludeGuidelines { get; set; } = true;

    /// <summary>
    /// YAML 缩进空格数
    /// </summary>
    public int IndentSpaces { get; set; } = 2;

    /// <summary>
    /// 获取默认选项
    /// </summary>
    public static InstructionGenerationOptions Default => new()
    {
        AgentPrefix = "GEN",
        AgentName = "Generated Agent",
        StartInstructionNumber = 1,
        IncludeApiExamples = true,
        IncludeConstraintChecks = true,
        IncludeTestCommands = true,
        IncludeGuidelines = true,
        IndentSpaces = 2
    };

    /// <summary>
    /// 验证选项的有效性
    /// </summary>
    /// <exception cref="ArgumentException">当选项无效时抛出</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AgentPrefix))
        {
            throw new ArgumentException("AgentPrefix 不能为空", nameof(AgentPrefix));
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(AgentPrefix, "^[A-Z]{2,3}$"))
        {
            throw new ArgumentException(
                "AgentPrefix 必须是 2-3 个大写字母", 
                nameof(AgentPrefix));
        }

        if (string.IsNullOrWhiteSpace(AgentName))
        {
            throw new ArgumentException("AgentName 不能为空", nameof(AgentName));
        }

        if (StartInstructionNumber < 1)
        {
            throw new ArgumentException(
                "StartInstructionNumber 必须大于 0", 
                nameof(StartInstructionNumber));
        }

        if (IndentSpaces < 1 || IndentSpaces > 8)
        {
            throw new ArgumentException(
                "IndentSpaces 必须在 1-8 之间", 
                nameof(IndentSpaces));
        }
    }
}
