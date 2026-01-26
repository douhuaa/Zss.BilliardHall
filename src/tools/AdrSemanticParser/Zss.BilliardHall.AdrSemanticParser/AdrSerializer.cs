using System.Text.Json;
using System.Text.Json.Serialization;
using Zss.BilliardHall.AdrSemanticParser.Models;

namespace Zss.BilliardHall.AdrSemanticParser;

/// <summary>
/// ADR 语义模型序列化器
/// </summary>
public sealed class AdrSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// 将 ADR 语义模型序列化为 JSON 字符串
    /// </summary>
    public string Serialize(AdrSemanticModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return JsonSerializer.Serialize(model, _options);
    }

    /// <summary>
    /// 将 ADR 语义模型序列化为 JSON 并写入文件
    /// </summary>
    public async Task SerializeToFileAsync(AdrSemanticModel model, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = Serialize(model);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化为 ADR 语义模型
    /// </summary>
    public AdrSemanticModel? Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<AdrSemanticModel>(json, _options);
    }

    /// <summary>
    /// 从 JSON 文件反序列化为 ADR 语义模型
    /// </summary>
    public async Task<AdrSemanticModel?> DeserializeFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Deserialize(json);
    }

    /// <summary>
    /// 批量序列化多个 ADR 模型
    /// </summary>
    public string SerializeBatch(IEnumerable<AdrSemanticModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return JsonSerializer.Serialize(models, _options);
    }
}
