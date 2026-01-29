using System.Text;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR 关系图生成工具
/// 基于 ADR 关系声明生成 Markdown 格式的关系图
/// </summary>
public static class AdrRelationshipMapGenerator
{
    /// <summary>
    /// 生成 ADR 关系图并写入指定文件
    /// </summary>
    public static void GenerateRelationshipMap(string adrRootPath, string outputFilePath)
    {
        var repo = new AdrRepository(adrRootPath);
        var adrs = repo.LoadAll().OrderBy(a => a.Id).ToList();

        var markdown = new StringBuilder();
        
        // 文件头
        markdown.AppendLine("# ADR 关系图");
        markdown.AppendLine();
        markdown.AppendLine("> **自动生成** - 请勿手动编辑");
        markdown.AppendLine("> 生成时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        markdown.AppendLine();
        markdown.AppendLine("本文档展示所有 ADR 之间的关系声明。");
        markdown.AppendLine();
        
        // 统计信息
        markdown.AppendLine("## 统计");
        markdown.AppendLine();
        markdown.AppendLine($"- **总 ADR 数**: {adrs.Count}");
        markdown.AppendLine($"- **依赖关系数**: {adrs.Sum(a => a.DependsOn.Count)}");
        markdown.AppendLine($"- **替代关系数**: {adrs.Sum(a => a.Supersedes.Count)}");
        markdown.AppendLine();

        // 按分类列出 ADR
        markdown.AppendLine("## ADR 列表");
        markdown.AppendLine();
        
        var grouped = adrs.GroupBy(a => GetAdrCategory(a.Id));
        
        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            markdown.AppendLine($"### {group.Key}");
            markdown.AppendLine();
            
            foreach (var adr in group)
            {
                markdown.AppendLine($"#### {adr.Id}");
                markdown.AppendLine();
                markdown.AppendLine($"**文件**: `{Path.GetFileName(adr.FilePath)}`");
                markdown.AppendLine();

                if (adr.DependsOn.Any())
                {
                    markdown.AppendLine("**依赖（Depends On）**:");
                    foreach (var dep in adr.DependsOn.OrderBy(d => d))
                    {
                        markdown.AppendLine($"- {dep}");
                    }
                    markdown.AppendLine();
                }

                if (adr.DependedBy.Any())
                {
                    markdown.AppendLine("**被依赖（Depended By）**:");
                    foreach (var dep in adr.DependedBy.OrderBy(d => d))
                    {
                        markdown.AppendLine($"- {dep}");
                    }
                    markdown.AppendLine();
                }

                if (adr.Supersedes.Any())
                {
                    markdown.AppendLine("**替代（Supersedes）**:");
                    foreach (var sup in adr.Supersedes.OrderBy(s => s))
                    {
                        markdown.AppendLine($"- {sup}");
                    }
                    markdown.AppendLine();
                }

                if (adr.SupersededBy.Any())
                {
                    markdown.AppendLine("**被替代（Superseded By）**:");
                    foreach (var sup in adr.SupersededBy.OrderBy(s => s))
                    {
                        markdown.AppendLine($"- {sup}");
                    }
                    markdown.AppendLine();
                }

                if (adr.Related.Any())
                {
                    markdown.AppendLine("**相关（Related）**:");
                    foreach (var rel in adr.Related.OrderBy(r => r))
                    {
                        markdown.AppendLine($"- {rel}");
                    }
                    markdown.AppendLine();
                }

                markdown.AppendLine("---");
                markdown.AppendLine();
            }
        }

        File.WriteAllText(outputFilePath, markdown.ToString());
    }

    /// <summary>
    /// 根据 ADR 编号确定分类
    /// </summary>
    private static string GetAdrCategory(string adrId)
    {
        var match = System.Text.RegularExpressions.Regex.Match(adrId, @"ADR-(\d{4})");
        if (!match.Success)
            return "其他";

        var number = int.Parse(match.Groups[1].Value);
        
        return number switch
        {
            0 => "治理（Governance）",
            >= 1 and <= 9 => "宪法（Constitutional）",
            >= 10 and <= 99 => "宪法（Constitutional）",
            >= 100 and <= 199 => "结构（Structure）",
            >= 200 and <= 299 => "运行时（Runtime）",
            >= 300 and <= 399 => "技术（Technical）",
            >= 400 => "治理（Governance）",
            _ => "其他"
        };
    }
}
