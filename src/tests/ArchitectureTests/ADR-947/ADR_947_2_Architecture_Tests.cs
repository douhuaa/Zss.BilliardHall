namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_947;

using Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-947_2: 关系区边界即标题边界（Rule）
/// 验证关系声明区内容仅限于列表项，不包含说明性段落或子章节
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-947_2_1: 边界限制
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md
/// </summary>
public sealed class ADR_947_2_Architecture_Tests
{
    /// <summary>
    /// ADR-947_2_1: 边界限制
    /// 验证关系声明区仅包含依赖/被依赖/替代/被替代/相关五类列表，不包含说明性段落或子章节（§ADR-947_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-947_2_1: 关系声明区内容必须仅为列表项")]
    public void ADR_947_2_1_Relationships_Section_Must_Only_Contain_Lists()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, "docs/adr");

        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var fileName = Path.GetFileName(adrFile);
            var content = File.ReadAllText(adrFile);

            // 提取 Relationships 章节内容
            var relationshipsContent = ExtractRelationshipsSection(content);
            if (string.IsNullOrWhiteSpace(relationshipsContent))
            {
                continue; // 没有 Relationships 章节，跳过
            }

            var lines = relationshipsContent.Split('\n');
            var hasInvalidContent = false;
            var invalidLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue; // 空行允许
                }

                // 检查是否为子章节标题（### 或 ####）
                if (trimmedLine.StartsWith("###") || trimmedLine.StartsWith("####"))
                {
                    hasInvalidContent = true;
                    invalidLines.Add($"子章节: {trimmedLine}");
                    continue;
                }

                // 检查是否为有效的关系类型标题（粗体）
                if (trimmedLine.StartsWith("**"))
                {
                    var validHeaders = new[] { "**依赖", "**被依赖", "**替代", "**被替代", "**相关",
                                              "**Depends On**", "**Depended By**", "**Supersedes**",
                                              "**Superseded By**", "**Related**" };
                    var isValidHeader = validHeaders.Any(h => trimmedLine.StartsWith(h));
                    if (!isValidHeader)
                    {
                        hasInvalidContent = true;
                        invalidLines.Add($"无效粗体内容: {trimmedLine}");
                    }
                    continue;
                }

                // 检查是否为列表项（- 或 * 开头）或 ADR 链接
                var isListItem = trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* ");
                var isAdrLink = trimmedLine.StartsWith("[ADR-") || trimmedLine.Contains("ADR-");
                var isNoContent = trimmedLine == "无" || trimmedLine == "None" || trimmedLine == "-";

                if (!isListItem && !isAdrLink && !isNoContent)
                {
                    // 可能是说明性段落
                    if (trimmedLine.Length > 20 && !trimmedLine.Contains("[") && !trimmedLine.Contains("]"))
                    {
                        hasInvalidContent = true;
                        invalidLines.Add($"说明性段落: {trimmedLine.Substring(0, Math.Min(50, trimmedLine.Length))}...");
                    }
                }
            }

            if (hasInvalidContent)
            {
                violations.Add($"{fileName}: 关系声明区包含非列表内容：\n  {string.Join("\n  ", invalidLines)}");
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-947_2_1：以下 ADR 文档的关系声明区包含无效内容：\n{string.Join("\n", violations)}");
    }

    // ========== 辅助方法 ==========


    private static string ExtractRelationshipsSection(string content)
    {
        // 使用正则表达式提取 Relationships 章节内容
        // 必须是行首的 ## Relationships，不匹配代码示例中的内容
        var pattern = @"^##\s+(Relationships|关系声明).*?\n(.*?)(?=\n##\s|\z)";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        return match.Success ? match.Groups[2].Value : string.Empty;
    }
}
