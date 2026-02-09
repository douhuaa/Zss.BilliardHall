namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR 测试 Fixture
/// 提供统一的 ADR 文档加载和缓存，避免在每个测试类中重复加载
/// </summary>
public sealed class AdrTestFixture : IAsyncLifetime
{
    private AdrRepository _repository = null!;

    /// <summary>
    /// 所有已加载的 ADR 文档（按 ID 索引）
    /// </summary>
    public IReadOnlyDictionary<string, AdrDocument> AllAdrs { get; private set; }
        = new Dictionary<string, AdrDocument>();

    /// <summary>
    /// ADR 文档列表
    /// </summary>
    public IReadOnlyList<AdrDocument> AdrList { get; private set; }
        = Array.Empty<AdrDocument>();

    /// <summary>
    /// ADR 文档目录路径
    /// </summary>
    public string AdrPath => TestEnvironment.AdrPath;

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            // 验证环境
            TestEnvironment.ValidateEnvironment();

            var adrPath = TestEnvironment.AdrPath;
            Debug.WriteLine($"[AdrTestFixture] 开始加载 ADR 文档，路径: {adrPath}");

            _repository = new AdrRepository(adrPath);

            var adrs = _repository.LoadAll();

            if (adrs.Count == 0)
            {
                throw new InvalidOperationException(
                    $"未能加载任何 ADR 文档。请检查：\n" +
                    $"  1. 路径是否存在：{adrPath}\n" +
                    $"  2. 文件是否符合 ADR-*.md 格式\n" +
                    $"  3. 文件是否包含正确的元数据格式");
            }

            AdrList = adrs;
            AllAdrs = adrs.ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);

            Debug.WriteLine($"[AdrTestFixture] 成功加载 {AllAdrs.Count} 个 ADR 文档");
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// 获取指定 ID 的 ADR 文档
    /// </summary>
    public AdrDocument GetAdr(string adrId)
    {
        if (!AllAdrs.TryGetValue(adrId, out var adr))
        {
            throw new KeyNotFoundException($"未找到 ADR 文档: {adrId}");
        }
        return adr;
    }

    /// <summary>
    /// 尝试获取指定 ID 的 ADR 文档
    /// </summary>
    public bool TryGetAdr(string adrId, out AdrDocument? adr)
    {
        return AllAdrs.TryGetValue(adrId, out adr);
    }
}
