using System.Diagnostics;
using System.Text;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 运行架构测试命令处理器
/// 执行架构测试并分析结果，确保输出包含 RuleId 可溯源信息
/// </summary>
public sealed class RunArchitectureTestsCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly string _repositoryRoot;

    public RunArchitectureTestsCommandHandler(
        IFileSystem fileSystem,
        string? repositoryRoot = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repositoryRoot = repositoryRoot ?? FindRepositoryRoot();
    }

    public async Task<int> ExecuteAsync(
        int? adrNumber = null,
        bool verbose = false,
        bool failFast = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("🧪 运行架构测试");

            // 确定测试项目路径
            var testProjectPath = Path.Combine(_repositoryRoot, "src", "tests", "ArchitectureTests", "ArchitectureTests.csproj");
            
            if (!_fileSystem.FileExists(testProjectPath))
            {
                Console.WriteLine($"❌ 未找到架构测试项目: {testProjectPath}");
                return 1;
            }

            Console.WriteLine($"📂 测试项目: {testProjectPath}");

            // 如果指定了 ADR 编号，验证其存在性
            if (adrNumber.HasValue)
            {
                Console.WriteLine($"🔍 运行 ADR-{adrNumber:D3} 的测试...");
                
                if (!RuleSetRegistry.Contains(adrNumber.Value))
                {
                    Console.WriteLine($"❌ ADR-{adrNumber:D3} 的规则集不存在");
                    Console.WriteLine($"   可用的 ADR 编号：{string.Join(", ", RuleSetRegistry.GetAllAdrNumbers())}");
                    return 1;
                }

                var ruleSet = RuleSetRegistry.GetStrict(adrNumber.Value);
                Console.WriteLine($"✅ 规则集 ADR-{ruleSet.AdrNumber:D3}");
                Console.WriteLine($"   规则数: {ruleSet.RuleCount}");
                Console.WriteLine($"   条款数: {ruleSet.ClauseCount}");
            }
            else
            {
                Console.WriteLine("🔍 运行所有架构测试...");
            }

            // 构建 dotnet test 命令
            var arguments = BuildTestArguments(testProjectPath, adrNumber, verbose, failFast);
            
            Console.WriteLine($"💻 执行命令: dotnet {arguments}");
            Console.WriteLine();

            // 执行测试
            var startTime = DateTime.UtcNow;
            var exitCode = await RunDotnetTestAsync(arguments, cancellationToken);
            var duration = DateTime.UtcNow - startTime;

            Console.WriteLine();
            Console.WriteLine($"⏱️  耗时: {duration.TotalSeconds:F2} 秒");

            // 根据退出码输出结果
            if (exitCode == 0)
            {
                Console.WriteLine("✅ 所有测试通过");
                return 0;
            }
            else
            {
                Console.WriteLine($"❌ 测试失败 (退出码: {exitCode})");
                Console.WriteLine();
                Console.WriteLine("💡 提示:");
                Console.WriteLine("   - 查看上方测试输出了解失败详情");
                Console.WriteLine("   - 失败信息应包含 RuleId（如 ADR-001_2_1）");
                Console.WriteLine("   - 使用 --verbose 获取更详细的输出");
                
                if (!adrNumber.HasValue)
                {
                    Console.WriteLine("   - 使用 --adr-number <编号> 运行特定 ADR 的测试");
                }

                return exitCode;
            }
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

    private static string BuildTestArguments(
        string testProjectPath,
        int? adrNumber,
        bool verbose,
        bool failFast)
    {
        var args = new StringBuilder();
        
        args.Append("test ");
        args.Append($"\"{testProjectPath}\" ");

        // 添加过滤器（如果指定了 ADR 编号）
        if (adrNumber.HasValue)
        {
            args.Append($"--filter \"FullyQualifiedName~ADR_{adrNumber:D3}\" ");
        }
        else
        {
            args.Append("--filter \"Category=Architecture\" ");
        }

        // 添加日志级别
        if (verbose)
        {
            args.Append("--logger \"console;verbosity=detailed\" ");
        }
        else
        {
            args.Append("--logger \"console;verbosity=normal\" ");
        }

        // 添加快速失败选项
        if (failFast)
        {
            args.Append("-- xUnit.StopOnFail=true ");
        }

        // 不显示 logo
        args.Append("--nologo ");

        return args.ToString().TrimEnd();
    }

    private static async Task<int> RunDotnetTestAsync(string arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        
        // 实时输出到控制台
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        return process.ExitCode;
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"无法找到仓库根目录。当前目录: {currentDirectory}");
    }
}
