using Volo.Abp.Application.Services;
using Zss.BilliardHall.Dtos;

namespace Zss.BilliardHall.Services;

/// <summary>
/// 台球桌服务接口
/// </summary>
public interface IBilliardTableAppService : IApplicationService
{
    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    /// <param name="storeId">门店ID</param>
    /// <returns></returns>
    Task<List<BilliardTableDto>> GetListAsync(Guid? storeId = null);

    /// <summary>
    /// 根据ID获取台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <returns></returns>
    Task<BilliardTableDto> GetAsync(Guid id);

    /// <summary>
    /// 创建台球桌
    /// </summary>
    /// <param name="input">创建输入</param>
    /// <returns></returns>
    Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);

    /// <summary>
    /// 更新台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <param name="input">更新输入</param>
    /// <returns></returns>
    Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input);

    /// <summary>
    /// 删除台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <returns></returns>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 开台
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>会话ID</returns>
    Task<Guid> StartSessionAsync(Guid tableId, Guid userId);

    /// <summary>
    /// 关台
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <returns></returns>
    Task EndSessionAsync(Guid tableId, Guid sessionId);

    /// <summary>
    /// 获取台球桌当前状态
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <returns></returns>
    Task<BilliardTableDto> GetStatusAsync(Guid tableId);

    /// <summary>
    /// 更新设备心跳
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="deviceId">设备ID</param>
    /// <returns></returns>
    Task UpdateHeartbeatAsync(Guid tableId, string deviceId);
}