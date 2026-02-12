namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Adr;

/// <summary>
/// ADR 关系图生成工具
/// 基于 ADR 关系声明生成 Markdown 格式的关系图
/// 
/// 重构说明（2026-02-09）：
/// - 分类逻辑已提取到 AdrCategoryClassifier
/// - 添加错误处理
/// - 改进职责分离
/// </summary>
public static class AdrRelationshipMapGenerator
{
    /// <summary>
    /// 生成 ADR 关系图并写入指定文件
    /// </summary>
    /// <param name="adrRootPath">ADR 文档根目录路径</param>
    /// <param name="outputFilePath">输出文件路径</param>
    /// <exception cref="ArgumentException">路径参数无效时抛出</exception>
    /// <exception cref="DirectoryNotFoundException">ADR 目录不存在时抛出</exception>
    public static void GenerateRelationshipMap(string adrRootPath, string outputFilePath)
    {
        // 参数验证
        if (string.IsNullOrWhiteSpace(adrRootPath))
        {
            throw new ArgumentException("ADR 根目录路径不能为空", nameof(adrRootPath));
        }

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException("输出文件路径不能为空", nameof(outputFilePath));
        }

        if (!Directory.Exists(adrRootPath))
        {
            throw new DirectoryNotFoundException($"ADR 根目录不存在: {adrRootPath}");
        }

        try
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

            // 使用 AdrCategoryClassifier 进行分类
            var grouped = adrs.GroupBy(a => AdrCategoryClassifier.TryGetCategory(a.Id, out var category)
                ? category
                : "其他");

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

            // 写入文件（带错误处理）
            try
            {
                var directory = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputFilePath, markdown.ToString());
            }
            catch (Exception ex)
            {
                throw new IOException($"无法写入输出文件 {outputFilePath}: {ex.Message}", ex);
            }
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not DirectoryNotFoundException && ex is not IOException)
        {
            throw new InvalidOperationException($"生成 ADR 关系图时发生错误: {ex.Message}", ex);
        }
    }
}
