namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;

/// <summary>
/// 架构规则集定义接口
/// 
/// 用途：
/// 1. 支持 RuleSet 自注册机制
/// 2. 由 RuleSetRegistry 通过反射自动发现
/// 3. 实现 ADR 编号声明与规则集定义的统一接口
/// 
/// 设计原则：
/// - 每个 RuleSet 必须声明自己的 ADR 编号
/// - 通过 Define() 方法返回完整的规则集定义
/// - Registry 通过反射发现所有实现类，无需手工注册
/// </summary>
public interface IArchitectureRuleSetDefinition
{
    /// <summary>
    /// 获取 ADR 编号
    /// </summary>
    int AdrNumber { get; }

    /// <summary>
    /// 定义规则集
    /// </summary>
    /// <returns>完整的架构规则集</returns>
    ArchitectureRuleSet Define();
}
