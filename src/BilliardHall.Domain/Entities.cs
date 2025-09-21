namespace BilliardHall.Domain;

// Core entities matching schema.sql
public class Store
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public int Status { get; set; } = 1;
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class BilliardTable
{
    public long Id { get; set; }
    public long StoreId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Status { get; set; } = 0; // 0=空闲 1=使用中 2=维修
    public long? LastSessionId { get; set; }
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class User
{
    public long Id { get; set; }
    public string? Phone { get; set; }
    public string? Nickname { get; set; }
    public int Level { get; set; } = 0;
    public string? Source { get; set; }
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TableSession
{
    public long Id { get; set; }
    public string SessionNo { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public long TableId { get; set; }
    public long UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Status { get; set; } = 0; // 0=进行中 1=已结束 2=已取消
    public int? TotalMinutes { get; set; }
    public long? BillingAmount { get; set; }
    public int PayStatus { get; set; } = 0; // 0=未支付 1=已支付
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class BillingSnapshot
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public int OriginalMinutes { get; set; }
    public long UnitPriceCents { get; set; }
    public string? RuleApplied { get; set; }
    public string? DetailBreakdown { get; set; }
    public long FinalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PaymentOrder
{
    public long Id { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public long SessionId { get; set; }
    public long UserId { get; set; }
    public long Amount { get; set; }
    public string Channel { get; set; } = string.Empty;
    public int Status { get; set; } = 0; // 0=待支付 1=成功 2=失败 3=取消
    public string? RequestPayload { get; set; }
    public string? NotifyPayload { get; set; }
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Device
{
    public long Id { get; set; }
    public long StoreId { get; set; }
    public long? TableId { get; set; }
    public int Type { get; set; }
    public string Sn { get; set; } = string.Empty;
    public string? FirmwareVersion { get; set; }
    public int Status { get; set; } = 1;
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class DeviceHeartbeat
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public DateTime Ts { get; set; }
    public string? MetricsJson { get; set; }
    public int Online { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EventLog
{
    public long Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BizId { get; set; }
    public long? UserId { get; set; }
    public long? SessionId { get; set; }
    public string? Payload { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PricingRule
{
    public long Id { get; set; }
    public long? StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RuleType { get; set; } = 1;
    public long PricePerMinute { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public int Status { get; set; } = 1;
    public int IsDeleted { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}