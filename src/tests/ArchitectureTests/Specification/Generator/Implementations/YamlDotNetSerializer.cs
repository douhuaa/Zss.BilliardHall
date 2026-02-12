using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Interfaces;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Implementations;

/// <summary>
/// 基于 YamlDotNet 的 YAML 序列化器实现
/// 为了保持向后兼容和安全性，对输出进行后处理以匹配原有格式
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
        yaml = NormalizeNewlines(yaml);
        yaml = PostProcessYaml(yaml);
        return yaml;
    }

    /// <summary>
    /// 统一行尾为 LF，避免跨平台差异
    /// </summary>
    private static string NormalizeNewlines(string? input) =>
        string.IsNullOrEmpty(input)
            ? string.Empty
            : input.Replace("\r\n", "\n").Replace("\r", "\n");

    /// <summary>
    /// 后处理 YAML 输出，使其与旧格式兼容
    /// </summary>
    private static string PostProcessYaml(string yaml)
    {
        // 处理空列表格式：instructions: [] -> instructions:
        yaml = yaml.Replace("instructions: []", "instructions:");
        
        // 将YamlDotNet的多行字符串格式转换为单行转义格式
        yaml = ConvertMultilineToSingleLine(yaml);
        
        // YamlDotNet 对简单值不加引号，但为了兼容旧测试，我们需要添加引号
        // 处理常见的字段值
        yaml = AddQuotesToField(yaml, "id");
        yaml = AddQuotesToField(yaml, "description");
        yaml = AddQuotesToField(yaml, "action");
        yaml = AddQuotesToField(yaml, "output");
        
        // 处理列表项的引号
        yaml = AddQuotesToListItems(yaml);
        
        // 处理 commands 的键值对
        yaml = AddQuotesToCommands(yaml);
        
        // 转义特殊字符（在添加引号之后）
        yaml = EscapeSpecialCharactersInQuotedValues(yaml);
        
        return yaml;
    }

    /// <summary>
    /// 将 YamlDotNet 的多行格式（| 或 >）转换为单行转义格式
    /// </summary>
    private static string ConvertMultilineToSingleLine(string yaml)
    {
        var lines = yaml.Split('\n');
        var result = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // 检测多行字符串标记 | 或 >
            if (line.TrimEnd().EndsWith("|") || line.TrimEnd().EndsWith(">"))
            {
                var indent = GetIndent(line);
                var fieldPart = line.Substring(0, line.LastIndexOf(':') + 1);
                
                // 收集多行内容
                var content = new StringBuilder();
                i++; // 跳过标记行
                
                while (i < lines.Length && (string.IsNullOrWhiteSpace(lines[i]) || lines[i].StartsWith(indent + "  ")))
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var contentLine = lines[i].Trim();
                        if (content.Length > 0)
                            content.Append("\\n");
                        content.Append(contentLine);
                    }
                    i++;
                }
                i--; // 回退一行
                
                // 转义并添加引号
                var escaped = content.ToString()
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("`", "\\`")
                    .Replace("$", "\\$");
                
                result.Add($"{fieldPart} \"{escaped}\"");
            }
            else
            {
                result.Add(line);
            }
        }
        
        return string.Join("\n", result);
    }

    private static string GetIndent(string line)
    {
        int spaces = 0;
        foreach (char c in line)
        {
            if (c == ' ')
                spaces++;
            else
                break;
        }
        return new string(' ', spaces);
    }

    /// <summary>
    /// 在引号内转义特殊字符以防止命令注入
    /// </summary>
    private static string EscapeSpecialCharactersInQuotedValues(string yaml)
    {
        var lines = yaml.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // 查找引号内的内容并转义
            if (line.Contains("\""))
            {
                line = line
                    .Replace("$(", "\\$(")
                    .Replace("`", "\\`")
                    .Replace("${", "\\${");
                
                lines[i] = line;
            }
        }
        return string.Join("\n", lines);
    }

    private static string AddQuotesToField(string yaml, string fieldName)
    {
        var lines = yaml.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();
            
            // 匹配类似 "id: value" 的行
            if (trimmed.StartsWith($"{fieldName}:") && !trimmed.Contains($"{fieldName}: \""))
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
