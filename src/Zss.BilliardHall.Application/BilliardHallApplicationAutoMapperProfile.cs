using AutoMapper;
using Zss.BilliardHall.Entities;
using Zss.BilliardHall.Dtos;

namespace Zss.BilliardHall.Application;

/// <summary>
/// AutoMapper配置文件
/// </summary>
public class BilliardHallApplicationAutoMapperProfile : Profile
{
    public BilliardHallApplicationAutoMapperProfile()
    {
        CreateMap<BilliardTable, BilliardTableDto>();
        CreateMap<CreateBilliardTableDto, BilliardTable>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.QrCodeContent, opt => opt.Ignore())
            .ForMember(dest => dest.LastHeartbeatTime, opt => opt.Ignore());

        CreateMap<TableSession, TableSessionDto>();
        CreateMap<CreateTableSessionDto, TableSession>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.Ignore())
            .ForMember(dest => dest.EndTime, opt => opt.Ignore());
    }
}

/// <summary>
/// 台球会话DTO（简化版）
/// </summary>
public class TableSessionDto
{
    public Guid Id { get; set; }
    public Guid BilliardTableId { get; set; }
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int BillingMinutes { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ActualPaidAmount { get; set; }
}

/// <summary>
/// 创建台球会话DTO（简化版）
/// </summary>
public class CreateTableSessionDto
{
    public Guid BilliardTableId { get; set; }
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public decimal BaseHourlyRate { get; set; }
}