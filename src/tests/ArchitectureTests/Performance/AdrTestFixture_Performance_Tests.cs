namespace Zss.BilliardHall.Tests.ArchitectureTests.Performance;

/// <summary>
/// AdrTestFixture 性能测试
/// 对比使用 AdrTestFixture 前后的性能差异
/// </summary>
public sealed class AdrTestFixture_Performance_Tests : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _fixture;

    public AdrTestFixture_Performance_Tests(AdrTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 性能对比：直接加载 vs 使用 Fixture
    /// </summary>
    [Fact(DisplayName = "性能对比：AdrTestFixture 缓存效果")]
    public void Performance_Comparison_AdrTestFixture_Caching()
    {
        var iterations = 10;
        var directLoadTimes = new List<long>();
        var fixtureLoadTimes = new List<long>();

        // 测试直接加载（每次都重新加载）
        for (int i = 0; i < iterations; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var repo = new AdrRepository(TestEnvironment.AdrPath);
            var adrs = repo.LoadAll();
            sw.Stop();
            directLoadTimes.Add(sw.ElapsedMilliseconds);
        }

        // 测试使用 Fixture（使用缓存）
        for (int i = 0; i < iterations; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var adrs = _fixture.AllAdrs;
            var adrList = _fixture.AdrList;
            sw.Stop();
            fixtureLoadTimes.Add(sw.ElapsedMilliseconds);
        }

        // 计算平均时间
        var avgDirectLoad = directLoadTimes.Average();
        var avgFixtureLoad = fixtureLoadTimes.Average();
        var improvement = ((avgDirectLoad - avgFixtureLoad) / avgDirectLoad) * 100;

        // 输出结果
        var report = $"""
            
            ============================================
            AdrTestFixture 性能对比报告
            ============================================
            
            测试迭代次数: {iterations}
            
            直接加载（每次重新加载）:
              平均时间: {avgDirectLoad:F2} ms
              最快: {directLoadTimes.Min()} ms
              最慢: {directLoadTimes.Max()} ms
            
            使用 Fixture（缓存）:
              平均时间: {avgFixtureLoad:F2} ms
              最快: {fixtureLoadTimes.Min()} ms
              最慢: {fixtureLoadTimes.Max()} ms
            
            性能提升: {improvement:F2}%
            
            结论: 使用 AdrTestFixture 比直接加载快 {improvement:F0}%
            ============================================
            
            """;

        System.Console.WriteLine(report);

        // 验证 Fixture 确实更快
        avgFixtureLoad.Should().BeLessThan(avgDirectLoad,
            because: "使用缓存的 Fixture 应该比每次重新加载更快");

        // 验证至少有 50% 的性能提升
        improvement.Should().BeGreaterThan(50,
            because: "AdrTestFixture 应该提供显著的性能提升（预期 >50%）");
    }

    /// <summary>
    /// 验证 Fixture 数据的完整性
    /// </summary>
    [Fact(DisplayName = "验证 AdrTestFixture 数据完整性")]
    public void Verify_AdrTestFixture_Data_Integrity()
    {
        // 验证缓存的数据与直接加载的数据一致
        var repo = new AdrRepository(TestEnvironment.AdrPath);
        var directLoadAdrs = repo.LoadAll();

        // 验证数量一致
        _fixture.AdrList.Count.Should().Be(directLoadAdrs.Count,
            because: "Fixture 中的 ADR 数量应该与直接加载的一致");

        _fixture.AllAdrs.Count.Should().Be(directLoadAdrs.Count,
            because: "Fixture 字典中的 ADR 数量应该与直接加载的一致");

        // 验证每个 ADR 都存在
        foreach (var adr in directLoadAdrs)
        {
            _fixture.AllAdrs.Should().ContainKey(adr.Id,
                because: $"Fixture 应该包含 ADR {adr.Id}");

            var fixtureAdr = _fixture.GetAdr(adr.Id);
            fixtureAdr.Id.Should().Be(adr.Id);
            fixtureAdr.FilePath.Should().Be(adr.FilePath);
            fixtureAdr.Status.Should().Be(adr.Status);
        }
    }

    /// <summary>
    /// 模拟真实测试场景的性能
    /// </summary>
    [Fact(DisplayName = "模拟真实测试场景性能")]
    public void Simulate_Real_World_Test_Performance()
    {
        var testCount = 5; // 模拟 5 个测试方法
        var totalTime = 0L;

        for (int i = 0; i < testCount; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // 模拟测试逻辑：访问 ADR 数据
            var adrs = _fixture.AdrList;
            var hasAdrs = adrs.Count > 0;
            hasAdrs.Should().BeTrue();

            // 模拟一些 ADR 查找操作
            var adr940 = _fixture.AllAdrs.Values.FirstOrDefault(a => a.Id.StartsWith("ADR-940"));
            adr940.Should().NotBeNull();

            sw.Stop();
            totalTime += sw.ElapsedMilliseconds;
        }

        var avgTime = totalTime / (double)testCount;
        
        System.Console.WriteLine($"\n模拟 {testCount} 个测试的平均执行时间: {avgTime:F2} ms");
        
        // 验证性能合理（每个测试应该在 1ms 内完成，因为使用了缓存）
        avgTime.Should().BeLessThan(5,
            because: "使用 Fixture 缓存后，每个测试应该非常快（< 5ms）");
    }
}
