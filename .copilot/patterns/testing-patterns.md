# ABP 测试基础设施指导 (ABP Testing Infrastructure Guidelines)

## 总体原则 (General Principles)

### 1. ABP 测试架构
- 使用 ABP 提供的测试基础设施
- 遵循 ABP 的测试项目结构
- 利用 ABP 的依赖注入和模块系统
- 集成 ABP 的多租户和权限测试
- 使用 ABP 的内存数据库测试

### 2. 测试分层策略
- **Domain.Tests**: 领域层单元测试
- **Application.Tests**: 应用服务集成测试
- **EntityFrameworkCore.Tests**: 数据访问层测试
- **HttpApi.Client.ConsoleTestApp**: HTTP API 集成测试
- **TestBase**: 共享测试基础设施

### 3. 测试最佳实践
- 快速、独立、可重复的测试
- 使用内存数据库提高测试速度
- 模拟外部依赖
- 测试业务规则和异常情况
- 集成测试覆盖关键业务流程

## ABP 测试基类设计

### 1. 测试基础设施

```csharp
// BilliardHallTestBaseModule.cs - 测试模块基类
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace Zss.BilliardHall.TestBase
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpTestBaseModule),
        typeof(AbpAuthorizationModule),
        typeof(BilliardHallDomainModule)
    )]
    public class BilliardHallTestBaseModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAlwaysAllowAuthorization();
        }
    }
}

// BilliardHallTestBase.cs - 测试基类
using Volo.Abp;
using Volo.Abp.Testing;

namespace Zss.BilliardHall.TestBase
{
    public abstract class BilliardHallTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
        where TStartupModule : IAbpModule
    {
        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }
    }
}
```

### 2. Domain 层测试基类

```csharp
// BilliardHallDomainTestModule.cs
[DependsOn(
    typeof(BilliardHallTestBaseModule),
    typeof(BilliardHallDomainModule)
)]
public class BilliardHallDomainTestModule : AbpModule
{
    // Domain 层特定配置
}

// BilliardHallDomainTestBase.cs
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Zss.BilliardHall.Domain.Tests
{
    public abstract class BilliardHallDomainTestBase : BilliardHallTestBase<BilliardHallDomainTestModule>
    {
        protected IRepository<BilliardHall, Guid> BilliardHallRepository { get; }
        protected IRepository<BilliardTable, Guid> BilliardTableRepository { get; }
        protected BilliardTableManager BilliardTableManager { get; }

        protected BilliardHallDomainTestBase()
        {
            BilliardHallRepository = GetRequiredService<IRepository<BilliardHall, Guid>>();
            BilliardTableRepository = GetRequiredService<IRepository<BilliardTable, Guid>>();
            BilliardTableManager = GetRequiredService<BilliardTableManager>();
        }

        /// <summary>
        /// 创建测试台球厅
        /// </summary>
        protected async Task<BilliardHall> CreateTestBilliardHallAsync(string name = "测试台球厅")
        {
            var hall = new BilliardHall(
                Guid.NewGuid(),
                name,
                "测试地址",
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(23)
            );

            return await BilliardHallRepository.InsertAsync(hall, autoSave: true);
        }

        /// <summary>
        /// 创建测试台球桌
        /// </summary>
        protected async Task<BilliardTable> CreateTestBilliardTableAsync(
            Guid? hallId = null,
            int number = 1,
            BilliardTableType type = BilliardTableType.ChineseEightBall,
            decimal hourlyRate = 50.0m)
        {
            if (!hallId.HasValue)
            {
                var hall = await CreateTestBilliardHallAsync();
                hallId = hall.Id;
            }

            var table = await BilliardTableManager.CreateAsync(number, type, hourlyRate);
            return await BilliardTableRepository.InsertAsync(table, autoSave: true);
        }
    }
}
```

### 3. Application 层测试基类

```csharp
// BilliardHallApplicationTestModule.cs
[DependsOn(
    typeof(BilliardHallDomainTestModule),
    typeof(BilliardHallApplicationModule)
)]
public class BilliardHallApplicationTestModule : AbpModule
{
    // Application 层特定配置
}

// BilliardHallApplicationTestBase.cs
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Users;

namespace Zss.BilliardHall.Application.Tests
{
    public abstract class BilliardHallApplicationTestBase : BilliardHallDomainTestBase
    {
        // Application Services
        protected IBilliardTableAppService BilliardTableAppService { get; }
        protected IReservationAppService ReservationAppService { get; }
        
        // 当前用户模拟
        protected ICurrentUser CurrentUser { get; }

        protected BilliardHallApplicationTestBase()
        {
            BilliardTableAppService = GetRequiredService<IBilliardTableAppService>();
            ReservationAppService = GetRequiredService<IReservationAppService>();
            CurrentUser = GetRequiredService<ICurrentUser>();
        }

        /// <summary>
        /// 设置当前用户
        /// </summary>
        protected void SetCurrentUser(Guid userId, string userName = "testuser")
        {
            GetRequiredService<ICurrentUser>().Id = userId;
            GetRequiredService<ICurrentUser>().UserName = userName;
        }

        /// <summary>
        /// 设置当前租户
        /// </summary>
        protected void SetCurrentTenant(Guid tenantId)
        {
            GetRequiredService<ICurrentTenant>().Change(tenantId);
        }
    }
}
```

## 领域层测试模式

### 1. 实体测试

```csharp
// BilliardTable_Tests.cs
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Domain.Tests
{
    public class BilliardTable_Tests : BilliardHallDomainTestBase
    {
        [Fact]
        public void Should_Create_BilliardTable_With_Valid_Data()
        {
            // Arrange & Act
            var table = new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(), // HallId
                1,
                BilliardTableType.ChineseEightBall,
                50.00m,
                100.0f,
                200.0f
            );

            // Assert
            table.Number.ShouldBe(1);
            table.Type.ShouldBe(BilliardTableType.ChineseEightBall);
            table.Status.ShouldBe(BilliardTableStatus.Available);
            table.HourlyRate.ShouldBe(50.00m);
            table.LocationX.ShouldBe(100.0f);
            table.LocationY.ShouldBe(200.0f);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Not_Create_BilliardTable_With_Invalid_Number(int invalidNumber)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(),
                invalidNumber,
                BilliardTableType.ChineseEightBall,
                50.00m
            ));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10.5)]
        public void Should_Not_Create_BilliardTable_With_Invalid_HourlyRate(decimal invalidRate)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                BilliardTableType.ChineseEightBall,
                invalidRate
            ));
        }

        [Fact]
        public void Should_Change_Status_Successfully()
        {
            // Arrange
            var table = new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                BilliardTableType.ChineseEightBall,
                50.00m
            );

            // Act
            table.ChangeStatus(BilliardTableStatus.Maintenance);

            // Assert
            table.Status.ShouldBe(BilliardTableStatus.Maintenance);
        }

        [Fact]
        public void Should_Not_Change_Status_From_OutOfOrder_To_Available_Directly()
        {
            // Arrange
            var table = new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                BilliardTableType.ChineseEightBall,
                50.00m
            );
            
            table.ChangeStatus(BilliardTableStatus.OutOfOrder);

            // Act & Assert
            Should.Throw<BusinessException>(() => 
                table.ChangeStatus(BilliardTableStatus.Available));
        }

        [Fact]
        public void Should_Update_Location_Successfully()
        {
            // Arrange
            var table = new BilliardTable(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                BilliardTableType.ChineseEightBall,
                50.00m
            );

            // Act
            table.UpdateLocation(150.5f, 250.5f);

            // Assert
            table.LocationX.ShouldBe(150.5f);
            table.LocationY.ShouldBe(250.5f);
        }
    }
}
```

### 2. 领域服务测试

```csharp
// BilliardTableManager_Tests.cs
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Domain.Tests
{
    public class BilliardTableManager_Tests : BilliardHallDomainTestBase
    {
        private readonly BilliardTableManager _billiardTableManager;

        public BilliardTableManager_Tests()
        {
            _billiardTableManager = GetRequiredService<BilliardTableManager>();
        }

        [Fact]
        public async Task Should_Create_BilliardTable()
        {
            // Act
            var table = await _billiardTableManager.CreateAsync(
                1,
                BilliardTableType.ChineseEightBall,
                50.00m,
                100.0f,
                200.0f
            );

            // Assert
            table.ShouldNotBeNull();
            table.Number.ShouldBe(1);
            table.Type.ShouldBe(BilliardTableType.ChineseEightBall);
            table.Status.ShouldBe(BilliardTableStatus.Available);
            table.HourlyRate.ShouldBe(50.00m);
        }

        [Fact]
        public async Task Should_Not_Create_BilliardTable_With_Duplicate_Number()
        {
            // Arrange
            var hallId = Guid.NewGuid();
            await BilliardTableRepository.InsertAsync(new BilliardTable(
                Guid.NewGuid(),
                hallId,
                1,
                BilliardTableType.ChineseEightBall,
                50.00m
            ));

            // Act & Assert
            var exception = await Should.ThrowAsync<BusinessException>(
                async () => await _billiardTableManager.CreateAsync(1, BilliardTableType.AmericanNineBall, 60.00m)
            );

            exception.Code.ShouldBe(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists);
        }

        [Fact]
        public async Task Should_Change_Status_With_Business_Rules()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();

            // Act
            await _billiardTableManager.ChangeStatusAsync(table, BilliardTableStatus.Maintenance, "定期维护");

            // Assert
            table.Status.ShouldBe(BilliardTableStatus.Maintenance);
        }

        [Fact]
        public async Task Should_Not_Change_Status_If_Table_Has_Active_Reservation()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();
            
            // 创建活跃预约 (模拟)
            var reservation = new Reservation(
                Guid.NewGuid(),
                table.Id,
                Guid.NewGuid(), // CustomerId
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(3),
                100.00m
            );

            // Act & Assert
            var exception = await Should.ThrowAsync<BusinessException>(
                async () => await _billiardTableManager.ChangeStatusAsync(table, BilliardTableStatus.Maintenance)
            );

            exception.Code.ShouldBe(BilliardHallDomainErrorCodes.CannotChangeStatusWithActiveReservation);
        }

        [Theory]
        [InlineData(BilliardTableType.ChineseEightBall, 10.0)] // 太低
        [InlineData(BilliardTableType.Snooker, 50.0)] // 斯诺克价格太低
        public async Task Should_Validate_Hourly_Rate_Business_Rules(BilliardTableType type, decimal rate)
        {
            // Act & Assert
            await Should.ThrowAsync<BusinessException>(
                async () => await _billiardTableManager.CreateAsync(1, type, rate)
            );
        }
    }
}
```

### 3. 聚合根测试

```csharp
// BilliardHall_Tests.cs
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Domain.Tests
{
    public class BilliardHall_Tests : BilliardHallDomainTestBase
    {
        [Fact]
        public void Should_Create_BilliardHall_With_Valid_Data()
        {
            // Arrange & Act
            var hall = new BilliardHall(
                Guid.NewGuid(),
                "星际台球厅",
                "北京市朝阳区",
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(23)
            );

            // Assert
            hall.Name.ShouldBe("星际台球厅");
            hall.Address.ShouldBe("北京市朝阳区");
            hall.OpenTime.ShouldBe(TimeSpan.FromHours(9));
            hall.CloseTime.ShouldBe(TimeSpan.FromHours(23));
            hall.Tables.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Not_Create_BilliardHall_With_Invalid_Name(string invalidName)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => new BilliardHall(
                Guid.NewGuid(),
                invalidName,
                "地址"
            ));
        }

        [Fact]
        public void Should_Add_Table_To_Hall()
        {
            // Arrange
            var hall = new BilliardHall(
                Guid.NewGuid(),
                "测试台球厅",
                "测试地址"
            );

            // Act
            var table = hall.AddTable(1, BilliardTableType.ChineseEightBall, 50.0m);

            // Assert
            hall.Tables.Count.ShouldBe(1);
            hall.Tables.First().ShouldBe(table);
            table.Number.ShouldBe(1);
            table.BilliardHallId.ShouldBe(hall.Id);
        }

        [Fact]
        public void Should_Not_Add_Table_With_Duplicate_Number()
        {
            // Arrange
            var hall = new BilliardHall(Guid.NewGuid(), "测试台球厅", "测试地址");
            hall.AddTable(1, BilliardTableType.ChineseEightBall, 50.0m);

            // Act & Assert
            var exception = Should.Throw<BusinessException>(() => 
                hall.AddTable(1, BilliardTableType.AmericanNineBall, 60.0m));

            exception.Code.ShouldBe(BilliardHallDomainErrorCodes.TableNumberAlreadyExists);
        }

        [Fact]
        public void Should_Update_Business_Hours()
        {
            // Arrange
            var hall = new BilliardHall(Guid.NewGuid(), "测试台球厅", "测试地址");

            // Act
            hall.UpdateBusinessHours(TimeSpan.FromHours(8), TimeSpan.FromHours(24));

            // Assert
            hall.OpenTime.ShouldBe(TimeSpan.FromHours(8));
            hall.CloseTime.ShouldBe(TimeSpan.FromHours(24));
        }
    }
}
```

## Application 层测试模式

### 1. Application Service 测试

```csharp
// BilliardTableAppService_Tests.cs
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace Zss.BilliardHall.Application.Tests
{
    public class BilliardTableAppService_Tests : BilliardHallApplicationTestBase
    {
        private readonly IBilliardTableAppService _billiardTableAppService;

        public BilliardTableAppService_Tests()
        {
            _billiardTableAppService = GetRequiredService<IBilliardTableAppService>();
        }

        [Fact]
        public async Task Should_Get_List_With_Paging()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            
            // 创建多个台球桌
            for (int i = 1; i <= 25; i++)
            {
                await CreateTestBilliardTableAsync(hall.Id, i);
            }

            var input = new GetBilliardTablesInput
            {
                MaxResultCount = 10,
                SkipCount = 0
            };

            // Act
            var result = await _billiardTableAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(25);
            result.Items.Count.ShouldBe(10);
        }

        [Fact]
        public async Task Should_Get_List_With_Filtering()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            await CreateTestBilliardTableAsync(hall.Id, 1, BilliardTableType.ChineseEightBall);
            await CreateTestBilliardTableAsync(hall.Id, 2, BilliardTableType.AmericanNineBall);
            await CreateTestBilliardTableAsync(hall.Id, 3, BilliardTableType.Snooker);

            var input = new GetBilliardTablesInput
            {
                Type = BilliardTableType.ChineseEightBall
            };

            // Act
            var result = await _billiardTableAppService.GetListAsync(input);

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Items.First().Type.ShouldBe(BilliardTableType.ChineseEightBall);
        }

        [Fact]
        public async Task Should_Get_Single_Table()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();

            // Act
            var result = await _billiardTableAppService.GetAsync(table.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(table.Id);
            result.Number.ShouldBe(table.Number);
            result.Type.ShouldBe(table.Type);
        }

        [Fact]
        public async Task Should_Create_Table_With_Valid_Input()
        {
            // Arrange
            var input = new CreateBilliardTableDto
            {
                Number = 10,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = 55.00m,
                LocationX = 100.0f,
                LocationY = 200.0f
            };

            // Act
            var result = await _billiardTableAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Number.ShouldBe(input.Number);
            result.Type.ShouldBe(input.Type);
            result.HourlyRate.ShouldBe(input.HourlyRate);
            result.Status.ShouldBe(BilliardTableStatus.Available);

            // 验证数据库中的数据
            var tableInDb = await BilliardTableRepository.GetAsync(result.Id);
            tableInDb.ShouldNotBeNull();
            tableInDb.Number.ShouldBe(input.Number);
        }

        [Fact]
        public async Task Should_Not_Create_Table_With_Duplicate_Number()
        {
            // Arrange
            await CreateTestBilliardTableAsync(number: 1);

            var input = new CreateBilliardTableDto
            {
                Number = 1, // 重复号码
                Type = BilliardTableType.AmericanNineBall,
                HourlyRate = 60.00m
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<BusinessException>(
                async () => await _billiardTableAppService.CreateAsync(input)
            );

            exception.Code.ShouldBe(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists);
        }

        [Fact]
        public async Task Should_Update_Table()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();

            var input = new UpdateBilliardTableDto
            {
                HourlyRate = 75.00m,
                LocationX = 150.0f,
                LocationY = 250.0f
            };

            // Act
            var result = await _billiardTableAppService.UpdateAsync(table.Id, input);

            // Assert
            result.HourlyRate.ShouldBe(input.HourlyRate.Value);
            result.LocationX.ShouldBe(input.LocationX.Value);
            result.LocationY.ShouldBe(input.LocationY.Value);

            // 验证数据库更新
            var updatedTable = await BilliardTableRepository.GetAsync(table.Id);
            updatedTable.HourlyRate.ShouldBe(input.HourlyRate.Value);
        }

        [Fact]
        public async Task Should_Delete_Table()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();

            // Act
            await _billiardTableAppService.DeleteAsync(table.Id);

            // Assert
            var deletedTable = await BilliardTableRepository.FindAsync(table.Id);
            deletedTable.ShouldBeNull(); // 软删除或硬删除取决于配置
        }

        [Fact]
        public async Task Should_Change_Table_Status()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();

            var input = new ChangeBilliardTableStatusDto
            {
                Status = BilliardTableStatus.Maintenance,
                Reason = "定期维护"
            };

            // Act
            var result = await _billiardTableAppService.ChangeStatusAsync(table.Id, input);

            // Assert
            result.Status.ShouldBe(BilliardTableStatus.Maintenance);

            // 验证数据库更新
            var updatedTable = await BilliardTableRepository.GetAsync(table.Id);
            updatedTable.Status.ShouldBe(BilliardTableStatus.Maintenance);
        }
    }
}
```

### 2. DTO 验证测试

```csharp
// BilliardTableDto_Validation_Tests.cs
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Application.Tests
{
    public class BilliardTableDto_Validation_Tests : BilliardHallApplicationTestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1000)] // 超过最大值
        public void CreateBilliardTableDto_Should_Validate_Number_Range(int invalidNumber)
        {
            // Arrange
            var dto = new CreateBilliardTableDto
            {
                Number = invalidNumber,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = 50.0m
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.ShouldContain(vr => vr.MemberNames.Contains(nameof(dto.Number)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10.5)]
        [InlineData(10000)] // 超过最大值
        public void CreateBilliardTableDto_Should_Validate_HourlyRate_Range(decimal invalidRate)
        {
            // Arrange
            var dto = new CreateBilliardTableDto
            {
                Number = 1,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = invalidRate
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.ShouldContain(vr => vr.MemberNames.Contains(nameof(dto.HourlyRate)));
        }

        [Fact]
        public void CreateBilliardTableDto_Should_Pass_Custom_Business_Validation()
        {
            // Arrange - 斯诺克台球桌价格应该较高
            var dto = new CreateBilliardTableDto
            {
                Number = 1,
                Type = BilliardTableType.Snooker,
                HourlyRate = 50.0m // 太低了
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.ShouldContain(vr => 
                vr.ErrorMessage.Contains("斯诺克台球桌的小时费率不能低于80元"));
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            
            // 自定义验证
            if (model is IValidatableObject validatableObject)
            {
                validationResults.AddRange(validatableObject.Validate(validationContext));
            }
            
            return validationResults;
        }
    }
}
```

## Entity Framework Core 测试模式

### 1. 数据库集成测试

```csharp
// BilliardHallEntityFrameworkCoreTestModule.cs
[DependsOn(
    typeof(BilliardHallApplicationTestModule),
    typeof(BilliardHallEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreTestModule)
)]
public class BilliardHallEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddEntityFrameworkInMemoryDatabase();
        
        var databaseName = Guid.NewGuid().ToString();
        
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseInMemoryDatabase(databaseName);
            });
        });
    }
}

// BilliardHallEntityFrameworkCoreTestBase.cs
namespace Zss.BilliardHall.EntityFrameworkCore.Tests
{
    public abstract class BilliardHallEntityFrameworkCoreTestBase : BilliardHallApplicationTestBase
    {
        protected virtual void UsingDbContext(Action<BilliardHallDbContext> action)
        {
            using (var context = GetRequiredService<BilliardHallDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }

        protected virtual T UsingDbContext<T>(Func<BilliardHallDbContext, T> action)
        {
            using (var context = GetRequiredService<BilliardHallDbContext>())
            {
                var result = action(context);
                context.SaveChanges();
                return result;
            }
        }

        protected virtual async Task UsingDbContextAsync(Func<BilliardHallDbContext, Task> action)
        {
            using (var context = GetRequiredService<BilliardHallDbContext>())
            {
                await action(context);
                await context.SaveChangesAsync();
            }
        }

        protected virtual async Task<T> UsingDbContextAsync<T>(Func<BilliardHallDbContext, Task<T>> action)
        {
            using (var context = GetRequiredService<BilliardHallDbContext>())
            {
                var result = await action(context);
                await context.SaveChangesAsync();
                return result;
            }
        }
    }
}
```

### 2. 仓储测试

```csharp
// BilliardTableRepository_Tests.cs
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.EntityFrameworkCore.Tests
{
    public class BilliardTableRepository_Tests : BilliardHallEntityFrameworkCoreTestBase
    {
        private readonly IBilliardTableRepository _billiardTableRepository;

        public BilliardTableRepository_Tests()
        {
            _billiardTableRepository = GetRequiredService<IBilliardTableRepository>();
        }

        [Fact]
        public async Task Should_Get_Available_Tables()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            
            await CreateTestBilliardTableAsync(hall.Id, 1, status: BilliardTableStatus.Available);
            await CreateTestBilliardTableAsync(hall.Id, 2, status: BilliardTableStatus.Occupied);
            await CreateTestBilliardTableAsync(hall.Id, 3, status: BilliardTableStatus.Available);

            // Act
            var availableTables = await _billiardTableRepository.GetAvailableTablesAsync();

            // Assert
            availableTables.Count.ShouldBe(2);
            availableTables.All(t => t.Status == BilliardTableStatus.Available).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Get_Tables_By_Type()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            
            await CreateTestBilliardTableAsync(hall.Id, 1, BilliardTableType.ChineseEightBall);
            await CreateTestBilliardTableAsync(hall.Id, 2, BilliardTableType.AmericanNineBall);
            await CreateTestBilliardTableAsync(hall.Id, 3, BilliardTableType.ChineseEightBall);

            // Act
            var chineseTables = await _billiardTableRepository.GetAvailableTablesAsync(
                type: BilliardTableType.ChineseEightBall);

            // Assert
            chineseTables.Count.ShouldBe(2);
            chineseTables.All(t => t.Type == BilliardTableType.ChineseEightBall).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Get_Status_Statistics()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            
            await CreateTestBilliardTableAsync(hall.Id, 1, status: BilliardTableStatus.Available);
            await CreateTestBilliardTableAsync(hall.Id, 2, status: BilliardTableStatus.Available);
            await CreateTestBilliardTableAsync(hall.Id, 3, status: BilliardTableStatus.Occupied);
            await CreateTestBilliardTableAsync(hall.Id, 4, status: BilliardTableStatus.Maintenance);

            // Act
            var statistics = await _billiardTableRepository.GetStatusStatisticsAsync(hall.Id);

            // Assert
            statistics[BilliardTableStatus.Available].ShouldBe(2);
            statistics[BilliardTableStatus.Occupied].ShouldBe(1);
            statistics[BilliardTableStatus.Maintenance].ShouldBe(1);
        }

        [Fact]
        public async Task Should_Check_Number_Uniqueness()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            var existingTable = await CreateTestBilliardTableAsync(hall.Id, 1);

            // Act & Assert
            var isUnique1 = await _billiardTableRepository.IsNumberUniqueAsync(1, hall.Id);
            isUnique1.ShouldBeFalse(); // 已存在

            var isUnique2 = await _billiardTableRepository.IsNumberUniqueAsync(2, hall.Id);
            isUnique2.ShouldBeTrue(); // 不存在

            var isUnique3 = await _billiardTableRepository.IsNumberUniqueAsync(1, hall.Id, existingTable.Id);
            isUnique3.ShouldBeTrue(); // 排除自己
        }

        [Fact]
        public async Task Should_Include_Related_Data()
        {
            // Arrange
            var hall = await CreateTestBilliardHallAsync();
            var table = await CreateTestBilliardTableAsync(hall.Id, 1);

            // Act
            var tablesWithHall = await UsingDbContextAsync(async context =>
            {
                return await context.BilliardTables
                    .Include(t => t.BilliardHall)
                    .Where(t => t.Id == table.Id)
                    .ToListAsync();
            });

            // Assert
            var tableWithHall = tablesWithHall.First();
            tableWithHall.BilliardHall.ShouldNotBeNull();
            tableWithHall.BilliardHall.Name.ShouldNotBeNullOrEmpty();
        }
    }
}
```

## 多租户测试模式

### 1. 租户隔离测试

```csharp
// MultiTenant_Tests.cs
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Zss.BilliardHall.Application.Tests
{
    public class MultiTenant_Tests : BilliardHallApplicationTestBase
    {
        [Fact]
        public async Task Should_Isolate_Data_By_Tenant()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            // 租户1的数据
            SetCurrentTenant(tenant1Id);
            var hall1 = await CreateTestBilliardHallAsync("租户1台球厅");
            var table1 = await CreateTestBilliardTableAsync(hall1.Id, 1);

            // 租户2的数据
            SetCurrentTenant(tenant2Id);
            var hall2 = await CreateTestBilliardHallAsync("租户2台球厅");
            var table2 = await CreateTestBilliardTableAsync(hall2.Id, 1);

            // Act & Assert - 租户1只能看到自己的数据
            SetCurrentTenant(tenant1Id);
            var tenant1Tables = await BilliardTableAppService.GetListAsync(new GetBilliardTablesInput());
            tenant1Tables.TotalCount.ShouldBe(1);
            tenant1Tables.Items.First().Id.ShouldBe(table1.Id);

            // Act & Assert - 租户2只能看到自己的数据
            SetCurrentTenant(tenant2Id);
            var tenant2Tables = await BilliardTableAppService.GetListAsync(new GetBilliardTablesInput());
            tenant2Tables.TotalCount.ShouldBe(1);
            tenant2Tables.Items.First().Id.ShouldBe(table2.Id);
        }

        [Fact]
        public async Task Should_Not_Access_Other_Tenant_Data()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            SetCurrentTenant(tenant1Id);
            var table1 = await CreateTestBilliardTableAsync();

            // Act & Assert - 租户2不能访问租户1的数据
            SetCurrentTenant(tenant2Id);
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await BilliardTableAppService.GetAsync(table1.Id);
            });
        }

        [Fact]
        public async Task Should_Allow_Host_Access_All_Tenant_Data()
        {
            // Arrange
            var tenant1Id = Guid.NewGuid();
            
            SetCurrentTenant(tenant1Id);
            var table1 = await CreateTestBilliardTableAsync();

            // Act - Host 用户 (null tenant)
            SetCurrentTenant(null);
            
            // 使用 DataFilter 访问所有租户数据
            using (DataFilter.Disable<IMultiTenant>())
            {
                var allTables = await BilliardTableAppService.GetListAsync(new GetBilliardTablesInput());
                
                // Assert
                allTables.TotalCount.ShouldBeGreaterThan(0);
            }
        }
    }
}
```

### 2. 权限测试

```csharp
// Permission_Tests.cs
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Authorization;
using Xunit;

namespace Zss.BilliardHall.Application.Tests
{
    public class Permission_Tests : BilliardHallApplicationTestBase
    {
        [Fact]
        public async Task Should_Not_Allow_Create_Without_Permission()
        {
            // Arrange - 移除创建权限
            var permissionChecker = GetRequiredService<IPermissionChecker>();
            
            // 模拟没有权限的用户
            SetCurrentUser(Guid.NewGuid(), "no-permission-user");

            var input = new CreateBilliardTableDto
            {
                Number = 1,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = 50.0m
            };

            // Act & Assert
            await Should.ThrowAsync<AbpAuthorizationException>(async () =>
            {
                await BilliardTableAppService.CreateAsync(input);
            });
        }

        [Fact]
        public async Task Should_Allow_Read_With_Default_Permission()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();
            SetCurrentUser(Guid.NewGuid(), "read-only-user");

            // Act - 读取权限通常是默认允许的
            var result = await BilliardTableAppService.GetAsync(table.Id);

            // Assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Not_Allow_Status_Change_Without_ManageStatus_Permission()
        {
            // Arrange
            var table = await CreateTestBilliardTableAsync();
            SetCurrentUser(Guid.NewGuid(), "basic-user");

            var input = new ChangeBilliardTableStatusDto
            {
                Status = BilliardTableStatus.Maintenance,
                Reason = "测试"
            };

            // Act & Assert
            await Should.ThrowAsync<AbpAuthorizationException>(async () =>
            {
                await BilliardTableAppService.ChangeStatusAsync(table.Id, input);
            });
        }
    }
}
```

## 集成测试模式

### 1. HTTP API 集成测试

```csharp
// BilliardTableController_Integration_Tests.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.HttpApi.Tests
{
    public class BilliardTableController_Integration_Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BilliardTableController_Integration_Tests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Get_BilliardTables_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/api/app/billiard-table");

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task Post_BilliardTable_Should_Create_Successfully()
        {
            // Arrange
            var createDto = new CreateBilliardTableDto
            {
                Number = 99,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = 50.0m,
                LocationX = 100.0f,
                LocationY = 200.0f
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/app/billiard-table", content);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BilliardTableDto>(responseContent);
            
            result.ShouldNotBeNull();
            result.Number.ShouldBe(createDto.Number);
        }
    }
}
```

### 2. 端到端测试

```csharp
// End_To_End_Tests.cs
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Zss.BilliardHall.Application.Tests
{
    public class End_To_End_Tests : BilliardHallApplicationTestBase
    {
        [Fact]
        public async Task Complete_Table_Lifecycle_Should_Work()
        {
            // 1. 创建台球桌
            var createInput = new CreateBilliardTableDto
            {
                Number = 1,
                Type = BilliardTableType.ChineseEightBall,
                HourlyRate = 50.0m,
                LocationX = 100.0f,
                LocationY = 200.0f
            };

            var createdTable = await BilliardTableAppService.CreateAsync(createInput);
            createdTable.ShouldNotBeNull();
            createdTable.Status.ShouldBe(BilliardTableStatus.Available);

            // 2. 获取台球桌列表
            var listResult = await BilliardTableAppService.GetListAsync(new GetBilliardTablesInput());
            listResult.Items.ShouldContain(t => t.Id == createdTable.Id);

            // 3. 更新台球桌
            var updateInput = new UpdateBilliardTableDto
            {
                HourlyRate = 60.0m,
                LocationX = 150.0f
            };

            var updatedTable = await BilliardTableAppService.UpdateAsync(createdTable.Id, updateInput);
            updatedTable.HourlyRate.ShouldBe(60.0m);
            updatedTable.LocationX.ShouldBe(150.0f);

            // 4. 更改状态
            var statusInput = new ChangeBilliardTableStatusDto
            {
                Status = BilliardTableStatus.Maintenance,
                Reason = "定期维护"
            };

            var statusUpdatedTable = await BilliardTableAppService.ChangeStatusAsync(createdTable.Id, statusInput);
            statusUpdatedTable.Status.ShouldBe(BilliardTableStatus.Maintenance);

            // 5. 删除台球桌
            await BilliardTableAppService.DeleteAsync(createdTable.Id);

            // 6. 验证删除
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await BilliardTableAppService.GetAsync(createdTable.Id);
            });
        }

        [Fact]
        public async Task Reservation_Workflow_Should_Work()
        {
            // 1. 准备台球桌
            var table = await CreateTestBilliardTableAsync();

            // 2. 创建预约
            var reservation = new CreateReservationDto
            {
                BilliardTableId = table.Id,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(3),
                CustomerNotes = "生日聚会"
            };

            var createdReservation = await ReservationAppService.CreateAsync(reservation);

            // 3. 验证台球桌状态变更
            var reservedTable = await BilliardTableAppService.GetAsync(table.Id);
            reservedTable.Status.ShouldBe(BilliardTableStatus.Reserved);

            // 4. 开始使用 (签到)
            await ReservationAppService.CheckInAsync(createdReservation.Id);

            // 5. 验证状态变更
            var occupiedTable = await BilliardTableAppService.GetAsync(table.Id);
            occupiedTable.Status.ShouldBe(BilliardTableStatus.Occupied);

            // 6. 结束使用 (签出)
            await ReservationAppService.CheckOutAsync(createdReservation.Id);

            // 7. 验证台球桌恢复可用
            var availableTable = await BilliardTableAppService.GetAsync(table.Id);
            availableTable.Status.ShouldBe(BilliardTableStatus.Available);
        }
    }
}
```

## 测试工具和辅助方法

### 1. 测试数据构建器

```csharp
// TestDataBuilder.cs
namespace Zss.BilliardHall.TestBase
{
    public class BilliardTableTestDataBuilder
    {
        private int _number = 1;
        private BilliardTableType _type = BilliardTableType.ChineseEightBall;
        private decimal _hourlyRate = 50.0m;
        private float _locationX = 0.0f;
        private float _locationY = 0.0f;
        private BilliardTableStatus _status = BilliardTableStatus.Available;

        public BilliardTableTestDataBuilder WithNumber(int number)
        {
            _number = number;
            return this;
        }

        public BilliardTableTestDataBuilder WithType(BilliardTableType type)
        {
            _type = type;
            return this;
        }

        public BilliardTableTestDataBuilder WithHourlyRate(decimal rate)
        {
            _hourlyRate = rate;
            return this;
        }

        public BilliardTableTestDataBuilder WithLocation(float x, float y)
        {
            _locationX = x;
            _locationY = y;
            return this;
        }

        public BilliardTableTestDataBuilder WithStatus(BilliardTableStatus status)
        {
            _status = status;
            return this;
        }

        public BilliardTable Build(Guid? id = null, Guid? hallId = null)
        {
            var table = new BilliardTable(
                id ?? Guid.NewGuid(),
                hallId ?? Guid.NewGuid(),
                _number,
                _type,
                _hourlyRate,
                _locationX,
                _locationY
            );

            if (_status != BilliardTableStatus.Available)
            {
                table.ChangeStatus(_status);
            }

            return table;
        }

        public CreateBilliardTableDto BuildCreateDto()
        {
            return new CreateBilliardTableDto
            {
                Number = _number,
                Type = _type,
                HourlyRate = _hourlyRate,
                LocationX = _locationX,
                LocationY = _locationY
            };
        }
    }

    // 使用示例
    public class SampleTest : BilliardHallDomainTestBase
    {
        [Fact]
        public async Task Sample_Test_With_Builder()
        {
            // Arrange
            var table = new BilliardTableTestDataBuilder()
                .WithNumber(5)
                .WithType(BilliardTableType.Snooker)
                .WithHourlyRate(80.0m)
                .WithLocation(100.0f, 200.0f)
                .Build();

            // Act & Assert
            table.Number.ShouldBe(5);
            table.Type.ShouldBe(BilliardTableType.Snooker);
        }
    }
}
```

### 2. 断言扩展

```csharp
// TestExtensions.cs
namespace Zss.BilliardHall.TestBase
{
    public static class BilliardTableAssertions
    {
        public static void ShouldBeValidBilliardTable(this BilliardTableDto table)
        {
            table.ShouldNotBeNull();
            table.Id.ShouldNotBe(Guid.Empty);
            table.Number.ShouldBeGreaterThan(0);
            table.HourlyRate.ShouldBeGreaterThan(0);
            table.CreationTime.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        public static void ShouldBeEquivalentTo(this BilliardTableDto actual, CreateBilliardTableDto expected)
        {
            actual.Number.ShouldBe(expected.Number);
            actual.Type.ShouldBe(expected.Type);
            actual.HourlyRate.ShouldBe(expected.HourlyRate);
            actual.LocationX.ShouldBe(expected.LocationX);
            actual.LocationY.ShouldBe(expected.LocationY);
        }

        public static void ShouldHaveValidAuditFields(this BilliardTableDto table)
        {
            table.CreationTime.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            
            if (table.LastModificationTime.HasValue)
            {
                table.LastModificationTime.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
                table.LastModificationTime.Value.ShouldBeGreaterThanOrEqualTo(table.CreationTime);
            }
        }
    }
}
```

这些测试模式确保了完整覆盖 ABP 框架的各个层面，从领域层的业务规则到应用层的服务集成，再到数据访问层的持久化逻辑，为台球厅管理系统提供了可靠的质量保证。