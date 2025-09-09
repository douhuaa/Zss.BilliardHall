using System;
using Volo.Abp.Application.Dtos;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 获取台球桌列表查询参数
/// </summary>
public class GetBilliardTablesInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// 台球桌状态过滤
    /// </summary>
    public BilliardTableStatus? Status { get; set; }
    
    /// <summary>
    /// 台球桌类型过滤
    /// </summary>
    public BilliardTableType? Type { get; set; }
    
    /// <summary>
    /// 台球厅ID过滤
    /// </summary>
    public Guid? BilliardHallId { get; set; }
    
    /// <summary>
    /// 最小小时费率
    /// </summary>
    public decimal? MinHourlyRate { get; set; }
    
    /// <summary>
    /// 最大小时费率
    /// </summary>
    public decimal? MaxHourlyRate { get; set; }
}