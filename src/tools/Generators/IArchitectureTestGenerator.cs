using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Generators;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// 架构测试生成器接口
/// 用于将 RuleSet 自动转换为 xUnit 测试代码
///
/// 职责：
/// - 接收 ArchitectureRuleSet 作为输入
/// - 生成符合命名规范的 xUnit 测试类代码
/// - 支持 NetArchTest.Rules 断言生成
/// - 支持配置化的生成选项
///
/// 使用场景：
/// - 从 RuleSet 定义自动生成架构测试
/// - 保持测试与规则定义的同步
/// - 减少手工编写测试的工作量
/// </summary>
public interface IArchitectureTestGenerator
{
    /// <summary>
    /// 生成架构测试代码
    /// </summary>
    /// <param name="ruleSet">要生成测试的规则集</param>
    /// <param name="options">生成选项（可选）</param>
    /// <returns>生成的测试代码</returns>
    GeneratedTestCode Generate(ArchitectureRuleSet ruleSet, TestGenerationOptions? options = null);
}
