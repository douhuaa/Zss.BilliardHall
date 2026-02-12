namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// 代码生成辅助工具
/// 用于格式化和构建生成的代码
/// </summary>
public static class CodeGenerationHelper
{
    /// <summary>
    /// 规范化行尾为 LF（\n），避免跨平台差异
    /// </summary>
    public static string NormalizeNewlines(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// 为文本添加缩进
    /// </summary>
    /// <param name="text">要缩进的文本</param>
    /// <param name="indentLevel">缩进级别（1 = 4个空格）</param>
    /// <param name="indentString">缩进字符串（默认4个空格）</param>
    public static string Indent(string text, int indentLevel, string indentString = "    ")
    {
        if (string.IsNullOrEmpty(text) || indentLevel <= 0)
        {
            return text;
        }

        var indent = string.Concat(Enumerable.Repeat(indentString, indentLevel));
        var lines = text.Split('\n');
        
        return string.Join('\n', lines.Select(line => 
            string.IsNullOrWhiteSpace(line) ? line : indent + line));
    }

    /// <summary>
    /// 构建代码块
    /// </summary>
    /// <param name="lines">代码行</param>
    /// <returns>合并的代码块</returns>
    public static string BuildCodeBlock(params string[] lines)
    {
        return NormalizeNewlines(string.Join("\n", lines.Where(l => l != null)));
    }

    /// <summary>
    /// 构建 XML 文档注释
    /// </summary>
    /// <param name="summary">摘要内容</param>
    /// <param name="indentLevel">缩进级别</param>
    public static string BuildXmlDocComment(string summary, int indentLevel = 0)
    {
        var lines = new[]
        {
            "/// <summary>",
            $"/// {summary}",
            "/// </summary>"
        };

        return Indent(string.Join("\n", lines), indentLevel);
    }

    /// <summary>
    /// 构建 XML 文档注释（多行）
    /// </summary>
    /// <param name="summaryLines">摘要内容（多行）</param>
    /// <param name="indentLevel">缩进级别</param>
    public static string BuildXmlDocCommentMultiLine(string[] summaryLines, int indentLevel = 0)
    {
        var lines = new List<string> { "/// <summary>" };
        lines.AddRange(summaryLines.Select(line => $"/// {line}"));
        lines.Add("/// </summary>");

        return Indent(string.Join("\n", lines), indentLevel);
    }

    /// <summary>
    /// 转义 C# 字符串字面量
    /// </summary>
    public static string EscapeStringLiteral(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// 构建命名空间声明
    /// </summary>
    public static string BuildNamespaceDeclaration(string ns)
    {
        return $"namespace {ns};";
    }

    /// <summary>
    /// 构建 using 语句
    /// </summary>
    public static string BuildUsingStatements(params string[] namespaces)
    {
        return string.Join("\n", namespaces.OrderBy(ns => ns).Select(ns => $"using {ns};"));
    }
}
