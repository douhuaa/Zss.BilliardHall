using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Interfaces;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Implementations;

/// <summary>
/// 基于 YamlDotNet 的 YAML 序列化器实现
/// </summary>
public sealed class YamlDotNetSerializer : IYamlSerializer
{
    private readonly ISerializer _serializer;

    public YamlDotNetSerializer()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
    }

    /// <summary>
    /// 序列化对象为 YAML 字符串
    /// </summary>
    public string Serialize<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        var yaml = _serializer.Serialize(obj);
        return NormalizeNewlines(yaml);
    }

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");
}
