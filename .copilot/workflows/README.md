# 工作流和自动化 (Workflows and Automation)

## 概述 (Overview)

本目录包含智慧台球厅管理系统的各种工作流定义，支持开发、测试、部署和维护的全流程自动化。这些工作流设计遵循"机器可读优先、人机混合协作、流程自动化"的原则。

## 工作流类型 (Workflow Types)

### 1. 开发工作流 (Development Workflow)
- 代码生成和脚手架
- 本地开发环境配置
- 代码质量检查
- 单元测试执行

### 2. 测试工作流 (Testing Workflow)
- 自动化测试执行
- 集成测试流程
- 性能测试
- 安全扫描

### 3. 部署工作流 (Deployment Workflow)
- CI/CD 流水线
- 环境管理
- 发布流程
- 回滚机制

### 4. 维护工作流 (Maintenance Workflow)
- 监控和告警
- 数据备份
- 性能优化
- 问题诊断

## GitHub Actions 集成

### 基础工作流配置

```yaml
# .github/workflows/ci.yml
name: Continuous Integration

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '18.x'

jobs:
  build-and-test:
    name: Build and Test
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
        
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build solution
      run: dotnet build --no-restore
      
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
```

### 代码质量检查

```yaml
# .github/workflows/code-quality.yml
name: Code Quality

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  code-analysis:
    name: Code Analysis
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
        
    - name: SonarCloud Scan
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        
  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Run Snyk security scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
```

### 自动化部署

```yaml
# .github/workflows/deploy.yml
name: Deploy to Environment

on:
  push:
    branches: [ main ]
    tags: [ 'v*' ]

jobs:
  deploy-staging:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    environment:
      name: staging
      url: https://staging-api.billiard-hall.com
      
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Build Docker image
      run: |
        docker build -t billiard-hall-api:${{ github.sha }} .
        
    - name: Deploy to staging
      run: |
        # 部署逻辑
        echo "Deploying to staging environment"
        
  deploy-production:
    name: Deploy to Production
    runs-on: ubuntu-latest
    if: startsWith(github.ref, 'refs/tags/v')
    
    environment:
      name: production
      url: https://api.billiard-hall.com
      
    needs: [ build-and-test ]
    
    steps:
    - name: Deploy to production
      run: |
        # 生产部署逻辑
        echo "Deploying to production environment"
```

## 本地开发自动化

### 开发环境脚本

```bash
#!/bin/bash
# scripts/setup-dev.sh

set -e

echo "🏗️  设置台球厅管理系统开发环境..."

# 检查必要工具
check_tool() {
    if ! command -v $1 &> /dev/null; then
        echo "❌ $1 未安装，请先安装"
        exit 1
    fi
    echo "✅ $1 已安装"
}

check_tool "dotnet"
check_tool "node"
check_tool "docker"
check_tool "git"

# 安装 .NET 依赖
echo "📦 安装 .NET 依赖..."
dotnet restore

# 安装前端依赖
echo "📦 安装前端依赖..."
npm install

# 数据库迁移
echo "🗄️  执行数据库迁移..."
dotnet ef database update

# 生成开发证书
echo "🔐 生成开发证书..."
dotnet dev-certs https --trust

# 启动开发服务
echo "🚀 启动开发服务..."
dotnet run --project src/Zss.BilliardHall.Api &
npm run dev &

echo "✅ 开发环境设置完成!"
echo "📖 API 文档: https://localhost:5001/swagger"
echo "🖥️  前端应用: https://localhost:3000"
```

### 代码生成脚本

```bash
#!/bin/bash
# scripts/generate-code.sh

ENTITY_NAME=$1
if [ -z "$ENTITY_NAME" ]; then
    echo "用法: $0 <实体名称>"
    echo "示例: $0 Customer"
    exit 1
fi

echo "🏗️  为实体 $ENTITY_NAME 生成代码..."

# 创建目录结构
mkdir -p "src/Zss.BilliardHall.Domain/Entities"
mkdir -p "src/Zss.BilliardHall.Application/DTOs/$ENTITY_NAME"
mkdir -p "src/Zss.BilliardHall.Application/Services"
mkdir -p "src/Zss.BilliardHall.Infrastructure/Repositories"
mkdir -p "src/Zss.BilliardHall.Api/Controllers"
mkdir -p "tests/Zss.BilliardHall.Application.Tests/Services"

# 使用模板生成代码文件
# (这里可以集成更复杂的代码生成器)

echo "✅ $ENTITY_NAME 相关代码生成完成!"
```

## 测试自动化流程

### 测试分类和执行策略

```yaml
# 测试配置 (test-config.yml)
test_categories:
  unit:
    pattern: "**/*Tests.cs"
    timeout: 300
    parallel: true
    coverage_threshold: 80
    
  integration:
    pattern: "**/*IntegrationTests.cs"
    timeout: 600
    parallel: false
    requires_database: true
    
  performance:
    pattern: "**/*PerformanceTests.cs"
    timeout: 1800
    parallel: false
    requires_load_data: true
    
  e2e:
    pattern: "**/*E2ETests.cs"
    timeout: 3600
    parallel: false
    requires_full_environment: true

environments:
  test:
    database_connection: "Server=(localdb)\\mssqllocaldb;Database=BilliardHall_Test"
    redis_connection: "localhost:6379,db=1"
    
  ci:
    database_connection: "Server=localhost,1433;Database=BilliardHall_CI;User Id=sa;Password=TestPassword123;"
    redis_connection: "redis:6379,db=1"
```

### 自动化测试报告

```bash
#!/bin/bash
# scripts/run-tests.sh

echo "🧪 运行自动化测试..."

# 单元测试
echo "📝 运行单元测试..."
dotnet test \
    --filter "Category=Unit" \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger "trx;LogFileName=unit-tests.trx"

# 集成测试
echo "🔗 运行集成测试..."
dotnet test \
    --filter "Category=Integration" \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger "trx;LogFileName=integration-tests.trx"

# 生成测试报告
echo "📊 生成测试报告..."
reportgenerator \
    -reports:"TestResults/**/coverage.cobertura.xml" \
    -targetdir:"TestResults/Coverage" \
    -reporttypes:"Html;Cobertura;SonarQube"

echo "✅ 测试完成! 报告位置: TestResults/Coverage/index.html"
```

## 监控和运维自动化

### 健康检查配置

```csharp
// 健康检查自动化配置
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, 
                                                    IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "database",
                tags: new[] { "ready" })
            .AddRedis(
                connectionString: configuration.GetConnectionString("Redis"),
                name: "redis",
                tags: new[] { "ready" })
            .AddUrlGroup(
                uri: new Uri(configuration["ExternalServices:PaymentApi"]),
                name: "payment-api",
                tags: new[] { "external" })
            .AddCheck<BilliardTableAvailabilityHealthCheck>(
                name: "table-availability",
                tags: new[] { "business" });
                
        return services;
    }
}
```

### 自动化告警

```yaml
# monitoring/alerts.yml
alerts:
  - name: high_error_rate
    condition: "error_rate > 0.05"
    for: "5m"
    annotations:
      description: "API 错误率超过 5%"
      runbook_url: "https://docs.billiard-hall.com/runbooks/high-error-rate"
    actions:
      - type: email
        recipients: ["dev-team@billiard-hall.com"]
      - type: slack
        channel: "#alerts"
        
  - name: database_connection_failure
    condition: "health_check{name='database'} == 0"
    for: "1m"
    annotations:
      description: "数据库连接失败"
      severity: "critical"
    actions:
      - type: pagerduty
        service_key: "{{ .PagerDutyServiceKey }}"
        
  - name: low_table_availability
    condition: "available_tables_count < 5"
    for: "10m"
    annotations:
      description: "可用台球桌数量过低"
      severity: "warning"
    actions:
      - type: webhook
        url: "https://api.billiard-hall.com/webhooks/low-availability"
```

## 数据备份自动化

```bash
#!/bin/bash
# scripts/backup.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backup/billiard-hall"
DB_NAME="BilliardHall"

echo "📦 开始数据备份 - $DATE"

# 数据库备份
echo "🗄️  备份数据库..."
sqlcmd -S localhost -E -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_DIR/db_$DATE.bak'"

# Redis 备份
echo "💾 备份 Redis..."
redis-cli --rdb "$BACKUP_DIR/redis_$DATE.rdb"

# 文件备份
echo "📁 备份应用文件..."
tar -czf "$BACKUP_DIR/files_$DATE.tar.gz" /app/wwwroot/uploads

# 清理旧备份 (保留7天)
find "$BACKUP_DIR" -name "*.bak" -mtime +7 -delete
find "$BACKUP_DIR" -name "*.rdb" -mtime +7 -delete
find "$BACKUP_DIR" -name "*.tar.gz" -mtime +7 -delete

echo "✅ 备份完成!"
```

## 性能监控自动化

```csharp
// 性能监控中间件
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IMetrics _metrics;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // 记录请求指标
            _metrics.Record("request_duration", stopwatch.ElapsedMilliseconds, new[]
            {
                new KeyValuePair<string, object>("method", context.Request.Method),
                new KeyValuePair<string, object>("endpoint", context.Request.Path),
                new KeyValuePair<string, object>("status_code", context.Response.StatusCode)
            });
            
            // 慢请求告警
            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                _logger.LogWarning("慢请求检测: {Method} {Path} 耗时 {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
```

## 文档自动化更新

```bash
#!/bin/bash
# scripts/update-docs.sh

echo "📚 更新项目文档..."

# 生成 API 文档
echo "📖 生成 API 文档..."
dotnet swagger tofile --output docs/api/swagger.json src/Zss.BilliardHall.Api/bin/Debug/net8.0/Zss.BilliardHall.Api.dll v1

# 生成数据库文档
echo "🗄️  生成数据库文档..."
dotnet ef dbcontext scaffold \
    "Server=(localdb)\\mssqllocaldb;Database=BilliardHall" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir docs/database \
    --context-dir docs/database \
    --force

# 更新 README
echo "📝 更新 README..."
# (可以集成自动更新 README 的逻辑)

echo "✅ 文档更新完成!"
```

---

> 这些工作流和自动化脚本旨在提高开发效率，确保代码质量，并支持可靠的部署和运维。请根据实际项目需求调整和扩展。