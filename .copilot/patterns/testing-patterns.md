# 测试模式 (Testing Patterns)

## 测试策略 (Testing Strategy)

### 测试金字塔 (Test Pyramid)

```
        /\
       /  \  E2E Tests (少量)
      /____\
     /      \  Integration Tests (适量) 
    /________\
   /          \  Unit Tests (大量)
  /__________\
```

### 测试分类和覆盖目标

```yaml
test_coverage_targets:
  unit_tests:
    coverage: 90%
    focus: "业务逻辑、领域模型、服务层"
    
  integration_tests:
    coverage: 70%
    focus: "数据访问、外部服务、API端点"
    
  e2e_tests:
    coverage: 30%
    focus: "关键业务流程、用户场景"
```

## 单元测试模式 (Unit Test Patterns)

### 1. 实体测试 (Entity Tests)

```csharp
[TestClass]
public class BilliardTableTests
{
    [TestMethod]
    public void Constructor_ValidInputs_CreatesBilliardTable()
    {
        // Arrange
        var number = 5;
        var type = TableType.Chinese8Ball;
        var hourlyRate = new Money(35.00m, "CNY");
        var location = new TableLocation(10.5f, 5.2f, 1, "A");
        var hallId = Guid.NewGuid();

        // Act
        var table = new BilliardTable(number, type, hourlyRate, location, hallId);

        // Assert
        Assert.AreEqual(number, table.Number);
        Assert.AreEqual(type, table.Type);
        Assert.AreEqual(TableStatus.Available, table.Status);
        Assert.AreEqual(hourlyRate, table.HourlyRate);
        Assert.AreEqual(location, table.Location);
        Assert.AreEqual(hallId, table.HallId);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_InvalidNumber_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        new BilliardTable(-1, TableType.Chinese8Ball, 
                          new Money(35, "CNY"), 
                          new TableLocation(1, 1, 1, "A"), 
                          Guid.NewGuid());
    }

    [TestMethod]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var table = CreateValidBilliardTable();
        var newStatus = TableStatus.Occupied;
        var reason = "客户开始使用";

        // Act
        table.UpdateStatus(newStatus, reason);

        // Assert
        Assert.AreEqual(newStatus, table.Status);
        Assert.IsTrue(table.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateStatus_InvalidTransition_ThrowsException()
    {
        // Arrange
        var table = CreateValidBilliardTable();
        table.UpdateStatus(TableStatus.Occupied);

        // Act & Assert - 不能从 Occupied 直接转换到 Reserved
        table.UpdateStatus(TableStatus.Reserved);
    }

    private BilliardTable CreateValidBilliardTable()
    {
        return new BilliardTable(
            1, 
            TableType.Chinese8Ball, 
            new Money(35, "CNY"),
            new TableLocation(1, 1, 1, "A"),
            Guid.NewGuid()
        );
    }
}
```

### 2. 服务层测试 (Service Layer Tests)

```csharp
[TestClass]
public class BilliardTableServiceTests
{
    private Mock<IBilliardTableRepository> _mockRepository;
    private Mock<IValidator<CreateBilliardTableDto>> _mockValidator;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<BilliardTableService>> _mockLogger;
    private BilliardTableService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IBilliardTableRepository>();
        _mockValidator = new Mock<IValidator<CreateBilliardTableDto>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<BilliardTableService>>();

        _service = new BilliardTableService(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [TestMethod]
    public async Task CreateTableAsync_ValidInput_ReturnsCreatedTable()
    {
        // Arrange
        var dto = CreateValidCreateTableDto();
        var entity = CreateValidBilliardTable();
        var expectedDto = CreateValidTableDto();

        _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                     .ReturnsAsync(new ValidationResult());
        
        _mockRepository.Setup(r => r.GetByNumberAsync(dto.Number, dto.HallId))
                      .ReturnsAsync((BilliardTable)null);
        
        _mockRepository.Setup(r => r.GetByLocationAsync(dto.LocationX, dto.LocationY, dto.Floor, dto.HallId))
                      .ReturnsAsync((BilliardTable)null);
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<BilliardTable>()))
                      .ReturnsAsync(entity);
        
        _mockMapper.Setup(m => m.Map<BilliardTableDto>(entity))
                  .Returns(expectedDto);

        // Act
        var result = await _service.CreateTableAsync(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<BilliardTable>()), Times.Once);
        _mockValidator.Verify(v => v.ValidateAsync(dto, default), Times.Once);
    }

    [TestMethod]
    public async Task CreateTableAsync_DuplicateNumber_ThrowsBusinessRuleException()
    {
        // Arrange
        var dto = CreateValidCreateTableDto();
        var existingTable = CreateValidBilliardTable();

        _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                     .ReturnsAsync(new ValidationResult());
        
        _mockRepository.Setup(r => r.GetByNumberAsync(dto.Number, dto.HallId))
                      .ReturnsAsync(existingTable);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateTableAsync(dto));

        Assert.AreEqual($"台球桌编号 {dto.Number} 已存在", exception.Message);
    }

    [TestMethod]
    public async Task CreateTableAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var dto = CreateValidCreateTableDto();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Number", "编号不能为空")
        });

        _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                     .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.CreateTableAsync(dto));

        Assert.AreEqual("输入数据验证失败", exception.Message);
        Assert.IsTrue(exception.Errors.Any(e => e.PropertyName == "Number"));
    }

    private CreateBilliardTableDto CreateValidCreateTableDto()
    {
        return new CreateBilliardTableDto
        {
            Number = 1,
            Type = TableType.Chinese8Ball,
            HourlyRate = 35.00m,
            LocationX = 10.5f,
            LocationY = 5.2f,
            Floor = 1,
            Zone = "A",
            HallId = Guid.NewGuid()
        };
    }

    private BilliardTable CreateValidBilliardTable()
    {
        return new BilliardTable(1, TableType.Chinese8Ball, 
                                new Money(35, "CNY"),
                                new TableLocation(1, 1, 1, "A"),
                                Guid.NewGuid());
    }

    private BilliardTableDto CreateValidTableDto()
    {
        return new BilliardTableDto
        {
            Id = Guid.NewGuid(),
            Number = 1,
            Type = "Chinese8Ball",
            Status = "Available"
        };
    }
}
```

## 集成测试模式 (Integration Test Patterns)

### 1. 仓储层集成测试

```csharp
[TestClass]
public class BilliardTableRepositoryIntegrationTests
{
    private BilliardHallDbContext _context;
    private BilliardTableRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BilliardHallDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BilliardHallDbContext(options);
        _repository = new BilliardTableRepository(_context);

        // 初始化测试数据
        SeedTestData();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task CreateAsync_ValidTable_SavesSuccessfully()
    {
        // Arrange
        var hallId = await CreateTestHall();
        var table = new BilliardTable(10, TableType.Chinese8Ball, 
                                     new Money(40, "CNY"),
                                     new TableLocation(15, 10, 1, "B"),
                                     hallId);

        // Act
        var result = await _repository.CreateAsync(table);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Id);

        var savedTable = await _context.BilliardTables.FindAsync(result.Id);
        Assert.IsNotNull(savedTable);
        Assert.AreEqual(10, savedTable.Number);
    }

    [TestMethod]
    public async Task GetByNumberAsync_ExistingTable_ReturnsTable()
    {
        // Arrange
        var hallId = await CreateTestHall();
        var expectedNumber = 5;

        // Act
        var result = await _repository.GetByNumberAsync(expectedNumber, hallId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedNumber, result.Number);
    }

    [TestMethod]
    public async Task GetPagedAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var hallId = await CreateTestHall();
        var filter = new BilliardTableQuery
        {
            HallId = hallId,
            Status = TableStatus.Available,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.All(t => t.Status == TableStatus.Available));
        Assert.IsTrue(result.Items.All(t => t.HallId == hallId));
    }

    private async Task<Guid> CreateTestHall()
    {
        var hall = new BilliardHall
        {
            Id = Guid.NewGuid(),
            Name = "测试台球厅",
            Address = new Address("测试街道", "测试城市", "测试省份", "100000"),
            OperatingHours = new OperatingHours(),
            Status = HallStatus.Active
        };

        _context.BilliardHalls.Add(hall);
        await _context.SaveChangesAsync();
        return hall.Id;
    }

    private void SeedTestData()
    {
        // 预置一些测试数据
        var hallId = Guid.NewGuid();
        var hall = new BilliardHall
        {
            Id = hallId,
            Name = "测试台球厅",
            Address = new Address("测试街道", "测试城市", "测试省份", "100000"),
            OperatingHours = new OperatingHours(),
            Status = HallStatus.Active
        };

        var tables = new[]
        {
            new BilliardTable(1, TableType.Chinese8Ball, new Money(35, "CNY"), new TableLocation(1, 1, 1, "A"), hallId),
            new BilliardTable(2, TableType.American9Ball, new Money(40, "CNY"), new TableLocation(2, 1, 1, "A"), hallId),
            new BilliardTable(5, TableType.Snooker, new Money(50, "CNY"), new TableLocation(5, 1, 1, "A"), hallId)
        };

        _context.BilliardHalls.Add(hall);
        _context.BilliardTables.AddRange(tables);
        _context.SaveChanges();
    }
}
```

### 2. API 集成测试

```csharp
[TestClass]
public class BilliardTablesControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 替换数据库为测试数据库
                    services.RemoveAll(typeof(DbContextOptions<BilliardHallDbContext>));
                    services.AddDbContext<BilliardHallDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });

        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetTables_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        await SeedTestData();

        // Act
        var response = await _client.GetAsync("/api/v1/billiard-tables?page=1&pageSize=10");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<BilliardTableListDto>>>(content);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data.Items.Count > 0);
    }

    [TestMethod]
    public async Task CreateTable_ValidInput_ReturnsCreated()
    {
        // Arrange
        var hallId = await CreateTestHall();
        var dto = new CreateBilliardTableDto
        {
            Number = 10,
            Type = TableType.Chinese8Ball,
            HourlyRate = 35.00m,
            LocationX = 10.5f,
            LocationY = 5.2f,
            Floor = 1,
            Zone = "A",
            HallId = hallId
        };

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/billiard-tables", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<BilliardTableDto>>(responseContent);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.Number);
    }

    [TestMethod]
    public async Task CreateTable_InvalidInput_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateBilliardTableDto
        {
            Number = -1, // 无效编号
            Type = TableType.Chinese8Ball,
            HourlyRate = 35.00m,
            HallId = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/billiard-tables", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BilliardHallDbContext>();

        var hallId = Guid.NewGuid();
        var hall = new BilliardHall
        {
            Id = hallId,
            Name = "测试台球厅",
            Address = new Address("测试街道", "测试城市", "测试省份", "100000"),
            OperatingHours = new OperatingHours(),
            Status = HallStatus.Active
        };

        var tables = new[]
        {
            new BilliardTable(1, TableType.Chinese8Ball, new Money(35, "CNY"), new TableLocation(1, 1, 1, "A"), hallId),
            new BilliardTable(2, TableType.American9Ball, new Money(40, "CNY"), new TableLocation(2, 1, 1, "A"), hallId)
        };

        context.BilliardHalls.Add(hall);
        context.BilliardTables.AddRange(tables);
        await context.SaveChangesAsync();
    }

    private async Task<Guid> CreateTestHall()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BilliardHallDbContext>();

        var hall = new BilliardHall
        {
            Id = Guid.NewGuid(),
            Name = "测试台球厅",
            Address = new Address("测试街道", "测试城市", "测试省份", "100000"),
            OperatingHours = new OperatingHours(),
            Status = HallStatus.Active
        };

        context.BilliardHalls.Add(hall);
        await context.SaveChangesAsync();
        return hall.Id;
    }
}
```

## 端到端测试模式 (E2E Test Patterns)

### 1. 用户场景测试

```csharp
[TestClass]
public class ReservationE2ETests
{
    private IWebDriver _driver;
    private WebApplicationFactory<Program> _factory;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        
        var options = new ChromeOptions();
        options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");
        _driver = new ChromeDriver(options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task CompleteReservationFlow_ValidUser_Success()
    {
        // Arrange
        var baseUrl = _factory.GetServerAddress();
        
        // Act & Assert
        // 1. 用户登录
        _driver.Navigate().GoToUrl($"{baseUrl}/login");
        var usernameField = _driver.FindElement(By.Id("username"));
        var passwordField = _driver.FindElement(By.Id("password"));
        var loginButton = _driver.FindElement(By.Id("login-button"));

        usernameField.SendKeys("testuser@example.com");
        passwordField.SendKeys("TestPassword123");
        loginButton.Click();

        // 2. 选择台球桌
        _driver.Navigate().GoToUrl($"{baseUrl}/tables");
        var availableTable = _driver.FindElement(By.CssSelector("[data-status='Available']:first-child"));
        availableTable.Click();

        // 3. 创建预约
        var reserveButton = _driver.FindElement(By.Id("reserve-button"));
        reserveButton.Click();

        var startTimeField = _driver.FindElement(By.Id("start-time"));
        var durationField = _driver.FindElement(By.Id("duration"));
        var confirmButton = _driver.FindElement(By.Id("confirm-reservation"));

        startTimeField.SendKeys(DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm"));
        durationField.SendKeys("120"); // 2小时
        confirmButton.Click();

        // 4. 验证预约成功
        var successMessage = _driver.FindElement(By.CssSelector(".success-message"));
        Assert.IsTrue(successMessage.Displayed);
        Assert.IsTrue(successMessage.Text.Contains("预约成功"));

        // 5. 验证预约出现在用户预约列表
        _driver.Navigate().GoToUrl($"{baseUrl}/my-reservations");
        var reservationItems = _driver.FindElements(By.CssSelector(".reservation-item"));
        Assert.IsTrue(reservationItems.Count > 0);
    }
}
```

## 性能测试模式 (Performance Test Patterns)

### 1. 负载测试

```csharp
[TestClass]
public class BilliardTableServicePerformanceTests
{
    private BilliardTableService _service;
    private Mock<IBilliardTableRepository> _mockRepository;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IBilliardTableRepository>();
        // 设置其他必要的 mocks...
        _service = new BilliardTableService(/*...*/);
    }

    [TestMethod]
    public async Task GetTablesAsync_HighVolume_CompletesWithinTimeout()
    {
        // Arrange
        var query = new BilliardTableQuery { Page = 1, PageSize = 100 };
        var tables = GenerateTestTables(100);
        
        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<BilliardTableQuery>()))
                      .ReturnsAsync(new PagedResult<BilliardTable> { Items = tables });

        var stopwatch = Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        var tasks = Enumerable.Range(0, iterations)
            .Select(_ => _service.GetTablesAsync(query))
            .ToArray();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                     $"操作耗时 {stopwatch.ElapsedMilliseconds}ms，超过预期的 5000ms");
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
        Assert.IsTrue(averageTime < 5, 
                     $"平均响应时间 {averageTime}ms，超过预期的 5ms");
    }

    [TestMethod]
    public async Task CreateTableAsync_ConcurrentRequests_HandlesCorrectly()
    {
        // Arrange
        const int concurrentRequests = 50;
        var dtos = Enumerable.Range(1, concurrentRequests)
            .Select(i => new CreateBilliardTableDto 
            { 
                Number = i,
                Type = TableType.Chinese8Ball,
                HallId = Guid.NewGuid(),
                HourlyRate = 35.00m
            })
            .ToList();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<BilliardTable>()))
                      .Returns((BilliardTable table) => Task.FromResult(table));

        // Act
        var tasks = dtos.Select(dto => _service.CreateTableAsync(dto)).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(concurrentRequests, results.Length);
        Assert.IsTrue(results.All(r => r != null));
    }

    private List<BilliardTable> GenerateTestTables(int count)
    {
        var hallId = Guid.NewGuid();
        return Enumerable.Range(1, count)
            .Select(i => new BilliardTable(i, TableType.Chinese8Ball, 
                                          new Money(35, "CNY"),
                                          new TableLocation(i, 1, 1, "A"),
                                          hallId))
            .ToList();
    }
}
```

### 2. 内存使用测试

```csharp
[TestMethod]
public async Task GetTablesAsync_LargeDataSet_DoesNotExceedMemoryLimit()
{
    // Arrange
    const int largeDataSetSize = 10000;
    var initialMemory = GC.GetTotalMemory(true);
    
    var query = new BilliardTableQuery { Page = 1, PageSize = largeDataSetSize };
    var tables = GenerateTestTables(largeDataSetSize);
    
    _mockRepository.Setup(r => r.GetPagedAsync(query))
                  .ReturnsAsync(new PagedResult<BilliardTable> { Items = tables });

    // Act
    var result = await _service.GetTablesAsync(query);

    // Assert
    var finalMemory = GC.GetTotalMemory(true);
    var memoryIncrease = finalMemory - initialMemory;
    var memoryLimitMB = 50; // 50MB 限制
    
    Assert.IsTrue(memoryIncrease < memoryLimitMB * 1024 * 1024,
                 $"内存增长 {memoryIncrease / 1024 / 1024}MB，超过限制 {memoryLimitMB}MB");
}
```

## 测试工具和辅助类 (Test Utilities)

### 1. 测试数据构建器

```csharp
public class BilliardTableTestBuilder
{
    private int _number = 1;
    private TableType _type = TableType.Chinese8Ball;
    private TableStatus _status = TableStatus.Available;
    private Money _hourlyRate = new Money(35, "CNY");
    private TableLocation _location = new TableLocation(1, 1, 1, "A");
    private Guid _hallId = Guid.NewGuid();

    public BilliardTableTestBuilder WithNumber(int number)
    {
        _number = number;
        return this;
    }

    public BilliardTableTestBuilder WithType(TableType type)
    {
        _type = type;
        return this;
    }

    public BilliardTableTestBuilder WithStatus(TableStatus status)
    {
        _status = status;
        return this;
    }

    public BilliardTableTestBuilder WithHourlyRate(decimal amount, string currency = "CNY")
    {
        _hourlyRate = new Money(amount, currency);
        return this;
    }

    public BilliardTableTestBuilder WithLocation(float x, float y, int floor = 1, string zone = "A")
    {
        _location = new TableLocation(x, y, floor, zone);
        return this;
    }

    public BilliardTableTestBuilder WithHallId(Guid hallId)
    {
        _hallId = hallId;
        return this;
    }

    public BilliardTable Build()
    {
        var table = new BilliardTable(_number, _type, _hourlyRate, _location, _hallId);
        if (_status != TableStatus.Available)
        {
            table.UpdateStatus(_status);
        }
        return table;
    }

    public static BilliardTableTestBuilder Create()
    {
        return new BilliardTableTestBuilder();
    }
}

// 使用示例
[TestMethod]
public void TestExample()
{
    var table = BilliardTableTestBuilder.Create()
        .WithNumber(5)
        .WithType(TableType.Snooker)
        .WithHourlyRate(50)
        .Build();
        
    // 测试逻辑...
}
```

### 2. 断言扩展

```csharp
public static class AssertExtensions
{
    public static void ShouldBeValidBilliardTable(this BilliardTable table)
    {
        Assert.IsNotNull(table);
        Assert.AreNotEqual(Guid.Empty, table.Id);
        Assert.IsTrue(table.Number > 0);
        Assert.IsNotNull(table.HourlyRate);
        Assert.IsTrue(table.HourlyRate.Amount > 0);
    }

    public static void ShouldBeValidApiResponse<T>(this ApiResponse<T> response)
    {
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.IsTrue(response.Timestamp > DateTime.UtcNow.AddMinutes(-1));
    }

    public static void ShouldBeValidPagedResult<T>(this PagedResult<T> result, int expectedMinCount = 0)
    {
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Items);
        Assert.IsTrue(result.Items.Count >= expectedMinCount);
        Assert.IsNotNull(result.Pagination);
        Assert.IsTrue(result.Pagination.TotalItems >= result.Items.Count);
    }
}
```

### 3. 测试配置管理

```csharp
public static class TestConfiguration
{
    public static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static DbContextOptions<BilliardHallDbContext> GetInMemoryDbOptions()
    {
        return new DbContextOptionsBuilder<BilliardHallDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public static DbContextOptions<BilliardHallDbContext> GetSqliteDbOptions()
    {
        var connectionString = $"DataSource=:memory:";
        return new DbContextOptionsBuilder<BilliardHallDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }
}
```

## 测试自动化 (Test Automation)

### 1. 测试运行脚本

```bash
#!/bin/bash
# scripts/run-all-tests.sh

echo "🧪 运行完整测试套件..."

# 环境变量
export ASPNETCORE_ENVIRONMENT=Test

# 单元测试
echo "📝 运行单元测试..."
dotnet test --filter "Category=Unit" \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger "trx;LogFileName=unit-tests.trx" \
    || exit 1

# 集成测试
echo "🔗 运行集成测试..."
dotnet test --filter "Category=Integration" \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger "trx;LogFileName=integration-tests.trx" \
    || exit 1

# 性能测试
echo "⚡ 运行性能测试..."
dotnet test --filter "Category=Performance" \
    --logger "trx;LogFileName=performance-tests.trx" \
    || exit 1

# 生成覆盖率报告
echo "📊 生成覆盖率报告..."
reportgenerator \
    -reports:"TestResults/**/coverage.cobertura.xml" \
    -targetdir:"TestResults/Coverage" \
    -reporttypes:"Html;Cobertura;SonarQube"

# 检查覆盖率阈值
coverage_threshold=80
coverage=$(grep -oP 'line-rate="\K[^"]*' TestResults/Coverage/Cobertura.xml | head -1)
coverage_percent=$(echo "$coverage * 100" | bc | cut -d. -f1)

if [ $coverage_percent -lt $coverage_threshold ]; then
    echo "❌ 代码覆盖率 $coverage_percent% 低于阈值 $coverage_threshold%"
    exit 1
fi

echo "✅ 所有测试通过，覆盖率: $coverage_percent%"
```

### 2. 持续集成配置

```yaml
# .github/workflows/test.yml
name: Test Suite

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: TestPassword123!
          ACCEPT_EULA: Y
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P TestPassword123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 3
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Run Unit Tests
      run: dotnet test --no-build --filter "Category=Unit" --collect:"XPlat Code Coverage"
      
    - name: Run Integration Tests
      run: dotnet test --no-build --filter "Category=Integration" --collect:"XPlat Code Coverage"
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost,1433;Database=BilliardHall_Test;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true"
        
    - name: Generate Coverage Report
      run: |
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:"Html;Cobertura"
        
    - name: Upload Coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage/Cobertura.xml
```

---

> 以上测试模式和实践应在所有测试开发中严格遵循，确保代码质量和系统稳定性。