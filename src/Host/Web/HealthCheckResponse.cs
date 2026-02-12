namespace Zss.BilliardHall.Host.Web;

public class HealthCheckResponse
{
    public required string Status { get; init; }
    public DateTime Timestamp { get; init; }
}
