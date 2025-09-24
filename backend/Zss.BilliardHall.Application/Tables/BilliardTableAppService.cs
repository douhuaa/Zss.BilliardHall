using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Zss.BilliardHall.Entities;

namespace Zss.BilliardHall.Tables
{
    /// <summary>
    /// 台桌管理应用服务
    /// </summary>
    public class BilliardTableAppService : ApplicationService, IBilliardTableAppService
    {
        private readonly IRepository<BilliardTable, Guid> _tableRepository;
        private readonly IRepository<TableSession, Guid> _sessionRepository;
        private readonly IAsyncQueryableExecuter _asyncExecuter;

        public BilliardTableAppService(
            IRepository<BilliardTable, Guid> tableRepository,
            IRepository<TableSession, Guid> sessionRepository,
            IAsyncQueryableExecuter asyncExecuter)
        {
            _tableRepository = tableRepository;
            _sessionRepository = sessionRepository;
            _asyncExecuter = asyncExecuter;
        }

        public async Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input)
        {
            var query = await _tableRepository.GetQueryableAsync();

            // 应用过滤条件
            if (input.StoreId.HasValue)
            {
                query = query.Where(x => x.StoreId == input.StoreId.Value);
            }

            if (input.Status.HasValue)
            {
                query = query.Where(x => x.Status == input.Status.Value);
            }

            if (input.IsEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(x => x.Number.Contains(input.Keyword) || x.Name.Contains(input.Keyword));
            }

            // 排序
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderBy(x => x.SortOrder).ThenBy(x => x.Number);
            }

            // 分页
            var totalCount = await _asyncExecuter.CountAsync(query);
            var items = await _asyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var dtos = new List<BilliardTableDto>();
            foreach (var item in items)
            {
                var dto = await MapToDto(item);
                dtos.Add(dto);
            }

            return new PagedResultDto<BilliardTableDto>(totalCount, dtos);
        }

        public async Task<BilliardTableDto> GetAsync(Guid id)
        {
            var table = await _tableRepository.GetAsync(id);
            return await MapToDto(table);
        }

        public async Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
        {
            var table = new BilliardTable(
                GuidGenerator.Create(),
                input.StoreId,
                input.Number,
                input.Name
            );

            table.UpdateInfo(input.Number, input.Name ?? $"{input.Number}号台", input.HourlyRate, input.Description);

            await _tableRepository.InsertAsync(table);
            return await MapToDto(table);
        }

        public async Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
        {
            var table = await _tableRepository.GetAsync(id);
            table.UpdateInfo(input.Number, input.Name, input.HourlyRate, input.Description);
            
            await _tableRepository.UpdateAsync(table);
            return await MapToDto(table);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _tableRepository.DeleteAsync(id);
        }

        public async Task EnableAsync(Guid id)
        {
            var table = await _tableRepository.GetAsync(id);
            table.Enable();
            await _tableRepository.UpdateAsync(table);
        }

        public async Task DisableAsync(Guid id)
        {
            var table = await _tableRepository.GetAsync(id);
            table.Disable();
            await _tableRepository.UpdateAsync(table);
        }

        public async Task<string> GenerateQrCodeAsync(Guid id)
        {
            var table = await _tableRepository.GetAsync(id);
            table.GenerateQrCode();
            await _tableRepository.UpdateAsync(table);
            return table.QrCode;
        }

        public async Task<BilliardTableDto> GetByQrCodeAsync(string qrCode)
        {
            var query = await _tableRepository.GetQueryableAsync();
            var table = await _asyncExecuter.FirstOrDefaultAsync(query.Where(x => x.QrCode == qrCode));
            
            if (table == null)
            {
                throw new Volo.Abp.BusinessException("台桌不存在或二维码无效");
            }

            return await MapToDto(table);
        }

        public async Task<List<BilliardTableDto>> GetAvailableTablesAsync(Guid? storeId = null)
        {
            var query = await _tableRepository.GetQueryableAsync();
            
            query = query.Where(x => x.IsEnabled && x.Status == TableStatus.Idle);
            
            if (storeId.HasValue)
            {
                query = query.Where(x => x.StoreId == storeId.Value);
            }

            var tables = await _asyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).ThenBy(x => x.Number));
            
            var dtos = new List<BilliardTableDto>();
            foreach (var table in tables)
            {
                var dto = await MapToDto(table);
                dtos.Add(dto);
            }

            return dtos;
        }

        private async Task<BilliardTableDto> MapToDto(BilliardTable table)
        {
            var dto = new BilliardTableDto
            {
                Id = table.Id,
                StoreId = table.StoreId,
                Number = table.Number,
                Name = table.Name,
                Status = table.Status,
                StatusDisplayName = GetStatusDisplayName(table.Status),
                QrCode = table.QrCode,
                HourlyRate = table.HourlyRate,
                Description = table.Description,
                IsEnabled = table.IsEnabled
            };

            // 如果台桌正在使用，获取当前会话信息
            if (table.Status == TableStatus.InUse)
            {
                var sessionQuery = await _sessionRepository.GetQueryableAsync();
                var currentSession = await _asyncExecuter.FirstOrDefaultAsync(
                    sessionQuery.Where(x => x.TableId == table.Id && x.Status == SessionStatus.Active)
                );

                if (currentSession != null)
                {
                    dto.CurrentSessionId = currentSession.Id;
                    dto.CurrentSessionStartTime = currentSession.StartTime;
                    dto.CurrentDurationMinutes = (int)(DateTime.UtcNow - currentSession.StartTime).TotalMinutes;
                    dto.EstimatedAmount = Math.Ceiling(dto.CurrentDurationMinutes / 60.0m) * table.HourlyRate;
                }
            }

            return dto;
        }

        private string GetStatusDisplayName(TableStatus status)
        {
            return status switch
            {
                TableStatus.Idle => "空闲",
                TableStatus.Locked => "已锁定",
                TableStatus.InUse => "使用中",
                TableStatus.PendingClose => "待结算",
                TableStatus.Closed => "已结束",
                TableStatus.Maintenance => "维护中",
                TableStatus.Error => "异常",
                _ => "未知"
            };
        }
    }
}