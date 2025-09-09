using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Zss.BilliardHall.BilliardHalls;

namespace Zss.BilliardHall.Reservations;

/// <summary>
/// 预约领域服务
/// </summary>
public class ReservationManager : DomainService
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;

    public ReservationManager(
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<BilliardTable, Guid> billiardTableRepository)
    {
        _reservationRepository = reservationRepository;
        _billiardTableRepository = billiardTableRepository;
    }

    /// <summary>
    /// 创建预约
    /// </summary>
    public async Task<Reservation> CreateAsync(
        Guid customerId,
        Guid billiardTableId,
        DateTime startTime,
        int durationMinutes,
        string? notes = null)
    {
        // 检查台球桌是否存在且可用
        var table = await _billiardTableRepository.GetAsync(billiardTableId);
        if (!table.IsAvailable())
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.TableNotAvailable)
                .WithData("TableId", billiardTableId)
                .WithData("Status", table.Status.ToString());
        }

        // 检查时间段冲突
        await CheckTimeSlotConflictAsync(billiardTableId, startTime, durationMinutes);

        // 计算预约金额
        var amount = CalculateAmount(table.HourlyRate, durationMinutes);

        // 生成预约编号
        var reservationNumber = await GenerateReservationNumberAsync();

        return new Reservation(
            GuidGenerator.Create(),
            reservationNumber,
            customerId,
            billiardTableId,
            startTime,
            durationMinutes,
            amount,
            notes
        );
    }

    /// <summary>
    /// 检查时间段冲突
    /// </summary>
    public async Task CheckTimeSlotConflictAsync(Guid billiardTableId, DateTime startTime, int durationMinutes, Guid? excludeReservationId = null)
    {
        var endTime = startTime.AddMinutes(durationMinutes);
        
        var query = await _reservationRepository.GetQueryableAsync();
        
        var conflictingReservations = query.Where(r => 
            r.BilliardTableId == billiardTableId &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Completed &&
            (excludeReservationId == null || r.Id != excludeReservationId) &&
            ((r.StartTime < endTime && r.EndTime > startTime))
        );

        if (await AsyncExecuter.AnyAsync(conflictingReservations))
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.TimeSlotConflict)
                .WithData("TableId", billiardTableId)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }
    }

    /// <summary>
    /// 更新预约时间
    /// </summary>
    public async Task UpdateReservationTimeAsync(
        Reservation reservation, 
        DateTime newStartTime, 
        int newDurationMinutes)
    {
        Check.NotNull(reservation, nameof(reservation));
        
        // 检查预约状态是否允许修改
        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotCancelReservation)
                .WithData("Status", reservation.Status.ToString());
        }

        // 检查新时间段冲突（排除当前预约）
        await CheckTimeSlotConflictAsync(reservation.BilliardTableId, newStartTime, newDurationMinutes, reservation.Id);

        // 重新计算金额
        var table = await _billiardTableRepository.GetAsync(reservation.BilliardTableId);
        var newAmount = CalculateAmount(table.HourlyRate, newDurationMinutes);

        reservation.SetReservationTime(newStartTime, newDurationMinutes);
        reservation.UpdateAmount(newAmount);
    }

    /// <summary>
    /// 自动取消过期预约
    /// </summary>
    public async Task CancelExpiredReservationsAsync()
    {
        var query = await _reservationRepository.GetQueryableAsync();
        
        var expiredReservations = query.Where(r => 
            r.Status == ReservationStatus.Confirmed &&
            r.StartTime.AddMinutes(30) < DateTime.UtcNow
        );

        var reservations = await AsyncExecuter.ToListAsync(expiredReservations);
        
        foreach (var reservation in reservations)
        {
            reservation.Cancel("系统自动取消：预约超时未使用");
        }
    }

    /// <summary>
    /// 计算预约金额
    /// </summary>
    private decimal CalculateAmount(decimal hourlyRate, int durationMinutes)
    {
        return hourlyRate * durationMinutes / 60m;
    }

    /// <summary>
    /// 生成预约编号
    /// </summary>
    private async Task<string> GenerateReservationNumberAsync()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var sequence = 1;
        
        var query = await _reservationRepository.GetQueryableAsync();
        var todayCount = await AsyncExecuter.CountAsync(
            query.Where(r => r.ReservationNumber.StartsWith($"R{date}"))
        );
        
        sequence = todayCount + 1;
        
        return $"R{date}{sequence:D4}";
    }
}