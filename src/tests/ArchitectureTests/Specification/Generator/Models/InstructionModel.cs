namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Models;

/// <summary>
/// Agent 指令模型
/// 用于 YAML 序列化的数据结构
/// </summary>
public sealed class InstructionModel
{
    /// <summary>
    /// 指令 ID（如 ADR907-001）
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 指令描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 执行动作
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// 触发条件列表
    /// </summary>
    public required List<string> Conditions { get; init; }

    /// <summary>
    /// 输出格式
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// 可用工具列表
    /// </summary>
    public required List<string> Tools { get; init; }

    /// <summary>
    /// 反馈机制列表
    /// </summary>
    public required List<string> Feedback { get; init; }

    /// <summary>
    /// 指导说明（可选）
    /// </summary>
    public List<string>? Guidelines { get; init; }

    /// <summary>
    /// 测试命令（可选）
    /// </summary>
    public Dictionary<string, string>? Commands { get; init; }
}

/// <summary>
/// 指令集合容器（用于 YAML 根对象）
/// </summary>
public sealed class InstructionsContainer
{
    /// <summary>
    /// 指令列表
    /// </summary>
    public required List<InstructionModel> Instructions { get; init; }
}
