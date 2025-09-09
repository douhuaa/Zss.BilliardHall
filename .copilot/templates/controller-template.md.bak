# 控制器代码模板 (Controller Code Template)

## 使用说明 (Usage Instructions)

此模板用于生成符合项目规范的 Web API 控制器代码。GitHub Copilot 应根据此模板生成一致的控制器代码结构。

### 模板变量 (Template Variables)
- `{EntityName}`: 实体名称 (如: BilliardTable, Customer, Reservation)
- `{EntityNameLower}`: 小写实体名称 (如: billiardTable, customer, reservation)  
- `{EntityNamePlural}`: 复数实体名称 (如: BilliardTables, Customers, Reservations)
- `{EntityNamePluralLower}`: 小写复数实体名称 (如: billiardTables, customers, reservations)

## 标准控制器模板

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Zss.BilliardHall.Application.Services;
using Zss.BilliardHall.Application.DTOs.{EntityName};
using Zss.BilliardHall.Application.DTOs.Common;
using Zss.BilliardHall.Api.Models;

namespace Zss.BilliardHall.Api.Controllers
{
    /// <summary>
    /// {EntityName} 管理 API
    /// </summary>
    /// <remarks>
    /// 提供 {EntityName} 的完整 CRUD 操作，包括：
    /// - 创建新的 {EntityName}
    /// - 查询 {EntityName} 列表和详情
    /// - 更新 {EntityName} 信息
    /// - 删除 {EntityName}
    /// - 批量操作支持
    /// </remarks>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class {EntityNamePlural}Controller : ControllerBase
    {
        private readonly I{EntityName}Service _service;
        private readonly ILogger<{EntityNamePlural}Controller> _logger;

        /// <summary>
        /// 初始化 {EntityNamePlural}Controller
        /// </summary>
        public {EntityNamePlural}Controller(
            I{EntityName}Service service,
            ILogger<{EntityNamePlural}Controller> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 获取 {EntityName} 列表
        /// </summary>
        /// <param name="query">查询参数，支持分页、过滤、排序</param>
        /// <returns>{EntityName} 分页列表</returns>
        /// <remarks>
        /// 获取 {EntityName} 列表，支持多种查询条件：
        /// 
        /// **查询参数说明:**
        /// - `page`: 页码，从1开始，默认为1
        /// - `pageSize`: 每页大小，1-100之间，默认为20
        /// - `sortBy`: 排序字段，默认为 CreatedAt
        /// - `sortDirection`: 排序方向 (Ascending, Descending)，默认为 Descending
        /// - `search`: 关键字搜索
        /// 
        /// **示例请求:**
        /// ```
        /// GET /api/v1/{EntityNamePluralLower}?page=1&amp;pageSize=10&amp;search=关键字
        /// ```
        /// </remarks>
        /// <response code="200">成功返回 {EntityName} 列表</response>
        /// <response code="400">请求参数错误</response>
        [HttpGet]
        [AllowAnonymous] // 根据业务需求调整权限
        [ProducesResponseType(typeof(ApiResponse<PagedResult<{EntityName}ListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedResult<{EntityName}ListDto>>>> Get{EntityNamePlural}(
            [FromQuery] {EntityName}Query query)
        {
            _logger.LogInformation("获取 {EntityName} 列表，页码: {Page}, 大小: {PageSize}", 
                                  query.Page, query.PageSize);

            var result = await _service.Get{EntityNamePlural}Async(query);
            
            _logger.LogInformation("成功获取 {EntityName} 列表，总数: {TotalItems}", 
                                  result.TotalItems);

            return Ok(ApiResponse.Success(result));
        }

        /// <summary>
        /// 根据ID获取 {EntityName} 详情
        /// </summary>
        /// <param name="id">{EntityName} 唯一标识符</param>
        /// <returns>{EntityName} 详细信息</returns>
        /// <remarks>
        /// 根据指定的ID获取 {EntityName} 的详细信息。
        /// 
        /// **示例请求:**
        /// ```
        /// GET /api/v1/{EntityNamePluralLower}/123e4567-e89b-12d3-a456-426614174000
        /// ```
        /// </remarks>
        /// <response code="200">成功返回 {EntityName} 详情</response>
        /// <response code="404">{EntityName} 不存在</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<{EntityName}DetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<{EntityName}DetailDto>>> Get{EntityName}(
            [Required] Guid id)
        {
            _logger.LogInformation("获取 {EntityName} 详情，ID: {Id}", id);

            var {EntityNameLower} = await _service.GetByIdAsync(id);
            if ({EntityNameLower} == null)
            {
                _logger.LogWarning("{EntityName} 不存在，ID: {Id}", id);
                return NotFound(ApiResponse.NotFound($"{EntityName} {id} 不存在"));
            }

            _logger.LogInformation("成功获取 {EntityName} 详情，ID: {Id}", id);
            return Ok(ApiResponse.Success({EntityNameLower}));
        }

        /// <summary>
        /// 创建新的 {EntityName}
        /// </summary>
        /// <param name="dto">{EntityName} 创建信息</param>
        /// <returns>创建的 {EntityName} 信息</returns>
        /// <remarks>
        /// 创建新的 {EntityName}，所有必填字段都需要提供。
        /// 
        /// **示例请求:**
        /// ```json
        /// POST /api/v1/{EntityNamePluralLower}
        /// {
        ///   // 根据实际 DTO 结构提供示例
        /// }
        /// ```
        /// </remarks>
        /// <response code="201">成功创建 {EntityName}</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="409">资源冲突</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<{EntityName}DetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<{EntityName}DetailDto>>> Create{EntityName}(
            [FromBody] Create{EntityName}Dto dto)
        {
            _logger.LogInformation("开始创建 {EntityName}");

            var {EntityNameLower} = await _service.CreateAsync(dto);

            _logger.LogInformation("成功创建 {EntityName}，ID: {Id}", {EntityNameLower}.Id);

            return CreatedAtAction(
                nameof(Get{EntityName}),
                new { id = {EntityNameLower}.Id },
                ApiResponse.Success({EntityNameLower}, "{EntityName} 创建成功")
            );
        }

        /// <summary>
        /// 更新 {EntityName} 信息
        /// </summary>
        /// <param name="id">{EntityName} 唯一标识符</param>
        /// <param name="dto">{EntityName} 更新信息</param>
        /// <returns>更新后的 {EntityName} 信息</returns>
        /// <remarks>
        /// 完整更新指定的 {EntityName}，提供的字段将覆盖现有值。
        /// 
        /// **示例请求:**
        /// ```json
        /// PUT /api/v1/{EntityNamePluralLower}/123e4567-e89b-12d3-a456-426614174000
        /// {
        ///   // 根据实际 DTO 结构提供示例
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">成功更新 {EntityName}</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="404">{EntityName} 不存在</response>
        /// <response code="409">资源冲突</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<{EntityName}DetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<{EntityName}DetailDto>>> Update{EntityName}(
            [Required] Guid id,
            [FromBody] Update{EntityName}Dto dto)
        {
            _logger.LogInformation("开始更新 {EntityName}，ID: {Id}", id);

            var {EntityNameLower} = await _service.UpdateAsync(id, dto);

            _logger.LogInformation("成功更新 {EntityName}，ID: {Id}", id);

            return Ok(ApiResponse.Success({EntityNameLower}, "{EntityName} 更新成功"));
        }

        /// <summary>
        /// 部分更新 {EntityName} 信息
        /// </summary>
        /// <param name="id">{EntityName} 唯一标识符</param>
        /// <param name="dto">{EntityName} 部分更新信息</param>
        /// <returns>更新后的 {EntityName} 信息</returns>
        /// <remarks>
        /// 部分更新指定的 {EntityName}，只更新提供的字段。
        /// 
        /// **示例请求:**
        /// ```json
        /// PATCH /api/v1/{EntityNamePluralLower}/123e4567-e89b-12d3-a456-426614174000
        /// {
        ///   // 只包含需要更新的字段
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">成功部分更新 {EntityName}</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="404">{EntityName} 不存在</response>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<{EntityName}DetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<{EntityName}DetailDto>>> Patch{EntityName}(
            [Required] Guid id,
            [FromBody] Patch{EntityName}Dto dto)
        {
            _logger.LogInformation("开始部分更新 {EntityName}，ID: {Id}", id);

            var {EntityNameLower} = await _service.PatchAsync(id, dto);

            _logger.LogInformation("成功部分更新 {EntityName}，ID: {Id}", id);

            return Ok(ApiResponse.Success({EntityNameLower}, "{EntityName} 更新成功"));
        }

        /// <summary>
        /// 删除 {EntityName}
        /// </summary>
        /// <param name="id">{EntityName} 唯一标识符</param>
        /// <returns>删除结果</returns>
        /// <remarks>
        /// 删除指定的 {EntityName}。请注意，此操作可能是软删除或硬删除，具体取决于业务规则。
        /// 
        /// **示例请求:**
        /// ```
        /// DELETE /api/v1/{EntityNamePluralLower}/123e4567-e89b-12d3-a456-426614174000
        /// ```
        /// </remarks>
        /// <response code="204">成功删除 {EntityName}</response>
        /// <response code="404">{EntityName} 不存在</response>
        /// <response code="409">删除冲突（存在关联数据）</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Delete{EntityName}([Required] Guid id)
        {
            _logger.LogInformation("开始删除 {EntityName}，ID: {Id}", id);

            await _service.DeleteAsync(id);

            _logger.LogInformation("成功删除 {EntityName}，ID: {Id}", id);

            return NoContent();
        }

        /// <summary>
        /// 批量操作 {EntityName}
        /// </summary>
        /// <param name="dto">批量操作信息</param>
        /// <returns>批量操作结果</returns>
        /// <remarks>
        /// 支持批量创建、更新、删除 {EntityName}。
        /// 
        /// **示例请求:**
        /// ```json
        /// POST /api/v1/{EntityNamePluralLower}/batch
        /// {
        ///   "operation": "create", // create, update, delete
        ///   "items": [
        ///     // 操作项列表
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">批量操作完成</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="409">部分操作失败</response>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(ApiResponse<Batch{EntityName}ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<Batch{EntityName}ResultDto>>> Batch{EntityName}Operations(
            [FromBody] Batch{EntityName}OperationDto dto)
        {
            _logger.LogInformation("开始批量操作 {EntityName}，操作类型: {Operation}, 数量: {Count}", 
                                  dto.Operation, dto.Items?.Count ?? 0);

            var result = await _service.BatchOperationAsync(dto);

            _logger.LogInformation("批量操作 {EntityName} 完成，成功: {Success}, 失败: {Failed}", 
                                  result.SuccessCount, result.FailedCount);

            return Ok(ApiResponse.Success(result, "批量操作完成"));
        }
    }
}
```

## 子资源控制器模板 (Sub-Resource Controller Template)

当实体有子资源时，使用此模板：

```csharp
/// <summary>
/// {ParentEntity} 的 {ChildEntity} 管理 API
/// </summary>
[ApiController]
[Route("api/v1/{parentEntityPluralLower}/{parentId:guid}/[controller]")]
[Produces("application/json")]
[Authorize]
public class {ChildEntityPlural}Controller : ControllerBase
{
    private readonly I{ChildEntity}Service _service;
    private readonly ILogger<{ChildEntityPlural}Controller> _logger;

    public {ChildEntityPlural}Controller(
        I{ChildEntity}Service service,
        ILogger<{ChildEntityPlural}Controller> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取 {ParentEntity} 的 {ChildEntity} 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<{ChildEntity}Dto>>>> Get{ChildEntityPlural}(
        [Required] Guid parentId,
        [FromQuery] {ChildEntity}Query query)
    {
        query.{ParentEntity}Id = parentId;
        var result = await _service.Get{ChildEntityPlural}Async(query);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// 为 {ParentEntity} 创建 {ChildEntity}
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<{ChildEntity}DetailDto>>> Create{ChildEntity}(
        [Required] Guid parentId,
        [FromBody] Create{ChildEntity}Dto dto)
    {
        dto.{ParentEntity}Id = parentId;
        var result = await _service.CreateAsync(dto);
        
        return CreatedAtAction(
            nameof(Get{ChildEntity}),
            new { parentId, id = result.Id },
            ApiResponse.Success(result)
        );
    }

    /// <summary>
    /// 获取 {ChildEntity} 详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<{ChildEntity}DetailDto>>> Get{ChildEntity}(
        [Required] Guid parentId,
        [Required] Guid id)
    {
        var result = await _service.GetByIdAsync(id, parentId);
        if (result == null)
        {
            return NotFound(ApiResponse.NotFound($"{ChildEntity} {id} 不存在"));
        }

        return Ok(ApiResponse.Success(result));
    }
}
```

## 使用指南 (Usage Guidelines)

### 1. 生成新控制器

当需要为新实体生成控制器时，告诉 Copilot：

```
基于控制器模板为 Customer 实体生成完整的控制器代码，包括 CRUD 操作
```

### 2. 添加自定义操作

对于特殊的业务操作，可以在标准模板基础上添加：

```csharp
/// <summary>
/// 激活/停用 {EntityName}
/// </summary>
[HttpPatch("{id:guid}/status")]
public async Task<ActionResult<ApiResponse<{EntityName}DetailDto>>> UpdateStatus(
    [Required] Guid id,
    [FromBody] UpdateStatusDto dto)
{
    var result = await _service.UpdateStatusAsync(id, dto.Status, dto.Reason);
    return Ok(ApiResponse.Success(result, "状态更新成功"));
}
```

### 3. 权限控制

根据业务需求调整权限控制：

```csharp
[Authorize(Roles = "Admin,Manager")]
[RequirePermission("{EntityNameLower}.delete")]
[HttpDelete("{id:guid}")]
public async Task<ActionResult> Delete{EntityName}([Required] Guid id)
{
    // 删除逻辑
}
```

### 4. 缓存支持

对于读取频繁的资源，添加缓存：

```csharp
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "page", "pageSize" })]
[HttpGet]
public async Task<ActionResult> Get{EntityNamePlural}([FromQuery] {EntityName}Query query)
{
    // 查询逻辑
}
```

---

> 使用此模板时，请确保替换所有模板变量，并根据具体业务需求调整权限、验证和业务逻辑。