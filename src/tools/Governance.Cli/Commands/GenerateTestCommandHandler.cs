using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Specification.Rules;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 生成架构测试代码
/// </summary>
public sealed class GenerateTestCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IArchitectureTestGenerator _testGenerator;
    private readonly IPathValidator _pathValidator;

    public GenerateTestCommandHandler(
        IFileSystem fileSystem,
        IArchitectureTestGenerator testGenerator,
        IPathValidator? pathValidator = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _testGenerator = testGenerator ?? throw new ArgumentNullException(nameof(testGenerator));
        _pathValidator = pathValidator ?? new RepositoryPathValidator();
    }

    public async Task<int> ExecuteAsync(string outputDirectory, int? adrNumber = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("🔧 生成架构测试代码");

            // 路径安全检查：防止路径遍历攻击
            if (!_pathValidator.IsPathSafe(outputDirectory, out var errorMessage))
            {
                Console.WriteLine($"❌ 路径安全检查失败: {errorMessage}");
                return 1;
            }

            // 确保输出目录存在
            if (!_fileSystem.DirectoryExists(outputDirectory))
            {
                Console.WriteLine($"📁 创建输出目录: {outputDirectory}");
                _fileSystem.CreateDirectory(outputDirectory);
            }

            // 获取要处理的 RuleSet
            var ruleSets = adrNumber.HasValue
                ? new List<ArchitectureRuleSet> { RuleSetRegistry.GetStrict(adrNumber.Value) }
                : RuleSetRegistry.GetAllRuleSets().ToList();

            Console.WriteLine($"📊 将生成 {ruleSets.Count} 个测试文件");

            var successCount = 0;
            var failedCount = 0;
            
            foreach (var ruleSet in ruleSets)
            {
                try
                {
                    Console.WriteLine($"\n🔨 处理 ADR-{ruleSet.AdrNumber:D3}");

                    // 生成测试代码
                    var options = new TestGenerationOptions
                    {
                        Namespace = "Zss.BilliardHall.Tests.ArchitectureTests.Generated",
                        TestClassSuffix = "_Tests",
                        IncludeComments = true,
                        IncludeExampleImplementation = true,
                        UseTheoryPattern = true,
                        IndentString = "    "
                    };

                    var generatedCode = _testGenerator.Generate(ruleSet, options);

                    // 安全的文件名生成
                    var fileName = $"{generatedCode.ClassName}.cs";
                    
                    // 验证文件名不包含危险字符
                    if (!IsFileNameSafe(fileName))
                    {
                        Console.WriteLine($"⚠️  跳过 ADR-{ruleSet.AdrNumber:D3}: 文件名包含非法字符");
                        failedCount++;
                        continue;
                    }

                    var filePath = Path.Combine(outputDirectory, fileName);
                    
                    // 再次验证完整路径
                    var fullPath = Path.GetFullPath(filePath);
                    var expectedDir = Path.GetFullPath(outputDirectory);
                    
                    // 确保 expectedDir 以目录分隔符结尾，避免路径前缀碰撞
                    // 例如：避免 /tmp/out 与 /tmp/outside 的前缀误匹配
                    if (!expectedDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        expectedDir += Path.DirectorySeparatorChar;
                    }
                    
                    if (!fullPath.StartsWith(expectedDir, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"⚠️  跳过 ADR-{ruleSet.AdrNumber:D3}: 路径验证失败");
                        failedCount++;
                        continue;
                    }

                    await _fileSystem.WriteAllTextAsync(filePath, generatedCode.SourceCode, cancellationToken);
                    Console.WriteLine($"✅ 已生成: {fileName} ({generatedCode.TestMethodCount} 个测试方法)");

                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  跳过 ADR-{ruleSet.AdrNumber:D3}: {ex.Message}");
                    failedCount++;
                }
            }

            Console.WriteLine($"\n✅ 完成！成功生成 {successCount}/{ruleSets.Count} 个文件");
            
            // 如果有失败的文件，返回部分失败状态码
            return failedCount > 0 ? 2 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 执行失败: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   内部异常: {ex.InnerException.Message}");
            }
            return 1;
        }
    }

    /// <summary>
    /// 检查文件名是否安全，防止文件名注入
    /// </summary>
    private static bool IsFileNameSafe(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // 检查是否包含路径分隔符或非法字符
        var invalidChars = Path.GetInvalidFileNameChars();
        return !fileName.Any(c => invalidChars.Contains(c) || c == '/' || c == '\\');
    }
}
