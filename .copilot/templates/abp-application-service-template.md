# ABP Application Service 代码模板 (ABP Application Service Code Template)

## 使用说明 (Usage Instructions)

此模板用于生成符合 ABP 框架规范的 Application Service 代码。GitHub Copilot 应根据此模板生成一致的 Application Service 代码结构。

### 模板变量 (Template Variables)
- `{EntityName}`: 实体名称 (如: BilliardTable, Customer, Reservation)
- `{EntityNameLower}`: 小写实体名称 (如: billiardTable, customer, reservation)
- `{EntityNamePlural}`: 复数实体名称 (如: BilliardTables, Customers, Reservations)
- `{EntityNamePluralLower}`: 小写复数实体名称 (如: billiardTables, customers, reservations)
- `{ModuleName}`: 模块名称 (如: BilliardHall)

## Application Service 接口模板

### 接口定义 (在 Application.Contracts 项目中)

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Zss.{ModuleName}.{EntityNamePlural}
{
    /// <summary>
    /// {EntityName} 应用服务接口
    /// 自动映射到 /api/app/{entity-name-kebab-case} 路由
    /// </summary>
    public interface I{EntityName}AppService : IApplicationService
    {
        #region 查询操作 (Query Operations)
        
        /// <summary>
        /// 获取 {EntityName} 分页列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<{EntityName}Dto>> GetListAsync(Get{EntityNamePlural}Input input);
        
        /// <summary>
        /// 获取单个 {EntityName}
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>{EntityName} 详情</returns>
        Task<{EntityName}Dto> GetAsync(Guid id);
        
        /// <summary>
        /// 获取 {EntityName} 查找列表 (用于下拉框等)
        /// </summary>
        /// <returns>查找项列表</returns>
        Task<ListResultDto<{EntityName}LookupDto>> GetLookupAsync();
        
        #endregion

        #region 写操作 (Write Operations)
        
        /// <summary>
        /// 创建新的 {EntityName}
        /// </summary>
        /// <param name="input">创建输入</param>
        /// <returns>创建的 {EntityName}</returns>
        Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input);
        
        /// <summary>
        /// 更新 {EntityName}
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <param name="input">更新输入</param>
        /// <returns>更新后的 {EntityName}</returns>
        Task<{EntityName}Dto> UpdateAsync(Guid id, Update{EntityName}Dto input);
        
        /// <summary>
        /// 删除 {EntityName}
        /// </summary>
        /// <param name="id">实体ID</param>
        Task DeleteAsync(Guid id);
        
        #endregion

        #region 批量操作 (Batch Operations)
        
        /// <summary>
        /// 批量删除 {EntityName}
        /// </summary>
        /// <param name="ids">实体ID列表</param>
        Task DeleteManyAsync(IEnumerable<Guid> ids);
        
        /// <summary>
        /// 获取多个 {EntityName}
        /// </summary>
        /// <param name="ids">实体ID列表</param>
        /// <returns>{EntityName} 列表</returns>
        Task<ListResultDto<{EntityName}Dto>> GetManyAsync(IEnumerable<Guid> ids);
        
        #endregion

        #region 业务操作 (Business Operations)
        
        // 在此添加特定于 {EntityName} 的业务操作
        // 例如：状态变更、业务流程等
        
        #endregion

        #region 统计和报表 (Statistics and Reports)
        
        /// <summary>
        /// 获取 {EntityName} 统计信息
        /// </summary>
        /// <param name="input">统计查询参数</param>
        /// <returns>统计结果</returns>
        Task<{EntityName}StatisticsDto> GetStatisticsAsync(Get{EntityName}StatisticsInput input);
        
        #endregion
    }
}
```

## Application Service 实现模板

### 服务实现 (在 Application 项目中)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Zss.{ModuleName}.Permissions;

namespace Zss.{ModuleName}.{EntityNamePlural}
{
    /// <summary>
    /// {EntityName} 应用服务实现
    /// 自动注册为 API 控制器: /api/app/{entity-name-kebab-case}
    /// </summary>
    [Authorize({ModuleName}Permissions.{EntityNamePlural}.Default)]
    public class {EntityName}AppService : {ModuleName}AppService, I{EntityName}AppService
    {
        #region 依赖注入 (Dependency Injection)
        
        private readonly IRepository<{EntityName}, Guid> _{entityNameLower}Repository;
        private readonly {EntityName}Manager _{entityNameLower}Manager;
        // 添加其他需要的依赖服务

        public {EntityName}AppService(
            IRepository<{EntityName}, Guid> {entityNameLower}Repository,
            {EntityName}Manager {entityNameLower}Manager)
        {
            _{entityNameLower}Repository = {entityNameLower}Repository;
            _{entityNameLower}Manager = {entityNameLower}Manager;
        }
        
        #endregion

        #region 查询操作实现 (Query Operations Implementation)
        
        /// <summary>
        /// 获取 {EntityName} 分页列表
        /// GET: /api/app/{entity-name-kebab-case}
        /// </summary>
        public virtual async Task<PagedResultDto<{EntityName}Dto>> GetListAsync(Get{EntityNamePlural}Input input)
        {
            var queryable = await _{entityNameLower}Repository.GetQueryableAsync();
            
            // 应用过滤条件
            queryable = ApplyFilters(queryable, input);

            // 获取总数
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            
            // 应用排序和分页
            queryable = ApplySortingAndPaging(queryable, input);

            // 执行查询并转换为 DTO
            var entities = await AsyncExecuter.ToListAsync(queryable);
            var entityDtos = await MapToGetListOutputDtosAsync(entities);
            
            return new PagedResultDto<{EntityName}Dto>(totalCount, entityDtos);
        }

        /// <summary>
        /// 获取单个 {EntityName}
        /// GET: /api/app/{entity-name-kebab-case}/{{id}}
        /// </summary>
        public virtual async Task<{EntityName}Dto> GetAsync(Guid id)
        {
            var entity = await _{entityNameLower}Repository.GetAsync(id);
            return await MapToGetOutputDtoAsync(entity);
        }

        /// <summary>
        /// 获取查找列表
        /// GET: /api/app/{entity-name-kebab-case}/lookup
        /// </summary>
        public virtual async Task<ListResultDto<{EntityName}LookupDto>> GetLookupAsync()
        {
            var queryable = await _{entityNameLower}Repository.GetQueryableAsync();
            
            // 只选择必要的字段用于查找
            var entities = await AsyncExecuter.ToListAsync(
                queryable.OrderBy(x => x./* 排序字段 */).Take(1000));
            
            var lookupDtos = entities.Select(x => new {EntityName}LookupDto
            {
                Id = x.Id,
                DisplayName = x./* 显示字段 */
            }).ToList();
            
            return new ListResultDto<{EntityName}LookupDto>(lookupDtos);
        }
        
        #endregion

        #region 写操作实现 (Write Operations Implementation)
        
        /// <summary>
        /// 创建 {EntityName}
        /// POST: /api/app/{entity-name-kebab-case}
        /// </summary>
        [Authorize({ModuleName}Permissions.{EntityNamePlural}.Create)]
        public virtual async Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input)
        {
            // 使用领域服务创建实体
            var entity = await _{entityNameLower}Manager.CreateAsync(
                /* 传入必要的参数 */);

            // 映射额外属性
            await MapToEntityAsync(input, entity);

            // 保存到仓储
            entity = await _{entityNameLower}Repository.InsertAsync(entity, autoSave: true);
            
            // 发布领域事件 (可选)
            await PublishEntityCreatedEventAsync(entity);

            return await MapToGetOutputDtoAsync(entity);
        }

        /// <summary>
        /// 更新 {EntityName}
        /// PUT: /api/app/{entity-name-kebab-case}/{{id}}
        /// </summary>
        [Authorize({ModuleName}Permissions.{EntityNamePlural}.Edit)]
        public virtual async Task<{EntityName}Dto> UpdateAsync(Guid id, Update{EntityName}Dto input)
        {
            var entity = await _{entityNameLower}Repository.GetAsync(id);
            
            // 使用 ABP ObjectMapper 进行映射
            await MapToEntityAsync(input, entity);
            
            // 业务逻辑验证 (通过领域服务)
            await _{entityNameLower}Manager.UpdateAsync(entity);
            
            // 更新到仓储
            entity = await _{entityNameLower}Repository.UpdateAsync(entity, autoSave: true);
            
            // 发布领域事件 (可选)
            await PublishEntityUpdatedEventAsync(entity);

            return await MapToGetOutputDtoAsync(entity);
        }

        /// <summary>
        /// 删除 {EntityName}
        /// DELETE: /api/app/{entity-name-kebab-case}/{{id}}
        /// </summary>
        [Authorize({ModuleName}Permissions.{EntityNamePlural}.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            // 业务规则检查 (通过领域服务)
            await _{entityNameLower}Manager.DeleteAsync(id);
            
            // 从仓储删除
            await _{entityNameLower}Repository.DeleteAsync(id);
            
            // 发布领域事件 (可选)
            await PublishEntityDeletedEventAsync(id);
        }
        
        #endregion

        #region 批量操作实现 (Batch Operations Implementation)
        
        /// <summary>
        /// 批量删除
        /// DELETE: /api/app/{entity-name-kebab-case}/batch
        /// </summary>
        [Authorize({ModuleName}Permissions.{EntityNamePlural}.Delete)]
        public virtual async Task DeleteManyAsync(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                await _{entityNameLower}Manager.DeleteAsync(id);
            }
            
            await _{entityNameLower}Repository.DeleteManyAsync(ids);
        }

        /// <summary>
        /// 获取多个实体
        /// GET: /api/app/{entity-name-kebab-case}/many?ids={{id1}}&ids={{id2}}
        /// </summary>
        public virtual async Task<ListResultDto<{EntityName}Dto>> GetManyAsync(IEnumerable<Guid> ids)
        {
            var entities = await _{entityNameLower}Repository.GetListAsync(x => ids.Contains(x.Id));
            var entityDtos = await MapToGetListOutputDtosAsync(entities);
            
            return new ListResultDto<{EntityName}Dto>(entityDtos);
        }
        
        #endregion

        #region 统计和报表实现 (Statistics Implementation)
        
        /// <summary>
        /// 获取统计信息
        /// GET: /api/app/{entity-name-kebab-case}/statistics
        /// </summary>
        public virtual async Task<{EntityName}StatisticsDto> GetStatisticsAsync(Get{EntityName}StatisticsInput input)
        {
            var queryable = await _{entityNameLower}Repository.GetQueryableAsync();
            
            // 应用统计过滤条件
            queryable = ApplyStatisticsFilters(queryable, input);
            
            // 计算统计信息
            var statistics = new {EntityName}StatisticsDto
            {
                TotalCount = await AsyncExecuter.CountAsync(queryable),
                // 添加其他统计字段
            };
            
            return statistics;
        }
        
        #endregion

        #region 辅助方法 (Helper Methods)
        
        /// <summary>
        /// 应用查询过滤条件
        /// </summary>
        protected virtual IQueryable<{EntityName}> ApplyFilters(IQueryable<{EntityName}> queryable, Get{EntityNamePlural}Input input)
        {
            return queryable
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), 
                         x => x./* 搜索字段 */.Contains(input.Filter))
                // 添加其他过滤条件
                ;
        }

        /// <summary>
        /// 应用排序和分页
        /// </summary>
        protected virtual IQueryable<{EntityName}> ApplySortingAndPaging(IQueryable<{EntityName}> queryable, Get{EntityNamePlural}Input input)
        {
            return queryable
                .OrderBy(input.Sorting ?? nameof({EntityName}./* 默认排序字段 */))
                .PageBy(input.SkipCount, input.MaxResultCount);
        }

        /// <summary>
        /// 应用统计过滤条件
        /// </summary>
        protected virtual IQueryable<{EntityName}> ApplyStatisticsFilters(IQueryable<{EntityName}> queryable, Get{EntityName}StatisticsInput input)
        {
            return queryable
                .WhereIf(input.StartDate.HasValue, x => x.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, x => x.CreationTime <= input.EndDate.Value);
        }

        /// <summary>
        /// 发布实体创建事件
        /// </summary>
        protected virtual async Task PublishEntityCreatedEventAsync({EntityName} entity)
        {
            await LocalEventBus.PublishAsync(new {EntityName}CreatedEto
            {
                Id = entity.Id,
                // 添加其他事件属性
            });
        }

        /// <summary>
        /// 发布实体更新事件
        /// </summary>
        protected virtual async Task PublishEntityUpdatedEventAsync({EntityName} entity)
        {
            await LocalEventBus.PublishAsync(new {EntityName}UpdatedEto
            {
                Id = entity.Id,
                // 添加其他事件属性
            });
        }

        /// <summary>
        /// 发布实体删除事件
        /// </summary>
        protected virtual async Task PublishEntityDeletedEventAsync(Guid id)
        {
            await LocalEventBus.PublishAsync(new {EntityName}DeletedEto
            {
                Id = id
            });
        }
        
        #endregion
    }
}
```

## DTO 模板

### 输出 DTO 模板

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace Zss.{ModuleName}.{EntityNamePlural}
{
    /// <summary>
    /// {EntityName} 数据传输对象
    /// </summary>
    public class {EntityName}Dto : FullAuditedEntityDto<Guid>
    {
        // 基本属性
        // 根据实体添加相应的属性
        
        // 业务属性
        // 例如：
        // public string Name { get; set; }
        // public string Description { get; set; }
        // public decimal Price { get; set; }
        
        // 枚举显示名称 (本地化)
        // public string StatusDisplayName => L[$"Enum:{EntityName}Status.{Status}"];
        
        // 计算属性
        // public bool IsActive => Status == {EntityName}Status.Active;
        
        // 关联数据 (可选)
        // public RelatedEntityDto RelatedEntity { get; set; }
    }

    /// <summary>
    /// {EntityName} 查找 DTO (用于下拉框等)
    /// </summary>
    public class {EntityName}LookupDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
    }
}
```

### 输入 DTO 模板

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Zss.{ModuleName}.{EntityNamePlural}
{
    /// <summary>
    /// 创建 {EntityName} 输入 DTO
    /// </summary>
    public class Create{EntityName}Dto : IValidatableObject
    {
        // 必填属性
        [Required(ErrorMessage = "字段不能为空")]
        [StringLength({EntityName}Consts.MaxNameLength, ErrorMessage = "长度不能超过{1}个字符")]
        public string Name { get; set; }

        // 可选属性
        [StringLength({EntityName}Consts.MaxDescriptionLength)]
        public string Description { get; set; }

        // 数值属性
        [Range(0.01, 999999.99, ErrorMessage = "值必须在{1}到{2}之间")]
        public decimal Price { get; set; }

        // 枚举属性
        [Required]
        public {EntityName}Status Status { get; set; }

        // 自定义验证
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 业务规则验证
            // 例如：
            // if (Status == {EntityName}Status.Premium && Price < 100)
            // {
            //     yield return new ValidationResult(
            //         "高级类型的价格不能低于100",
            //         new[] { nameof(Price) });
            // }
        }
    }

    /// <summary>
    /// 更新 {EntityName} 输入 DTO
    /// </summary>
    public class Update{EntityName}Dto
    {
        // 可更新的属性 (通常使用可空类型)
        [StringLength({EntityName}Consts.MaxNameLength)]
        public string Name { get; set; }

        [StringLength({EntityName}Consts.MaxDescriptionLength)]
        public string Description { get; set; }

        [Range(0.01, 999999.99)]
        public decimal? Price { get; set; }

        public {EntityName}Status? Status { get; set; }
    }

    /// <summary>
    /// 查询 {EntityName} 输入 DTO
    /// </summary>
    public class Get{EntityNamePlural}Input : PagedAndSortedResultRequestDto
    {
        // 通用搜索
        public string Filter { get; set; }

        // 特定过滤条件
        public {EntityName}Status? Status { get; set; }
        
        // 范围过滤
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // 日期过滤
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Get{EntityNamePlural}Input()
        {
            MaxResultCount = 20;
            Sorting = "CreationTime desc";
        }
    }

    /// <summary>
    /// 统计查询输入 DTO
    /// </summary>
    public class Get{EntityName}StatisticsInput
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public {EntityName}Status? Status { get; set; }
    }

    /// <summary>
    /// 统计结果 DTO
    /// </summary>
    public class {EntityName}StatisticsDto
    {
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<{EntityName}Status, int> StatusCounts { get; set; } = new();
        // 添加其他统计字段
    }
}
```

## 使用示例 (Usage Example)

基于 `BilliardTable` 实体的完整示例：

```csharp
// 1. 接口定义
public interface IBilliardTableAppService : IApplicationService
{
    Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input);
    Task<BilliardTableDto> GetAsync(Guid id);
    Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);
    Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input);
    Task DeleteAsync(Guid id);
    Task<BilliardTableDto> ChangeStatusAsync(Guid id, ChangeBilliardTableStatusDto input);
}

// 2. 服务实现
[Authorize(BilliardHallPermissions.BilliardTables.Default)]
public class BilliardTableAppService : BilliardHallAppService, IBilliardTableAppService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;
    private readonly BilliardTableManager _billiardTableManager;

    // 实现各个方法...
}

// 3. DTO 定义
public class BilliardTableDto : FullAuditedEntityDto<Guid>
{
    public int Number { get; set; }
    public BilliardTableType Type { get; set; }
    public BilliardTableStatus Status { get; set; }
    public decimal HourlyRate { get; set; }
    // ...
}
```

## 注意事项 (Important Notes)

1. **权限控制**: 确保为每个操作添加适当的权限验证
2. **异常处理**: ABP 会自动处理常见异常，但业务异常需要明确抛出
3. **多租户**: ABP 会自动应用多租户过滤，通常不需要手动处理
4. **本地化**: 使用 ABP 的本地化资源进行错误消息和显示文本
5. **审计**: ABP 会自动记录审计信息，无需手动处理
6. **事件**: 使用领域事件进行松耦合的业务逻辑处理

此模板确保生成的 Application Service 符合 ABP 框架的最佳实践和项目架构要求。