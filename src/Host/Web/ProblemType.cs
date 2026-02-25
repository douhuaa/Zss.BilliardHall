namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 集中管理 ProblemDetails.Type 的 URI 生成策略
/// </summary>
public static class ProblemType
{
    /// <summary>RFC 9110 §15.5.1 - 400 Bad Request</summary>
    public const string Validation = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1";

    /// <summary>RFC 9110 §15.5.10 - 409 Conflict</summary>
    public const string Domain = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10";

    /// <summary>
    /// 根据 HTTP 状态码生成稳定 URL（用于 Infrastructure / Unknown 异常）
    /// </summary>
    public static string FromStatusCode(int statusCode) =>
        $"https://httpstatuses.com/{statusCode}";
}
