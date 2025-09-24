using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Guids;
using Zss.BilliardHall.Services;
using Zss.BilliardHall.Dtos;
using Zss.BilliardHall.Entities;

namespace Zss.BilliardHall.Application.Services;

/// <summary>
/// 台球桌应用服务实现
/// </summary>
public class BilliardTableAppService : ITransientDependency, IBilliardTableAppService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;
    private readonly IRepository<TableSession, Guid> _sessionRepository;
    private readonly ILogger<BilliardTableAppService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IGuidGenerator _guidGenerator;

    public BilliardTableAppService(
        IRepository<BilliardTable, Guid> billiardTableRepository,
        IRepository<TableSession, Guid> sessionRepository,
        ILogger<BilliardTableAppService> logger,
        IObjectMapper objectMapper,
        IGuidGenerator guidGenerator)
    {
        _billiardTableRepository = billiardTableRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
        _objectMapper = objectMapper;
        _guidGenerator = guidGenerator;
    }

    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    public virtual async Task<List<BilliardTableDto>> GetListAsync(Guid? storeId = null)
    {
        _logger.LogInformation("获取台球桌列表，门店ID: {StoreId}", storeId);

        var query = await _billiardTableRepository.GetQueryableAsync();
        
        if (storeId.HasValue)
        {
            query = query.Where(x => x.StoreId == storeId.Value);
        }

        var tables = query.ToList();
        
        return _objectMapper.Map<List<BilliardTable>, List<BilliardTableDto>>(tables);
    }

    /// <summary>
    /// 根据ID获取台球桌
    /// </summary>
    public virtual async Task<BilliardTableDto> GetAsync(Guid id)
    {
        _logger.LogInformation("获取台球桌详情，ID: {Id}", id);

        var table = await _billiardTableRepository.GetAsync(id);
        return _objectMapper.Map<BilliardTable, BilliardTableDto>(table);
    }

    /// <summary>
    /// 创建台球桌
    /// </summary>
    public virtual async Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
    {
        _logger.LogInformation("创建台球桌: {TableName}", input.TableName);

        var table = new BilliardTable(
            _guidGenerator.Create(),
            input.TableNumber,
            input.TableName,
            input.StoreId,
            input.BasePrice);

        table.Location = input.Location;
        table.DeviceId = input.DeviceId;
        table.Remarks = input.Remarks;

        await _billiardTableRepository.InsertAsync(table);
        
        _logger.LogInformation("台球桌创建成功，ID: {Id}", table.Id);

        return _objectMapper.Map<BilliardTable, BilliardTableDto>(table);
    }

    /// <summary>
    /// 更新台球桌
    /// </summary>
    public virtual async Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
    {
        _logger.LogInformation("更新台球桌，ID: {Id}", id);

        var table = await _billiardTableRepository.GetAsync(id);
        
        table.TableName = input.TableName;
        table.Location = input.Location;
        table.BasePrice = input.BasePrice;
        table.IsEnabled = input.IsEnabled;
        table.DeviceId = input.DeviceId;
        table.Remarks = input.Remarks;

        await _billiardTableRepository.UpdateAsync(table);
        
        _logger.LogInformation("台球桌更新成功，ID: {Id}", id);

        return _objectMapper.Map<BilliardTable, BilliardTableDto>(table);
    }

    /// <summary>
    /// 删除台球桌
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("删除台球桌，ID: {Id}", id);

        await _billiardTableRepository.DeleteAsync(id);
        
        _logger.LogInformation("台球桌删除成功，ID: {Id}", id);
    }

    /// <summary>
    /// 开台
    /// </summary>
    public virtual async Task<Guid> StartSessionAsync(Guid tableId, Guid userId)
    {
        _logger.LogInformation("开台请求，桌号: {TableId}, 用户: {UserId}", tableId, userId);

        var table = await _billiardTableRepository.GetAsync(tableId);
        
        // 检查台球桌状态
        table.StartSession();

        // 创建会话
        var session = new TableSession(
            _guidGenerator.Create(),
            tableId,
            userId,
            table.StoreId,
            table.BasePrice);

        await _sessionRepository.InsertAsync(session);
        await _billiardTableRepository.UpdateAsync(table);

        _logger.LogInformation("开台成功，会话ID: {SessionId}", session.Id);

        return session.Id;
    }

    /// <summary>
    /// 关台
    /// </summary>
    public virtual async Task EndSessionAsync(Guid tableId, Guid sessionId)
    {
        _logger.LogInformation("关台请求，桌号: {TableId}, 会话: {SessionId}", tableId, sessionId);

        var table = await _billiardTableRepository.GetAsync(tableId);
        var session = await _sessionRepository.GetAsync(sessionId);

        // 结束会话
        session.EndSession();
        table.EndSession();

        await _sessionRepository.UpdateAsync(session);
        await _billiardTableRepository.UpdateAsync(table);

        _logger.LogInformation("关台成功，会话ID: {SessionId}", sessionId);
    }

    /// <summary>
    /// 获取台球桌当前状态
    /// </summary>
    public virtual async Task<BilliardTableDto> GetStatusAsync(Guid tableId)
    {
        _logger.LogInformation("获取台球桌状态，ID: {TableId}", tableId);

        var table = await _billiardTableRepository.GetAsync(tableId);
        return _objectMapper.Map<BilliardTable, BilliardTableDto>(table);
    }

    /// <summary>
    /// 更新设备心跳
    /// </summary>
    public virtual async Task UpdateHeartbeatAsync(Guid tableId, string deviceId)
    {
        _logger.LogInformation("更新设备心跳，桌号: {TableId}, 设备: {DeviceId}", tableId, deviceId);

        var table = await _billiardTableRepository.GetAsync(tableId);
        table.UpdateHeartbeat();

        await _billiardTableRepository.UpdateAsync(table);
        
        _logger.LogDebug("设备心跳更新成功，桌号: {TableId}", tableId);
    }
}