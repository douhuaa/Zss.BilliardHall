using System.CommandLine;
using System.CommandLine.Invocation;
using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Tools.Governance.Cli.Commands;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli;

/// <summary>
/// 架构治理产物落盘 CLI 主程序
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 创建根命令
        var rootCommand = new RootCommand("🏛️ 架构治理产物落盘 CLI")
        {
            Description = "根据 Specification 中的 RuleSet 生成治理产物（ADR Decision、Agent Instructions、测试代码等）并写回仓库"
        };

        // 全局选项: --dry-run
        var dryRunOption = new Option<bool>(
            aliases: new[] { "--dry-run", "-d" },
            description: "Dry-run 模式：仅输出预览，不写入文件"
        );
        rootCommand.AddGlobalOption(dryRunOption);

        // 子命令: generate
        var generateCommand = new Command("generate", "生成治理产物");
        rootCommand.Add(generateCommand);

        // 子命令: generate adr
        var generateAdrCommand = CreateGenerateAdrCommand(dryRunOption);
        generateCommand.Add(generateAdrCommand);

        // 子命令: generate agent
        var generateAgentCommand = CreateGenerateAgentCommand(dryRunOption);
        generateCommand.Add(generateAgentCommand);

        // 子命令: generate test
        var generateTestCommand = CreateGenerateTestCommand(dryRunOption);
        generateCommand.Add(generateTestCommand);

        // 子命令: validate
        var validateCommand = CreateValidateCommand();
        rootCommand.Add(validateCommand);

        // 子命令: run
        var runCommand = new Command("run", "运行测试或检查");
        rootCommand.Add(runCommand);

        // 子命令: run architecture-tests
        var runArchTestsCommand = CreateRunArchitectureTestsCommand();
        runCommand.Add(runArchTestsCommand);

        // 子命令: scan
        var scanCommand = new Command("scan", "扫描和分析");
        rootCommand.Add(scanCommand);

        // 子命令: scan cross-module-refs
        var scanCrossModuleCommand = CreateScanCrossModuleRefsCommand();
        scanCommand.Add(scanCrossModuleCommand);

        // 子命令: update
        var updateCommand = new Command("update", "更新文档和索引");
        rootCommand.Add(updateCommand);

        // 子命令: update documentation
        var updateDocsCommand = CreateUpdateDocumentationCommand(dryRunOption);
        updateCommand.Add(updateDocsCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static Command CreateGenerateAdrCommand(Option<bool> dryRunOption)
    {
        var command = new Command("adr", "生成 ADR Decision 章节并合并到现有 ADR 文档");

        var adrOption = new Option<string>(
            aliases: new[] { "--adr", "-a" },
            description: "ADR 编号或 ID（如：1 或 ADR-001）"
        ) { IsRequired = true };

        var pathOption = new Option<string>(
            aliases: new[] { "--path", "-p" },
            description: "ADR 文档文件路径"
        ) { IsRequired = true };

        command.AddOption(adrOption);
        command.AddOption(pathOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var adr = context.ParseResult.GetValueForOption(adrOption)!;
            var path = context.ParseResult.GetValueForOption(pathOption)!;
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            
            var fileSystem = CreateFileSystem(dryRun);
            var decisionGenerator = new AdrDecisionGenerator();
            var documentMerger = new AdrDocumentMerger(decisionGenerator);
            var handler = new GenerateAdrCommandHandler(fileSystem, decisionGenerator, documentMerger);

            var exitCode = await handler.ExecuteAsync(adr, path);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateGenerateAgentCommand(Option<bool> dryRunOption)
    {
        var command = new Command("agent", "生成 Agent Instructions YAML");

        var outputOption = new Option<string>(
            aliases: new[] { "--out", "-o" },
            description: "输出目录"
        ) { IsRequired = true };

        var adrOption = new Option<int?>(
            aliases: new[] { "--adr", "-a" },
            description: "可选：仅生成指定 ADR 的 Instructions（默认：全部）"
        );

        command.AddOption(outputOption);
        command.AddOption(adrOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var adr = context.ParseResult.GetValueForOption(adrOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            
            var fileSystem = CreateFileSystem(dryRun);
            var instructionGenerator = new AgentInstructionGenerator();
            var handler = new GenerateAgentCommandHandler(fileSystem, instructionGenerator);

            var exitCode = await handler.ExecuteAsync(output, adr);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateGenerateTestCommand(Option<bool> dryRunOption)
    {
        var command = new Command("test", "生成架构测试代码（C# xUnit）");

        var outputOption = new Option<string>(
            aliases: new[] { "--out", "-o" },
            description: "输出目录"
        ) { IsRequired = true };

        var adrOption = new Option<int?>(
            aliases: new[] { "--adr", "-a" },
            description: "可选：仅生成指定 ADR 的测试代码（默认：全部）"
        );

        command.AddOption(outputOption);
        command.AddOption(adrOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var adr = context.ParseResult.GetValueForOption(adrOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            
            var fileSystem = CreateFileSystem(dryRun);
            var testGenerator = new ArchitectureTestGenerator();
            var handler = new GenerateTestCommandHandler(fileSystem, testGenerator);

            var exitCode = await handler.ExecuteAsync(output, adr);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateValidateCommand()
    {
        var command = new Command("validate", "校验 RuleSetRegistry 注册完整性与 RuleId 格式");

        command.SetHandler(async (InvocationContext context) =>
        {
            var handler = new ValidateCommandHandler();
            var exitCode = await handler.ExecuteAsync();
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateRunArchitectureTestsCommand()
    {
        var command = new Command("architecture-tests", "运行架构测试并分析结果");

        var adrOption = new Option<int?>(
            aliases: new[] { "--adr", "-a" },
            description: "可选：仅运行指定 ADR 的测试（默认：全部）"
        );

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "输出详细信息"
        );

        var failFastOption = new Option<bool>(
            aliases: new[] { "--fail-fast", "-f" },
            description: "在首次失败后停止"
        );

        command.AddOption(adrOption);
        command.AddOption(verboseOption);
        command.AddOption(failFastOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var adrNumber = context.ParseResult.GetValueForOption(adrOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var failFast = context.ParseResult.GetValueForOption(failFastOption);

            var fileSystem = new RealFileSystem();
            var handler = new RunArchitectureTestsCommandHandler(fileSystem);

            var exitCode = await handler.ExecuteAsync(adrNumber, verbose, failFast);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateScanCrossModuleRefsCommand()
    {
        var command = new Command("cross-module-refs", "扫描跨模块引用");

        var sourceOption = new Option<string?>(
            aliases: new[] { "--source", "-s" },
            description: "可选：仅扫描指定源模块（默认：全部）"
        );

        var includeTestsOption = new Option<bool>(
            aliases: new[] { "--include-tests", "-t" },
            description: "包含测试代码"
        );

        var jsonOption = new Option<bool>(
            aliases: new[] { "--json", "-j" },
            description: "以 JSON 格式输出"
        );

        command.AddOption(sourceOption);
        command.AddOption(includeTestsOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var source = context.ParseResult.GetValueForOption(sourceOption);
            var includeTests = context.ParseResult.GetValueForOption(includeTestsOption);
            var outputJson = context.ParseResult.GetValueForOption(jsonOption);

            var fileSystem = new RealFileSystem();
            var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

            var exitCode = await handler.ExecuteAsync(source, includeTests, outputJson);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Command CreateUpdateDocumentationCommand(Option<bool> dryRunOption)
    {
        var command = new Command("documentation", "更新文档索引和交叉引用");

        var typeOption = new Option<string?>(
            aliases: new[] { "--type", "-t" },
            description: "可选：文档类型（adr, summary）（默认：adr）"
        );

        command.AddOption(typeOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var docType = context.ParseResult.GetValueForOption(typeOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);

            var fileSystem = CreateFileSystem(dryRun);
            var handler = new UpdateDocumentationCommandHandler(fileSystem);

            var exitCode = await handler.ExecuteAsync(docType, dryRun);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static IFileSystem CreateFileSystem(bool dryRun)
    {
        var realFileSystem = new RealFileSystem();
        return dryRun ? new DryRunFileSystem(realFileSystem) : realFileSystem;
    }
}
