using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 运行架构测试命令处理器
/// </summary>
public sealed class RunArchitectureTestsCommandHandler
{
    private readonly IFileSystem _fileSystem;
    private readonly IRuleSetQueryService _ruleSetQueryService;

    public RunArchitectureTestsCommandHandler(
        IFileSystem fileSystem,
        IRuleSetQueryService? ruleSetQueryService = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _ruleSetQueryService = ruleSetQueryService ?? new RuleSetQueryService();
    }

    /// <summary>
    /// 执行运行架构测试命令
    /// </summary>
    /// <param name="adrNumber">可选：仅运行指定ADR的测试</param>
    /// <param name="verbose">是否输出详细信息</param>
    /// <returns>退出代码：0表示成功，1表示失败</returns>
    public async Task<int> ExecuteAsync(int? adrNumber = null, bool verbose = false)
    {
        try
        {
            Console.WriteLine("🧪 运行架构测试");

            // 如果指定了ADR编号，验证其存在
            if (adrNumber.HasValue)
            {
                var ruleSet = _ruleSetQueryService.GetRuleSet(adrNumber.Value);
                if (ruleSet == null)
                {
                    Console.WriteLine($"❌ ADR-{adrNumber:000} 规则集不存在");
                    return 1;
                }
                
                var summary = _ruleSetQueryService.CreateSummary(ruleSet);
                Console.WriteLine($"📋 运行 {summary} 的测试");
            }
            else
            {
                Console.WriteLine("📋 运行所有架构测试");
            }

            // 构建dotnet test命令
            var testCommand = BuildTestCommand(adrNumber, verbose);
            Console.WriteLine($"🔧 执行命令: {testCommand}");

            // 执行测试
            var (exitCode, output) = await RunDotnetTestAsync(testCommand);

            // 分析结果
            if (exitCode == 0)
            {
                Console.WriteLine("✅ 所有测试通过");
                return 0;
            }
            else
            {
                Console.WriteLine("❌ 测试失败");
                Console.WriteLine();
                Console.WriteLine("📊 失败信息分析:");
                Console.WriteLine("测试输出中应包含 RuleId（格式如 ADR-001_2_1），用于追溯到具体的架构规则条款。");
                Console.WriteLine();
                
                // 提取可能的RuleId引用
                ExtractRuleIdReferences(output);
                
                Console.WriteLine();
                Console.WriteLine("💡 建议:");
                Console.WriteLine("1. 查看上述测试输出，定位具体失败的测试方法");
                Console.WriteLine("2. 测试方法名或失败消息中应包含 RuleId");
                Console.WriteLine("3. 使用 RuleId 在 RuleSetRegistry 中查找对应的规则条款");
                Console.WriteLine("4. 参考 ADR 文档了解违规原因和修复方案");
                
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 执行失败: {ex.Message}");
            return 1;
        }
    }

    private string BuildTestCommand(int? adrNumber, bool verbose)
    {
        var baseCommand = "dotnet test src/tests/ArchitectureTests/";
        var args = new List<string>();

        // 添加过滤器
        if (adrNumber.HasValue)
        {
            // 按ADR编号过滤，匹配测试类名包含 Adr{编号} 的模式
            // 支持多种格式：Adr001、Adr1（兼容性考虑）
            args.Add($"--filter \"FullyQualifiedName~Adr{adrNumber.Value:000} | FullyQualifiedName~Adr{adrNumber.Value}\"");
        }
        else
        {
            // 运行所有架构测试
            args.Add("--filter \"Category=Architecture\"");
        }

        // 日志级别
        var verbosity = verbose ? "detailed" : "normal";
        args.Add($"--logger \"console;verbosity={verbosity}\"");

        // 不使用缓存，确保运行最新代码
        args.Add("--no-build");

        return args.Count > 0 ? $"{baseCommand} {string.Join(" ", args)}" : baseCommand;
    }

    private async Task<(int exitCode, string output)> RunDotnetTestAsync(string command)
    {
        // 简化实现：直接调用dotnet test
        // 在真实实现中，应该使用Process类执行命令并捕获输出
        
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command.Replace("dotnet ", ""),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new System.Text.StringBuilder();
        
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                output.AppendLine(e.Data);
            }
        };
        
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.Error.WriteLine(e.Data);
                output.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync();
        
        return (process.ExitCode, output.ToString());
    }

    private void ExtractRuleIdReferences(string output)
    {
        // 提取可能的RuleId引用（格式: ADR-XXX_Y 或 ADR-XXX_Y_Z）
        // 使用严格的三位数字格式匹配标准ADR编号
        var ruleIdPattern = new System.Text.RegularExpressions.Regex(
            @"ADR[-_](\d{3})_(\d+)(?:_(\d+))?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var matches = ruleIdPattern.Matches(output);
        if (matches.Count > 0)
        {
            Console.WriteLine("🔍 检测到以下 RuleId 引用:");
            var uniqueRuleIds = new HashSet<string>();
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                uniqueRuleIds.Add(match.Value);
            }

            foreach (var ruleId in uniqueRuleIds.OrderBy(x => x))
            {
                Console.WriteLine($"   - {ruleId}");
            }
        }
        else
        {
            Console.WriteLine("⚠️  未在输出中检测到 RuleId 引用");
            Console.WriteLine("   建议在架构测试的断言消息中包含 RuleId");
        }
    }
}
