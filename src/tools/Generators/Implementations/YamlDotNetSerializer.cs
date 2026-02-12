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
        
        return yaml;
    }
}
