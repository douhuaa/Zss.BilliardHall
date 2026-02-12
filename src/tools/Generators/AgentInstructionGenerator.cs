using Zss.BilliardHall.Generators.Interfaces;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Generators.Implementations;
using Zss.BilliardHall.Generators.Models;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// Agent 指令生成器实现
/// 将 ArchitectureRuleSet 转换为 YAML 格式的 Agent Instructions
/// 使用依赖注入和 YamlDotNet 进行 YAML 序列化
/// </summary>
public sealed class AgentInstructionGenerator : IAgentInstructionGenerator
{
    private readonly IYamlSerializer _yamlSerializer;
    private readonly ICommandValidator? _commandValidator;

    /// <summary>
    /// 构造函数（使用默认实现）
    /// </summary>
    public AgentInstructionGenerator()
        : this(new YamlDotNetSerializer(), new CommandValidator())
    {
    }

    /// <summary>
    /// 构造函数（依赖注入）
    /// </summary>
    public AgentInstructionGenerator(
        IYamlSerializer yamlSerializer,
        ICommandValidator? commandValidator = null)
    {
        _yamlSerializer = yamlSerializer ?? throw new ArgumentNullException(nameof(yamlSerializer));
        _commandValidator = commandValidator;
    }

    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions（使用默认选项）
    /// </summary>
    public string GenerateInstructions(ArchitectureRuleSet ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        return GenerateInstructions(ruleSet, InstructionGenerationOptions.Default);
    }

    /// <summary>
    /// 从 RuleSet 生成 YAML 格式的 Agent Instructions（带选项）
    /// </summary>
    public string GenerateInstructions(ArchitectureRuleSet ruleSet, InstructionGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        // 步骤 1：构建指令模型列表
        var instructions = BuildInstructions(ruleSet, options);

        // 步骤 2：创建容器对象
        var container = new InstructionsContainer
        {
            Instructions = instructions
        };

        // 步骤 3：序列化为 YAML
        return _yamlSerializer.Serialize(container);
    }

    /// <summary>
    /// 构建指令模型列表
    /// </summary>
    private List<InstructionModel> BuildInstructions(
        ArchitectureRuleSet ruleSet,
        InstructionGenerationOptions options)
    {
        var orderedRules = ruleSet.Rules
            .OrderBy(r => r.Id.RuleNumber)
            .ToList();

        if (!orderedRules.Any())
        {
            return new List<InstructionModel>();
        }

        var builder = new InstructionModelBuilder(ruleSet, options);
        var instructions = new List<InstructionModel>();
        int instructionNumber = options.StartInstructionNumber;

        foreach (var rule in orderedRules)
        {
            var instruction = builder.BuildInstruction(rule, ruleSet, instructionNumber);
            
            // 如果启用命令验证，验证所有命令
            if (_commandValidator != null && instruction.Commands != null)
            {
                ValidateCommands(instruction.Commands);
            }

            instructions.Add(instruction);
            instructionNumber++;
        }

        return instructions;
    }

    /// <summary>
    /// 验证命令的有效性
    /// </summary>
    private void ValidateCommands(Dictionary<string, string> commands)
    {
        foreach (var (key, command) in commands)
        {
            if (_commandValidator != null && !_commandValidator.IsValidCommand(command))
            {
                throw new InvalidOperationException(
                    $"命令 '{key}' 包含危险模式：{command}");
            }
        }
    }
}
