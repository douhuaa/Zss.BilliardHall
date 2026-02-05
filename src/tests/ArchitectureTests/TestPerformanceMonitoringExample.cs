namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 测试性能监控示例
/// 演示如何使用 TestPerformanceCollector 监控测试性能
///
/// 使用方式：
/// 1. 在测试开始时创建 TestPerformanceTimer
/// 2. 在测试结束后（通过 using 语句自动完成），性能数据会被记录
/// 3. 使用 TestPerformanceCollector.GetStatistics() 获取统计信息
/// 4. 使用 TestPerformanceCollector.GeneratePerformanceReport() 生成报告
/// </summary>
public sealed class TestPerformanceMonitoringExample
{
    [Fact(DisplayName = "示例：如何在测试中使用性能监控")]
    public void Example_How_To_Use_Performance_Monitoring()
    {
        // 使用 using 语句自动记录测试执行时间
        using var timer = new TestPerformanceTimer(nameof(Example_How_To_Use_Performance_Monitoring));

        // 执行你的测试逻辑
        System.Threading.Thread.Sleep(10); // 模拟测试执行

        // 测试结束时，timer.Dispose() 会自动调用，记录执行时间
    }

    [Fact(DisplayName = "示例：生成性能报告")]
    public void Example_Generate_Performance_Report()
    {
        using var timer = new TestPerformanceTimer(nameof(Example_Generate_Performance_Report));

        // 模拟多次执行测试
        for (int i = 0; i < 5; i++)
        {
            TestPerformanceCollector.RecordTestDuration($"SampleTest_{i}", 10 + i * 5);
        }

        // 获取性能统计
        var stats = TestPerformanceCollector.GetStatistics();
        stats.Should().NotBeEmpty();

        // 生成性能报告
        var report = TestPerformanceCollector.GeneratePerformanceReport(topN: 10);
        report.Should().Contain("测试性能报告");
        report.Should().Contain("总体统计");
        report.Should().Contain("最慢的测试");

        // 输出报告到控制台（可选）
        System.Console.WriteLine(report);
    }

    [Fact(DisplayName = "示例：识别慢测试")]
    public void Example_Identify_Slow_Tests()
    {
        using var timer = new TestPerformanceTimer(nameof(Example_Identify_Slow_Tests));

        // 模拟一些快速测试和慢速测试
        TestPerformanceCollector.RecordTestDuration("FastTest1", 50);
        TestPerformanceCollector.RecordTestDuration("FastTest2", 100);
        TestPerformanceCollector.RecordTestDuration("SlowTest1", 1500);
        TestPerformanceCollector.RecordTestDuration("SlowTest2", 2000);

        // 获取慢测试（默认阈值 1000ms）
        var slowTests = TestPerformanceCollector.GetSlowTests();

        // 验证识别出了慢测试
        slowTests.Count.Should().BeGreaterThanOrEqualTo(2);
        slowTests.Should().Contain(s => s.TestName == "SlowTest1");
        slowTests.Should().Contain(s => s.TestName == "SlowTest2");
    }

    [Fact(DisplayName = "示例：导出性能数据到 JSON")]
    public void Example_Export_Performance_Data()
    {
        using var timer = new TestPerformanceTimer(nameof(Example_Export_Performance_Data));

        // 记录一些测试数据
        TestPerformanceCollector.RecordTestDuration("Test1", 100);
        TestPerformanceCollector.RecordTestDuration("Test2", 200);

        // 导出到 JSON 文件
        var outputPath = Path.Combine(Path.GetTempPath(), "test-performance.json");
        TestPerformanceCollector.ExportToJson(outputPath);

        // 验证文件已创建
        File.Exists(outputPath).Should().BeTrue();

        // 清理
        File.Delete(outputPath);
    }
}
