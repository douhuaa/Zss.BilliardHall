using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0008: 文档编写与维护宪法
/// 验证所有文档符合 ADR-0008 的分级和边界约束
/// 
/// 【测试覆盖映射】
/// ├─ ADR-0008.1: 文档分级约束 → Documents_Must_Declare_Authority_Status
/// ├─ ADR-0008.2: ADR内容边界 → ADR_Documents_Structure_Is_Valid
/// └─ ADR-0008.3: 非ADR文档约束 → Non_ADR_Documents_Declare_Authority_Basis
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md
/// - Prompts: docs/copilot/adr-0008.prompts.md
/// </summary>
public sealed class ADR_0008_Architecture_Tests
{
    [Fact(DisplayName = "ADR-0008.1: 文档治理宪法已定义")]
    public void ADR_0008_Document_Governance_Constitution_Exists()
    {
        // 验证 ADR-0008 文档存在
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md");
        
        Assert.True(File.Exists(adrFile), $"ADR-0008 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证必需章节存在
        Assert.Contains("文档分级", content);
        Assert.Contains("文档允许表达的内容边界", content);
        Assert.Contains("非 ADR 文档的强制约束", content);
        Assert.Contains("文档结构与格式规范", content);
        Assert.Contains("文档变更治理", content);
        Assert.Contains("违规处理", content);
    }

    [Fact(DisplayName = "ADR-0008.2: 对应的 Copilot Prompts 文件存在")]
    public void ADR_0008_Prompts_File_Exists()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var promptsFile = Path.Combine(repoRoot, "docs/copilot/adr-0008.prompts.md");
        
        Assert.True(File.Exists(promptsFile), $"ADR-0008 Prompts 文件不存在：{promptsFile}");
        
        var content = File.ReadAllText(promptsFile);
        
        // 验证 Prompts 文件包含权威声明
        Assert.Contains("权威声明", content);
        Assert.Contains("不具备裁决权", content);
        Assert.Contains("ADR-0008", content);
    }

    [Fact(DisplayName = "ADR-0008.3: 核心治理原则已定义")]
    public void Core_Governance_Principles_Are_Defined()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/constitutional/ADR-0008-documentation-governance-constitution.md");
        var content = File.ReadAllText(adrFile);
        
        // 验证核心原则：只有 ADR 具备裁决力
        Assert.Contains("只有 ADR 具备裁决力", content);
        
        // 验证防引用扩权规则
        Assert.Contains("任何非 ADR 文档，即使全文逐字引用 ADR，也不具备裁决力", content);
        
        // 验证文档优先级
        Assert.Contains("ADR 正文 > Instructions > Agents > Skills > Prompts > README", content);
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}
