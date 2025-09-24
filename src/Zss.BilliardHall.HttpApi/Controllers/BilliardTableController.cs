using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Zss.BilliardHall.Services;
using Zss.BilliardHall.Dtos;

namespace Zss.BilliardHall.Controllers;

/// <summary>
/// 台球桌管理控制器
/// </summary>
[Area("BilliardHall")]
[Route("api/billiard-tables")]
[ApiController]
public class BilliardTableController : AbpControllerBase, IBilliardTableAppService
{
    private readonly IBilliardTableAppService _billiardTableAppService;

    public BilliardTableController(IBilliardTableAppService billiardTableAppService)
    {
        _billiardTableAppService = billiardTableAppService;
    }

    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    /// <param name="storeId">门店ID</param>
    /// <returns>台球桌列表</returns>
    [HttpGet]
    public virtual Task<List<BilliardTableDto>> GetListAsync(Guid? storeId = null)
    {
        return _billiardTableAppService.GetListAsync(storeId);
    }

    /// <summary>
    /// 根据ID获取台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <returns>台球桌信息</returns>
    [HttpGet("{id}")]
    public virtual Task<BilliardTableDto> GetAsync(Guid id)
    {
        return _billiardTableAppService.GetAsync(id);
    }

    /// <summary>
    /// 创建台球桌
    /// </summary>
    /// <param name="input">创建参数</param>
    /// <returns>创建的台球桌信息</returns>
    [HttpPost]
    public virtual Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
    {
        return _billiardTableAppService.CreateAsync(input);
    }

    /// <summary>
    /// 更新台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <param name="input">更新参数</param>
    /// <returns>更新的台球桌信息</returns>
    [HttpPut("{id}")]
    public virtual Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
    {
        return _billiardTableAppService.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _billiardTableAppService.DeleteAsync(id);
    }

    /// <summary>
    /// 开台
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>会话ID</returns>
    [HttpPost("{tableId}/start-session")]
    public virtual Task<Guid> StartSessionAsync(Guid tableId, Guid userId)
    {
        return _billiardTableAppService.StartSessionAsync(tableId, userId);
    }

    /// <summary>
    /// 关台
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="sessionId">会话ID</param>
    [HttpPost("{tableId}/end-session")]
    public virtual Task EndSessionAsync(Guid tableId, Guid sessionId)
    {
        return _billiardTableAppService.EndSessionAsync(tableId, sessionId);
    }

    /// <summary>
    /// 获取台球桌当前状态
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <returns>台球桌状态</returns>
    [HttpGet("{tableId}/status")]
    public virtual Task<BilliardTableDto> GetStatusAsync(Guid tableId)
    {
        return _billiardTableAppService.GetStatusAsync(tableId);
    }

    /// <summary>
    /// 更新设备心跳
    /// </summary>
    /// <param name="tableId">台球桌ID</param>
    /// <param name="deviceId">设备ID</param>
    [HttpPost("{tableId}/heartbeat")]
    public virtual Task UpdateHeartbeatAsync(Guid tableId, [FromQuery] string deviceId)
    {
        return _billiardTableAppService.UpdateHeartbeatAsync(tableId, deviceId);
    }
}