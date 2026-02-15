namespace Zss.BilliardHall.Generators;

using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Specification.Language.RuleIdLanguage;

/// <summary>
/// 条款执行器工厂接口
/// 根据执行类型获取对应的执行器
/// </summary>
public interface IClauseExecutorFactory
{
    /// <summary>
    /// 根据执行类型获取执行器
    /// </summary>
    /// <param name="executionType">执行类型</param>
    /// <returns>对应的执行器</returns>
    /// <exception cref="NotSupportedException">当执行类型不支持时</exception>
    IClauseExecutor GetExecutor(ClauseExecutionType executionType);
}
