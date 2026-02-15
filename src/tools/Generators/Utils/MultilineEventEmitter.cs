using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Zss.BilliardHall.Generators.Utils;

/// <summary>
/// 自定义 YamlDotNet EventEmitter，用于处理多行字符串和特殊字符
/// 当序列化 string 且包含换行符或特殊字符时，使用 ScalarStyle.DoubleQuoted 确保安全转义
/// </summary>
public sealed class MultilineEventEmitter : ChainedEventEmitter
{
    public MultilineEventEmitter(IEventEmitter nextEmitter)
        : base(nextEmitter)
    {
    }

    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        // 只处理字符串类型
        if (eventInfo.Source.Type == typeof(string) && eventInfo.Source.Value is string stringValue)
        {
            // 如果字符串包含换行符或需要引号的特殊字符，使用 DoubleQuoted 样式
            if (NeedsQuoting(stringValue))
            {
                eventInfo = new ScalarEventInfo(eventInfo.Source)
                {
                    Style = ScalarStyle.DoubleQuoted
                };
            }
        }

        base.Emit(eventInfo, emitter);
    }

    /// <summary>
    /// 判断字符串是否需要引号（避免 YAML 解析错误和注入攻击）
    /// </summary>
    private static bool NeedsQuoting(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // 包含换行符 - 使用引号而不是 literal block，避免注入问题
        if (value.Contains('\n') || value.Contains("\r\n"))
            return true;

        // 包含引号 - 需要使用引号样式让 YamlDotNet 自动转义
        if (value.Contains('"') || value.Contains('\''))
            return true;

        // 以冒号开头
        if (value.StartsWith(':'))
            return true;

        // 包含 ": " 模式（冒号后跟空格）
        if (value.Contains(": "))
            return true;

        // 以空格开头或结尾
        if (value.StartsWith(' ') || value.EndsWith(' '))
            return true;

        // 包含特殊字符
        if (value.Contains('`') || value.Contains('$'))
            return true;

        // 包含潜在的列表标记或键值对模式
        if (value.Contains("\n-") || value.Contains("\n  "))
            return true;

        return false;
    }
}
