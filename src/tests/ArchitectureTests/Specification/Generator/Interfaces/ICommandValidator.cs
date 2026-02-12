namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Interfaces;

/// <summary>
/// 命令验证器接口
/// 负责验证测试命令的有效性
/// </summary>
public interface ICommandValidator
{
    /// <summary>
    /// 验证命令字符串是否有效
    /// </summary>
    /// <param name="command">命令字符串</param>
    /// <returns>如果命令有效返回 true，否则返回 false</returns>
    bool IsValidCommand(string command);

    /// <summary>
    /// 清理命令字符串（移除危险字符）
    /// </summary>
    /// <param name="command">原始命令</param>
    /// <returns>清理后的命令</returns>
    string SanitizeCommand(string command);
}
