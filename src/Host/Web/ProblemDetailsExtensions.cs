using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 为 ProblemDetails.Extensions 添加追踪信息
/// </summary>
internal static class ProblemDetailsExtensions
{
    /// <summary>
    /// 向 ProblemDetails.Extensions 添加 traceId 和 requestId。
    /// traceId 优先使用 Activity.Current?.Id，否则回退到 HttpContext.TraceIdentifier。
    /// </summary>
    public static void AddTraceInfo(this ProblemDetails problem, HttpContext context)
    {
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        problem.Extensions["requestId"] = context.TraceIdentifier;
    }
}
