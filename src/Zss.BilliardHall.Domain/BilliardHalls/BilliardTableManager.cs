using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Zss.BilliardHall.BilliardHalls;

/// <summary>
/// 台球桌领域服务
/// </summary>
public class BilliardTableManager : DomainService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;

    public BilliardTableManager(IRepository<BilliardTable, Guid> billiardTableRepository)
    {
        _billiardTableRepository = billiardTableRepository;
    }

    /// <summary>
    /// 创建台球桌
    /// </summary>
    public async Task<BilliardTable> CreateAsync(
        int number,
        BilliardTableType type,
        decimal hourlyRate,
        float locationX = 0,
        float locationY = 0,
        string? description = null)
    {
        // 检查台球桌编号是否已存在
        await CheckNumberNotExistsAsync(number);
        
        // 验证小时费率
        CheckHourlyRateValid(hourlyRate);

        return new BilliardTable(
            GuidGenerator.Create(),
            number,
            type,
            hourlyRate,
            locationX,
            locationY,
            description
        );
    }

    /// <summary>
    /// 更改台球桌状态
    /// </summary>
    public async Task ChangeStatusAsync(BilliardTable table, BilliardTableStatus newStatus)
    {
        Check.NotNull(table, nameof(table));
        
        // 复杂业务规则验证
        await ValidateStatusChangeAsync(table, newStatus);
        
        table.ChangeStatus(newStatus);
    }

    /// <summary>
    /// 检查台球桌编号是否已存在
    /// </summary>
    private async Task CheckNumberNotExistsAsync(int number)
    {
        var exists = await _billiardTableRepository.AnyAsync(x => x.Number == number);
        if (exists)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists)
                .WithData("Number", number);
        }
    }

    /// <summary>
    /// 验证小时费率
    /// </summary>
    private void CheckHourlyRateValid(decimal hourlyRate)
    {
        if (hourlyRate < BilliardTableConsts.MinHourlyRate || hourlyRate > BilliardTableConsts.MaxHourlyRate)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.InvalidHourlyRate)
                .WithData("Rate", hourlyRate);
        }
    }

    /// <summary>
    /// 验证状态变更
    /// </summary>
    private async Task ValidateStatusChangeAsync(BilliardTable table, BilliardTableStatus newStatus)
    {
        // 如果要设置为占用或预约状态，需要检查是否有冲突的预约
        if (newStatus == BilliardTableStatus.Occupied || newStatus == BilliardTableStatus.Reserved)
        {
            // 这里可以添加更复杂的预约冲突检查逻辑
            // 例如检查当前时间段是否有其他预约
        }
        
        await Task.CompletedTask;
    }
}