using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌应用服务接口
/// </summary>
public interface IBilliardTableAppService : IApplicationService
{
    /// <summary>
    /// 获取台球桌详情
    /// </summary>
    Task<BilliardTableDto> GetAsync(Guid id);
    
    /// <summary>
    /// 获取台球桌分页列表
    /// </summary>
    Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input);
    
    /// <summary>
    /// 创建台球桌
    /// </summary>
    Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);
    
    /// <summary>
    /// 更新台球桌
    /// </summary>
    Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input);
    
    /// <summary>
    /// 删除台球桌
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// 更改台球桌状态
    /// </summary>
    Task<BilliardTableDto> ChangeStatusAsync(Guid id, ChangeTableStatusDto input);
    
    /// <summary>
    /// 获取可用的台球桌列表
    /// </summary>
    Task<ListResultDto<BilliardTableDto>> GetAvailableTablesAsync(Guid billiardHallId);
}