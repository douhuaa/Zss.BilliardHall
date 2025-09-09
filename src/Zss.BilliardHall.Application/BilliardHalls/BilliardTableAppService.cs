using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Zss.BilliardHall.Permissions;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌应用服务实现
/// </summary>
[Authorize(BilliardHallPermissions.BilliardTables.Default)]
public class BilliardTableAppService : ApplicationService, IBilliardTableAppService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;
    private readonly BilliardTableManager _billiardTableManager;

    public BilliardTableAppService(
        IRepository<BilliardTable, Guid> billiardTableRepository,
        BilliardTableManager billiardTableManager)
    {
        _billiardTableRepository = billiardTableRepository;
        _billiardTableManager = billiardTableManager;
    }

    /// <summary>
    /// 获取台球桌详情
    /// </summary>
    public virtual async Task<BilliardTableDto> GetAsync(Guid id)
    {
        var table = await _billiardTableRepository.GetAsync(id);
        return await MapToGetOutputDtoAsync(table);
    }

    /// <summary>
    /// 获取台球桌分页列表
    /// </summary>
    public virtual async Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input)
    {
        var queryable = await _billiardTableRepository.GetQueryableAsync();
        
        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), 
                     x => x.Number.ToString().Contains(input.Filter!) ||
                          (x.Description != null && x.Description.Contains(input.Filter!)))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            .WhereIf(input.BilliardHallId.HasValue, x => x.BilliardHallId == input.BilliardHallId)
            .WhereIf(input.MinHourlyRate.HasValue, x => x.HourlyRate >= input.MinHourlyRate)
            .WhereIf(input.MaxHourlyRate.HasValue, x => x.HourlyRate <= input.MaxHourlyRate);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        queryable = queryable
            .OrderBy(NormalizeSorting(input.Sorting) ?? $"{nameof(BilliardTable.Number)}")
            .PageBy(input.SkipCount, input.MaxResultCount);

        var tables = await AsyncExecuter.ToListAsync(queryable);
        
        return new PagedResultDto<BilliardTableDto>(
            totalCount,
            await MapToGetListOutputDtosAsync(tables)
        );
    }

    /// <summary>
    /// 创建台球桌
    /// </summary>
    [Authorize(BilliardHallPermissions.BilliardTables.Create)]
    public virtual async Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
    {
        var table = await _billiardTableManager.CreateAsync(
            input.Number,
            input.Type,
            input.HourlyRate,
            input.LocationX,
            input.LocationY,
            input.Description
        );

        // Set BilliardHallId from input
        table.BilliardHallId = input.BilliardHallId;

        await _billiardTableRepository.InsertAsync(table);
        
        return await MapToGetOutputDtoAsync(table);
    }

    /// <summary>
    /// 更新台球桌
    /// </summary>
    [Authorize(BilliardHallPermissions.BilliardTables.Edit)]
    public virtual async Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
    {
        var table = await _billiardTableRepository.GetAsync(id);
        
        // Update properties
        table.Type = input.Type;
        table.UpdateHourlyRate(input.HourlyRate);
        table.UpdateLocation(input.LocationX, input.LocationY);
        table.SetDescription(input.Description);
        
        table = await _billiardTableRepository.UpdateAsync(table);
        
        return await MapToGetOutputDtoAsync(table);
    }

    /// <summary>
    /// 删除台球桌
    /// </summary>
    [Authorize(BilliardHallPermissions.BilliardTables.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _billiardTableRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 更改台球桌状态
    /// </summary>
    [Authorize(BilliardHallPermissions.BilliardTables.ChangeStatus)]
    public virtual async Task<BilliardTableDto> ChangeStatusAsync(Guid id, ChangeTableStatusDto input)
    {
        var table = await _billiardTableRepository.GetAsync(id);
        
        await _billiardTableManager.ChangeStatusAsync(table, input.Status);
        
        table = await _billiardTableRepository.UpdateAsync(table);
        
        return await MapToGetOutputDtoAsync(table);
    }

    /// <summary>
    /// 获取可用的台球桌列表
    /// </summary>
    public virtual async Task<ListResultDto<BilliardTableDto>> GetAvailableTablesAsync(Guid billiardHallId)
    {
        var queryable = await _billiardTableRepository.GetQueryableAsync();
        
        var availableTables = queryable
            .Where(x => x.BilliardHallId == billiardHallId && x.Status == BilliardTableStatus.Available)
            .OrderBy(x => x.Number);

        var tables = await AsyncExecuter.ToListAsync(availableTables);
        
        return new ListResultDto<BilliardTableDto>(
            await MapToGetListOutputDtosAsync(tables)
        );
    }

    /// <summary>
    /// 映射到输出DTO
    /// </summary>
    private Task<BilliardTableDto> MapToGetOutputDtoAsync(BilliardTable table)
    {
        return Task.FromResult(ObjectMapper.Map<BilliardTable, BilliardTableDto>(table));
    }

    /// <summary>
    /// 映射到输出DTO列表
    /// </summary>
    private Task<List<BilliardTableDto>> MapToGetListOutputDtosAsync(List<BilliardTable> tables)
    {
        return Task.FromResult(ObjectMapper.Map<List<BilliardTable>, List<BilliardTableDto>>(tables));
    }

    /// <summary>
    /// 标准化排序参数
    /// </summary>
    protected virtual string? NormalizeSorting(string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return null;
        }

        if (sorting.Contains("number", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Replace("number", "Number", StringComparison.OrdinalIgnoreCase);
        }

        return sorting;
    }
}