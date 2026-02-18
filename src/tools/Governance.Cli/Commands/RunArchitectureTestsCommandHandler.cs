using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Commands;

/// <summary>
/// 运行架构测试命令处理器
/// </summary>
public sealed class RunArchitectureTestsCommandHandler
{
    private readonly IRuleSetQueryService _ruleSetQueryService;

    public RunArchitectureTestsCommandHandler(
        IRuleSetQueryService ruleSetQueryService)
    {
        _ruleSetQueryService = ruleSetQueryService ?? throw new ArgumentNullException(nameof(ruleSetQueryService));
    }

    /// <summary>
    /// 执行运行架构测试命令
    /// </summary>
    /// <param name="adrNumber">可选：仅运行指定ADR的测试</param>
    /// <param name="verbose">是否输出详细信息</param>
    /// <param name="noBuild">是否跳过编译（默认false，会先编译）</param>
    /// <returns>退出代码：0表示成功，1表示失败</returns>
    public async Task<int> ExecuteAsync(int? adrNumber = null, bool verbose = false, bool noBuild = false)
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

            // 构建dotnet test参数
            var testArguments = BuildTestArguments(adrNumber, verbose, noBuild);
            Console.WriteLine("🔧 执行命令:");
            Console.WriteLine($"dotnet {testArguments}");

            // 执行测试
            var (exitCode, output) = await RunDotnetTestAsync(testArguments);

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
                Console.WriteLine("测试输出中应包含 RuleId（格式如 ADR-001.2.1），用于追溯到具体的架构规则条款。");
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

    private string BuildTestArguments(int? adrNumber, bool verbose, bool noBuild)
    {
        // 注意：命令行参数中的引号处理在不同操作系统上可能有差异
        // 当前实现适用于 Windows/Linux/macOS 的标准 shell 环境
        // Process.Start 会自动处理大部分跨平台差异
        var args = new List<string> { "test", "src/tests/ArchitectureTests/" };

        // 添加过滤器
        if (adrNumber.HasValue)
        {
            // 按ADR编号过滤，使用标准的三位数字格式
            args.Add($"--filter \"FullyQualifiedName~Adr{adrNumber.Value:000}\"");
        }
        else
        {
            // 运行所有架构测试
            args.Add("--filter \"Category=Architecture\"");
        }

        // 日志级别
        var verbosity = verbose ? "detailed" : "normal";
        args.Add($"--logger \"console;verbosity={verbosity}\"");

        // 可选：跳过编译（默认会先编译以确保运行最新代码）
        if (noBuild)
        {
            args.Add("--no-build");
        }

        return string.Join(" ", args);
    }

    private async Task<(int exitCode, string output)> RunDotnetTestAsync(string arguments)
    {
        // 使用Process执行dotnet test命令
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
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
        // 提取可能的 RuleId 引用（权威格式: ADR-XXX.Y 或 ADR-XXX.Y.Z）
        // 使用严格的三位数字格式匹配标准 ADR 编号（点号 . 为权威/标准分隔符）
        // 正则中的 (?:[._]) 同时接受点号和下划线，仅用于向后兼容历史下划线格式；解析后统一转换为点号格式输出
        var ruleIdPattern = new System.Text.RegularExpressions.Regex(
            @"ADR-(\d{3})(?:[._](\d+))(?:[._](\d+))?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var matches = ruleIdPattern.Matches(output);
        if (matches.Count > 0)
        {
            Console.WriteLine("🔍 检测到以下 RuleId 引用:");
            var uniqueRuleIds = new HashSet<string>();
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // 统一转换为点号格式
                var normalized = match.Value.Replace('_', '.');
                uniqueRuleIds.Add(normalized);
            }

            foreach (var ruleId in uniqueRuleIds.OrderBy(x => x))
            {
                Console.WriteLine($"   - {ruleId}");
            }
        }
        else
        {
            Console.WriteLine("⚠️  未在输出中检测到 RuleId 引用");
            Console.WriteLine("   建议在架构测试的断言消息中包含 RuleId（格式如 ADR-001.2.1）");
        }
    }
}
