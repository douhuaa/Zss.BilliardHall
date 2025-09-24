using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Zss.BilliardHall.Sessions
{
    /// <summary>
    /// 会话管理应用服务接口
    /// </summary>
    public interface ITableSessionAppService : IApplicationService
    {
        /// <summary>
        /// 获取会话列表
        /// </summary>
        Task<PagedResultDto<TableSessionDto>> GetListAsync(GetSessionsInput input);

        /// <summary>
        /// 获取会话详情
        /// </summary>
        Task<TableSessionDto> GetAsync(Guid id);

        /// <summary>
        /// 扫码开台
        /// </summary>
        Task<TableSessionDto> ScanToStartSessionAsync(ScanToStartSessionDto input);

        /// <summary>
        /// 直接开台
        /// </summary>
        Task<TableSessionDto> StartSessionAsync(StartSessionDto input);

        /// <summary>
        /// 结束会话
        /// </summary>
        Task<TableSessionDto> EndSessionAsync(EndSessionDto input);

        /// <summary>
        /// 取消会话
        /// </summary>
        Task CancelSessionAsync(Guid id);

        /// <summary>
        /// 获取用户当前活跃会话
        /// </summary>
        Task<TableSessionDto> GetCurrentActiveSessionAsync(Guid userId);

        /// <summary>
        /// 获取台桌当前会话
        /// </summary>
        Task<TableSessionDto> GetTableCurrentSessionAsync(Guid tableId);

        /// <summary>
        /// 更新会话时长和金额
        /// </summary>
        Task<TableSessionDto> RefreshSessionAsync(Guid id);
    }
}