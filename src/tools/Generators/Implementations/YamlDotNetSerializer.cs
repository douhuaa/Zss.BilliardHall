using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zss.BilliardHall.Generators.Interfaces;
using Zss.BilliardHall.Generators.Utils;

namespace Zss.BilliardHall.Generators.Implementations;

/// <summary>
/// 基于 YamlDotNet 的 YAML 序列化器实现
/// 使用 MultilineEventEmitter 处理多行字符串（使用 literal block | 格式）
/// 为了保持向后兼容，对输出进行轻量后处理
/// </summary>
public sealed class YamlDotNetSerializer : IYamlSerializer
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public YamlDotNetSerializer()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithEventEmitter(next => new MultilineEventEmitter(next))
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// 序列化对象为 YAML 字符串
    /// </summary>
    public string Serialize<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        var yaml = _serializer.Serialize(obj);
        yaml = NormalizeNewlines(yaml);
        yaml = PostProcessYaml(yaml);
        return yaml;
    }

    /// <summary>
    /// 反序列化 YAML 字符串为对象
    /// </summary>
    public T Deserialize<T>(string yaml) where T : class
    {
        ArgumentNullException.ThrowIfNull(yaml);
        return _deserializer.Deserialize<T>(yaml);
    }

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");

    /// <summary>
    /// 轻量后处理 YAML 输出，确保与旧格式兼容
    /// MultilineEventEmitter 已处理多行字符串，这里只需要处理一些格式细节
    /// </summary>
    private static string PostProcessYaml(string yaml)
    {
        // 处理空列表格式：instructions: [] -> instructions:
        yaml = yaml.Replace("instructions: []", "instructions:");

        // 为特定字段添加引号以保持向后兼容
        yaml = AddQuotesToField(yaml, "id");
        yaml = AddQuotesToField(yaml, "description");
        yaml = AddQuotesToField(yaml, "action");
        yaml = AddQuotesToField(yaml, "output");

        // 为列表项添加引号
        yaml = AddQuotesToListItems(yaml);

        // 为 commands 部分的值添加引号
        yaml = AddQuotesToCommands(yaml);

        return yaml;
    }

    private static string AddQuotesToField(string yaml, string fieldName)
    {
        var lines = yaml.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            // 匹配类似 "id: value" 的行，但跳过已经有引号或是多行标记的
            if (trimmed.StartsWith($"{fieldName}:") &&
                !trimmed.Contains($"{fieldName}: \"") &&
                !trimmed.EndsWith("|") &&
                !trimmed.EndsWith(">"))
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex >= 0 && colonIndex < line.Length - 1)
                {
                    var indent = line.Substring(0, line.IndexOf(fieldName));
                    var value = line.Substring(colonIndex + 1).TrimStart();

                    // 如果值不为空且不是列表/对象标记
                    if (!string.IsNullOrWhiteSpace(value) && value != "[]" && !value.StartsWith('-'))
                    {
                        lines[i] = $"{indent}{fieldName}: \"{value}\"";
                    }
                }
            }
        }
        return string.Join("\n", lines);
    }

    private static string AddQuotesToListItems(string yaml)
    {
        var lines = yaml.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            // 匹配列表项 "- value" （不包括已有引号的）
            if (trimmed.StartsWith("- ") && !trimmed.StartsWith("- \"") && !trimmed.StartsWith("- '"))
            {
                var dashIndex = line.IndexOf('-');
                if (dashIndex >= 0)
                {
                    var indent = line.Substring(0, dashIndex);
                    var value = line.Substring(dashIndex + 1).TrimStart();

                    // 不要给嵌套的键值对或子列表添加引号
                    if (!string.IsNullOrWhiteSpace(value) && !value.Contains(':'))
                    {
                        lines[i] = $"{indent}- \"{value}\"";
                    }
                }
            }
        }
        return string.Join("\n", lines);
    }

    private static string AddQuotesToCommands(string yaml)
    {
        var lines = yaml.Split('\n');
        bool inCommands = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            // 检测 commands 部分
            if (trimmed.StartsWith("commands:"))
            {
                inCommands = true;
                continue;
            }

            // 检测是否离开了 commands 部分（缩进减少）
            if (inCommands && !string.IsNullOrWhiteSpace(line) && !line.StartsWith(" "))
            {
                inCommands = false;
            }

            // 处理 commands 中的键值对
            if (inCommands && trimmed.Contains(":") && !trimmed.StartsWith("-"))
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex >= 0 && colonIndex < line.Length - 1)
                {
                    var key = line.Substring(0, colonIndex).TrimStart();
                    var value = line.Substring(colonIndex + 1).TrimStart();

                    // 为命令值添加引号
                    if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith("\""))
                    {
                        var indent = line.Substring(0, line.IndexOf(key));
                        lines[i] = $"{indent}{key}: \"{value}\"";
                    }
                }
            }
        }
        return string.Join("\n", lines);
    }
}
