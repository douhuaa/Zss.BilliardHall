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

        // 子命令: validate
        var validateCommand = CreateValidateCommand();
        rootCommand.Add(validateCommand);

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

    private static IFileSystem CreateFileSystem(bool dryRun)
    {
        var realFileSystem = new RealFileSystem();
        return dryRun ? new DryRunFileSystem(realFileSystem) : realFileSystem;
    }
}
