using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Zss.BilliardHall.Tables
{
    /// <summary>
    /// 台桌管理应用服务接口
    /// </summary>
    public interface IBilliardTableAppService : IApplicationService
    {
        /// <summary>
        /// 获取台桌列表
        /// </summary>
        Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input);

        /// <summary>
        /// 获取台桌详情
        /// </summary>
        Task<BilliardTableDto> GetAsync(Guid id);

        /// <summary>
        /// 创建台桌
        /// </summary>
        Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);

        /// <summary>
        /// 更新台桌
        /// </summary>
        Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input);

        /// <summary>
        /// 删除台桌
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 启用台桌
        /// </summary>
        Task EnableAsync(Guid id);

        /// <summary>
        /// 停用台桌
        /// </summary>
        Task DisableAsync(Guid id);

        /// <summary>
        /// 生成二维码
        /// </summary>
        Task<string> GenerateQrCodeAsync(Guid id);

        /// <summary>
        /// 根据二维码获取台桌信息
        /// </summary>
        Task<BilliardTableDto> GetByQrCodeAsync(string qrCode);

        /// <summary>
        /// 获取可用台桌列表
        /// </summary>
        Task<List<BilliardTableDto>> GetAvailableTablesAsync(Guid? storeId = null);
    }
}