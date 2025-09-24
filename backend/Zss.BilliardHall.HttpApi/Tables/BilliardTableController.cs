using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Zss.BilliardHall.Tables
{
    /// <summary>
    /// 台桌管理API控制器
    /// </summary>
    [RemoteService]
    [Area("app")]
    [Route("api/app/billiard-tables")]
    public class BilliardTableController : AbpControllerBase, IBilliardTableAppService
    {
        private readonly IBilliardTableAppService _billiardTableAppService;

        public BilliardTableController(IBilliardTableAppService billiardTableAppService)
        {
            _billiardTableAppService = billiardTableAppService;
        }

        /// <summary>
        /// 获取台桌列表
        /// </summary>
        [HttpGet]
        public Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input)
        {
            return _billiardTableAppService.GetListAsync(input);
        }

        /// <summary>
        /// 获取台桌详情
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public Task<BilliardTableDto> GetAsync(Guid id)
        {
            return _billiardTableAppService.GetAsync(id);
        }

        /// <summary>
        /// 创建台桌
        /// </summary>
        [HttpPost]
        public Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
        {
            return _billiardTableAppService.CreateAsync(input);
        }

        /// <summary>
        /// 更新台桌
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
        {
            return _billiardTableAppService.UpdateAsync(id, input);
        }

        /// <summary>
        /// 删除台桌
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public Task DeleteAsync(Guid id)
        {
            return _billiardTableAppService.DeleteAsync(id);
        }

        /// <summary>
        /// 启用台桌
        /// </summary>
        [HttpPost]
        [Route("{id}/enable")]
        public Task EnableAsync(Guid id)
        {
            return _billiardTableAppService.EnableAsync(id);
        }

        /// <summary>
        /// 停用台桌
        /// </summary>
        [HttpPost]
        [Route("{id}/disable")]
        public Task DisableAsync(Guid id)
        {
            return _billiardTableAppService.DisableAsync(id);
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        [HttpPost]
        [Route("{id}/qr-code")]
        public Task<string> GenerateQrCodeAsync(Guid id)
        {
            return _billiardTableAppService.GenerateQrCodeAsync(id);
        }

        /// <summary>
        /// 根据二维码获取台桌信息
        /// </summary>
        [HttpGet]
        [Route("by-qr-code/{qrCode}")]
        public Task<BilliardTableDto> GetByQrCodeAsync(string qrCode)
        {
            return _billiardTableAppService.GetByQrCodeAsync(qrCode);
        }

        /// <summary>
        /// 获取可用台桌列表
        /// </summary>
        [HttpGet]
        [Route("available")]
        public Task<List<BilliardTableDto>> GetAvailableTablesAsync([FromQuery] Guid? storeId = null)
        {
            return _billiardTableAppService.GetAvailableTablesAsync(storeId);
        }
    }
}