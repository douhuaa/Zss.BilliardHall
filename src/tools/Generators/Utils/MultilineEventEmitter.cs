using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Zss.BilliardHall.Generators.Utils;

/// <summary>
/// 自定义 YamlDotNet EventEmitter，用于处理多行字符串
/// 当序列化 string 且包含换行符时，使用 ScalarStyle.Literal（| 格式）
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
            // 如果字符串包含换行符，使用 Literal 样式（| 格式）
            if (stringValue.Contains('\n') || stringValue.Contains("\r\n"))
            {
                eventInfo = new ScalarEventInfo(eventInfo.Source)
                {
                    Style = ScalarStyle.Literal
                };
            }
            // 如果字符串包含 YAML 特殊字符（冒号后跟空格、以冒号开头等），使用 DoubleQuoted
            else if (NeedsQuoting(stringValue))
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
    /// 判断字符串是否需要引号（避免 YAML 解析错误）
    /// </summary>
    private static bool NeedsQuoting(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

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

        return false;
    }
}
