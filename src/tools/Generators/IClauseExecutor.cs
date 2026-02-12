namespace Zss.BilliardHall.Generators;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Language.RuleIdLanguage;

/// <summary>
/// 条款执行器接口
/// 使用策略模式为不同的 ClauseExecutionType 生成断言代码片段
/// </summary>
public interface IClauseExecutor
{
    /// <summary>
    /// 获取此执行器支持的执行类型
    /// </summary>
    ClauseExecutionType SupportedType { get; }

    /// <summary>
    /// 生成断言代码片段
    /// </summary>
    /// <param name="clause">条款定义</param>
    /// <param name="indent">缩进字符串</param>
    /// <returns>生成的断言代码</returns>
    string GenerateAssertionCode(ArchitectureClauseDefinition clause, string indent);
}
