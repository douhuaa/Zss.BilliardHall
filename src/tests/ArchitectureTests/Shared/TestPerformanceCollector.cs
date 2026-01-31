using System.Collections.Concurrent;
using System.Diagnostics;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 测试性能数据收集器
/// 用于收集测试执行时间，识别慢测试，并监控性能回归
/// </summary>
public sealed class TestPerformanceCollector
{
    private static readonly ConcurrentDictionary<string, List<long>> _testDurations = new();
    private static readonly object _lock = new();

    /// <summary>
    /// 记录测试执行时间（毫秒）
    /// </summary>
    /// <param name="testName">测试名称</param>
    /// <param name="durationMs">执行时间（毫秒）</param>
    public static void RecordTestDuration(string testName, long durationMs)
    {
        _testDurations.AddOrUpdate(
            testName,
            _ => new List<long> { durationMs },
            (_, list) =>
            {
                lock (_lock)
                {
                    list.Add(durationMs);
                    return list;
                }
            });
    }

    /// <summary>
    /// 获取所有测试的性能统计
    /// </summary>
    /// <returns>测试性能统计列表</returns>
    public static List<TestPerformanceStatistics> GetStatistics()
    {
        var stats = new List<TestPerformanceStatistics>();

        foreach (var kvp in _testDurations)
        {
            var durations = kvp.Value.ToList();
            if (durations.Count == 0) continue;

            stats.Add(new TestPerformanceStatistics
            {
                TestName = kvp.Key,
                ExecutionCount = durations.Count,
                MinDurationMs = durations.Min(),
                MaxDurationMs = durations.Max(),
                AverageDurationMs = (long)durations.Average(),
                MedianDurationMs = CalculateMedian(durations),
                P95DurationMs = CalculatePercentile(durations, 95)
            });
        }

        return stats.OrderByDescending(s => s.AverageDurationMs).ToList();
    }

    /// <summary>
    /// 获取慢测试（超过阈值的测试）
    /// </summary>
    /// <param name="thresholdMs">阈值（毫秒），默认 1000ms</param>
    /// <returns>慢测试列表</returns>
    public static List<TestPerformanceStatistics> GetSlowTests(long thresholdMs = 1000)
    {
        return GetStatistics()
            .Where(s => s.AverageDurationMs > thresholdMs)
            .ToList();
    }

    /// <summary>
    /// 生成性能报告（Markdown 格式）
    /// </summary>
    /// <param name="topN">显示前 N 个最慢的测试</param>
    /// <returns>Markdown 格式的性能报告</returns>
    public static string GeneratePerformanceReport(int topN = 20)
    {
        var stats = GetStatistics();
        var totalTests = stats.Count;
        var totalDuration = stats.Sum(s => s.AverageDurationMs);
        var slowTests = GetSlowTests();

        var report = new System.Text.StringBuilder();
        report.AppendLine("# 测试性能报告");
        report.AppendLine();
        report.AppendLine($"**生成时间**：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        report.AppendLine("## 📊 总体统计");
        report.AppendLine();
        report.AppendLine($"- **测试总数**：{totalTests}");
        report.AppendLine($"- **总执行时间**：{totalDuration:N0} ms");
        report.AppendLine($"- **平均执行时间**：{(totalTests > 0 ? totalDuration / totalTests : 0):N0} ms");
        report.AppendLine($"- **慢测试数量**：{slowTests.Count} (> 1000ms)");
        report.AppendLine();
        report.AppendLine($"## 🐌 前 {topN} 个最慢的测试");
        report.AppendLine();
        report.AppendLine("| 排名 | 测试名称 | 平均时间(ms) | 最小(ms) | 最大(ms) | 中位数(ms) | P95(ms) | 执行次数 |");
        report.AppendLine("|------|----------|--------------|----------|----------|------------|---------|----------|");

        var topTests = stats.Take(topN).ToList();
        for (int i = 0; i < topTests.Count; i++)
        {
            var stat = topTests[i];
            var emoji = stat.AverageDurationMs > 1000 ? "🔴" : stat.AverageDurationMs > 500 ? "🟡" : "🟢";
            report.AppendLine($"| {emoji} {i + 1} | {stat.TestName} | {stat.AverageDurationMs:N0} | {stat.MinDurationMs:N0} | {stat.MaxDurationMs:N0} | {stat.MedianDurationMs:N0} | {stat.P95DurationMs:N0} | {stat.ExecutionCount} |");
        }

        report.AppendLine();
        report.AppendLine("## 🎯 性能建议");
        report.AppendLine();

        if (slowTests.Count > 0)
        {
            report.AppendLine($"⚠️ 发现 {slowTests.Count} 个慢测试（执行时间 > 1000ms）：");
            report.AppendLine();
            foreach (var test in slowTests.Take(10))
            {
                report.AppendLine($"- **{test.TestName}**：{test.AverageDurationMs:N0} ms");
            }
            report.AppendLine();
            report.AppendLine("**建议**：");
            report.AppendLine("1. 检查是否有不必要的文件 I/O 操作");
            report.AppendLine("2. 考虑使用缓存减少重复计算");
            report.AppendLine("3. 评估是否可以使用并行执行");
        }
        else
        {
            report.AppendLine("✅ 所有测试执行时间在可接受范围内（< 1000ms）");
        }

        return report.ToString();
    }

    /// <summary>
    /// 清空所有收集的数据
    /// </summary>
    public static void Clear()
    {
        _testDurations.Clear();
    }

    /// <summary>
    /// 导出性能数据到 JSON 文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void ExportToJson(string filePath)
    {
        var stats = GetStatistics();
        var json = System.Text.Json.JsonSerializer.Serialize(stats, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    private static long CalculateMedian(List<long> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;
        if (count == 0) return 0;
        if (count % 2 == 1)
            return sorted[count / 2];
        return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
    }

    private static long CalculatePercentile(List<long> values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;
        if (count == 0) return 0;
        int index = (int)Math.Ceiling(count * percentile / 100.0) - 1;
        return sorted[Math.Max(0, Math.Min(index, count - 1))];
    }
}

/// <summary>
/// 测试性能统计数据
/// </summary>
public sealed class TestPerformanceStatistics
{
    public string TestName { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public long AverageDurationMs { get; set; }
    public long MedianDurationMs { get; set; }
    public long P95DurationMs { get; set; }
}

/// <summary>
/// 测试性能计时器（用于便捷地测量测试执行时间）
/// 使用方式：using var timer = new TestPerformanceTimer("测试名称");
/// </summary>
public sealed class TestPerformanceTimer : IDisposable
{
    private readonly string _testName;
    private readonly Stopwatch _stopwatch;

    public TestPerformanceTimer(string testName)
    {
        _testName = testName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        TestPerformanceCollector.RecordTestDuration(_testName, _stopwatch.ElapsedMilliseconds);
    }
}
