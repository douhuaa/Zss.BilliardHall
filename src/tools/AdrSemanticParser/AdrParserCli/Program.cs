using Zss.BilliardHall.AdrSemanticParser;

// ADR 语义解析器 CLI 工具
Console.WriteLine("🔍 ADR 语义解析器 CLI");
Console.WriteLine();

if (args.Length == 0)
{
    ShowHelp();
    return 1;
}

var command = args[0].ToLowerInvariant();

try
{
    switch (command)
    {
        case "parse":
            return await ParseCommand(args);
        case "batch":
            return await BatchCommand(args);
        case "help":
        case "--help":
        case "-h":
            ShowHelp();
            return 0;
        default:
            Console.WriteLine($"❌ 未知命令: {command}");
            ShowHelp();
            return 1;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 错误: {ex.Message}");
    return 1;
}

static void ShowHelp()
{
    Console.WriteLine("用法:");
    Console.WriteLine("  adr-parser parse <adr-file> [output-file]  解析单个 ADR 文档");
    Console.WriteLine("  adr-parser batch <adr-dir> <output-file>   批量解析 ADR 目录");
    Console.WriteLine("  adr-parser help                             显示帮助信息");
    Console.WriteLine();
    Console.WriteLine("示例:");
    Console.WriteLine("  adr-parser parse docs/adr/ADR-0001.md");
    Console.WriteLine("  adr-parser parse docs/adr/ADR-0001.md output.json");
    Console.WriteLine("  adr-parser batch docs/adr adr-models.json");
}

static async Task<int> ParseCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("❌ 缺少参数: 需要指定 ADR 文件路径");
        return 1;
    }

    var inputFile = args[1];
    var outputFile = args.Length > 2 ? args[2] : null;

    if (!File.Exists(inputFile))
    {
        Console.WriteLine($"❌ 文件不存在: {inputFile}");
        return 1;
    }

    Console.WriteLine($"📖 解析文件: {inputFile}");

    var parser = new AdrParser();
    var model = await parser.ParseFileAsync(inputFile);

    Console.WriteLine($"✅ 成功解析 ADR: {model.Id} - {model.Title}");
    Console.WriteLine($"   状态: {model.Status}");
    Console.WriteLine($"   级别: {model.Level}");
    Console.WriteLine($"   依赖: {model.Relationships.DependsOn.Count} 个");
    Console.WriteLine($"   被依赖: {model.Relationships.DependedBy.Count} 个");

    if (!string.IsNullOrWhiteSpace(outputFile))
    {
        var serializer = new AdrSerializer();
        await serializer.SerializeToFileAsync(model, outputFile);
        Console.WriteLine($"💾 已保存到: {outputFile}");
    }
    else
    {
        var serializer = new AdrSerializer();
        var json = serializer.Serialize(model);
        Console.WriteLine();
        Console.WriteLine("📄 JSON 输出:");
        Console.WriteLine(json);
    }

    return 0;
}

static async Task<int> BatchCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("❌ 缺少参数: 需要指定 ADR 目录和输出文件");
        return 1;
    }

    var inputDir = args[1];
    var outputFile = args[2];

    if (!Directory.Exists(inputDir))
    {
        Console.WriteLine($"❌ 目录不存在: {inputDir}");
        return 1;
    }

    Console.WriteLine($"📂 扫描目录: {inputDir}");

    var excludedNames = new[] { "README", "RELATIONSHIP-MAP" };
    var adrFiles = Directory.GetFiles(inputDir, "ADR-*.md", SearchOption.AllDirectories)
        .Where(f =>
        {
            var fileName = Path.GetFileNameWithoutExtension(f);
            return !excludedNames.Any(excluded => fileName.Contains(excluded, StringComparison.OrdinalIgnoreCase))
                && !f.Contains("/proposals/", StringComparison.OrdinalIgnoreCase)
                && !f.Contains("\\proposals\\", StringComparison.OrdinalIgnoreCase);
        })
        .ToList();

    Console.WriteLine($"   找到 {adrFiles.Count} 个 ADR 文件");

    var parser = new AdrParser();
    var models = new List<Zss.BilliardHall.AdrSemanticParser.Models.AdrSemanticModel>();

    foreach (var file in adrFiles)
    {
        try
        {
            var model = await parser.ParseFileAsync(file);
            models.Add(model);
            Console.WriteLine($"✅ {model.Id} - {model.Title}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  跳过文件 {Path.GetFileName(file)}: {ex.Message}");
        }
    }

    var serializer = new AdrSerializer();
    var json = serializer.SerializeBatch(models);
    await File.WriteAllTextAsync(outputFile, json);

    Console.WriteLine();
    Console.WriteLine($"✅ 成功解析 {models.Count} 个 ADR");
    Console.WriteLine($"💾 已保存到: {outputFile}");

    return 0;
}
