namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907;

/// <summary>
/// 条款注册策略接口
/// 定义如何将一个 ClauseSpec 注册到 ArchitectureRuleSet 中
/// 
/// 策略模式的核心接口，替代原有的 switch 语句
/// 每种 ClauseExecutionType 对应一个具体的注册策略实现
/// 
/// 策略职责：
/// 1. 根据 ClauseSpec 的元数据构建规则
/// 2. 根据 ClauseExecutionBinding（如果存在）绑定具体的执行处理器
/// 3. 处理降级场景（如 Runtime -> Convention 的回退）
/// </summary>
public interface IClauseRegistrationStrategy
{
    /// <summary>
    /// 注册条款到规则集
    /// </summary>
    /// <param name="ruleSet">目标规则集</param>
    /// <param name="spec">条款规范</param>
    /// <param name="binding">执行绑定（可选）</param>
    void Register(ArchitectureRuleSet ruleSet, ClauseSpec spec, ClauseExecutionBinding? binding);
}
