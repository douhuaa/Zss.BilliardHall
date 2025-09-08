# Zss.BilliardHall

## 项目简介

Zss.BilliardHall 是一个台球厅管理系统，旨在为台球厅提供全面的运营管理解决方案。该系统可以帮助台球厅管理者有效地管理球台、客户、预订、计费等各个方面的业务，提升运营效率和客户体验。

### 主要功能
- 🎱 球台管理：实时监控球台状态，管理球台信息
- 👥 客户管理：客户信息维护，会员管理
- 📅 预订系统：球台预订，时间管理
- 💰 计费系统：灵活的计费规则，自动计算费用
- 📊 数据统计：营业数据分析，报表生成
- 🔧 系统管理：用户权限管理，系统配置

## 快速启动

### 环境要求
- .NET 6.0 或更高版本
- Visual Studio 2022 或 Visual Studio Code
- SQL Server 或 MySQL 数据库

### 安装步骤

1. **克隆项目**
   ```bash
   git clone https://github.com/douhuaa/Zss.BilliardHall.git
   cd Zss.BilliardHall
   ```

2. **配置数据库**
   ```bash
   # 修改 appsettings.json 中的数据库连接字符串
   # 运行数据库迁移
   dotnet ef database update
   ```

3. **安装依赖**
   ```bash
   dotnet restore
   ```

4. **编译项目**
   ```bash
   dotnet build
   ```

5. **运行项目**
   ```bash
   dotnet run
   ```

6. **访问应用**
   
   打开浏览器访问 `http://localhost:5000` 或 `https://localhost:5001`

### Docker 部署

```bash
# 构建镜像
docker build -t zss-billiard-hall .

# 运行容器
docker run -p 8080:80 zss-billiard-hall
```

## 技术栈

### 后端技术
- **框架**: ASP.NET Core 6.0
- **语言**: C# 10
- **数据库**: Entity Framework Core
- **认证**: ASP.NET Core Identity
- **API**: RESTful API / GraphQL
- **缓存**: Redis
- **消息队列**: RabbitMQ

### 前端技术
- **框架**: Blazor Server / React (可选)
- **UI 组件**: Bootstrap / Ant Design
- **图表**: Chart.js / ECharts
- **实时通信**: SignalR

### 开发工具
- **IDE**: Visual Studio 2022 / VS Code
- **版本控制**: Git
- **CI/CD**: GitHub Actions
- **代码质量**: SonarQube
- **测试**: xUnit / MSTest

### 数据库
- **主数据库**: SQL Server / MySQL / PostgreSQL
- **缓存**: Redis
- **文件存储**: 本地存储 / 阿里云OSS / AWS S3

## 项目结构

```
Zss.BilliardHall/
├── src/                          # 源代码目录
│   ├── Zss.BilliardHall.Web/     # Web应用程序
│   ├── Zss.BilliardHall.Api/     # API接口
│   ├── Zss.BilliardHall.Core/    # 核心业务逻辑
│   ├── Zss.BilliardHall.Data/    # 数据访问层
│   └── Zss.BilliardHall.Shared/  # 共享组件
├── tests/                        # 测试项目
│   ├── Zss.BilliardHall.Tests/   # 单元测试
│   └── Zss.BilliardHall.IntegrationTests/ # 集成测试
├── docs/                         # 文档
│   ├── api/                      # API文档
│   ├── deployment/               # 部署文档
│   └── development/              # 开发文档
├── docker/                       # Docker相关文件
├── scripts/                      # 脚本文件
├── .github/                      # GitHub Actions配置
├── .gitignore                    # Git忽略文件
├── README.md                     # 项目说明文件
├── LICENSE                       # 许可证文件
└── Zss.BilliardHall.sln          # 解决方案文件
```

### 主要模块说明

- **Web**: 用户界面，提供台球厅管理的Web界面
- **Api**: RESTful API接口，支持移动端和第三方集成
- **Core**: 核心业务逻辑，包含实体、服务、业务规则
- **Data**: 数据访问层，处理数据库操作和数据映射
- **Shared**: 共享组件，包含通用工具类、扩展方法等

## 如何贡献

我们欢迎社区的贡献！请遵循以下步骤参与项目开发：

### 贡献指南

1. **Fork 项目**
   - 点击右上角的 Fork 按钮

2. **创建特性分支**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **提交更改**
   ```bash
   git commit -m "Add: 添加新功能描述"
   ```

4. **推送分支**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **创建 Pull Request**
   - 在 GitHub 上创建 PR
   - 详细描述您的更改
   - 等待代码审查

### 开发规范

- **代码风格**: 遵循 C# 编码规范
- **提交信息**: 使用清晰的提交信息格式
  - `Add: 添加新功能`
  - `Fix: 修复bug`
  - `Update: 更新功能`
  - `Docs: 更新文档`
- **测试**: 确保新功能有对应的测试用例
- **文档**: 更新相关文档

### 问题反馈

如果您发现bug或有功能建议，请：

1. 检查 [Issues](https://github.com/douhuaa/Zss.BilliardHall/issues) 是否已存在相关问题
2. 如果没有，请创建新的 Issue
3. 详细描述问题或建议
4. 提供复现步骤（如果是bug）

### 联系方式

- **项目维护者**: [douhuaa](https://github.com/douhuaa)
- **Email**: [请在GitHub上联系]
- **QQ群**: [待建立]
- **微信群**: [待建立]

## 许可证

本项目采用 [MIT License](LICENSE) 许可证。

### MIT License 说明

MIT许可证是一个宽松的开源许可证，允许：
- ✅ 商业使用
- ✅ 修改代码
- ✅ 分发代码
- ✅ 私人使用

要求：
- 📄 保留版权声明
- 📄 保留许可证声明

限制：
- ❌ 不提供责任保证
- ❌ 不提供质量保证

---

## 致谢

感谢所有为这个项目做出贡献的开发者！

### 主要贡献者
- [douhuaa](https://github.com/douhuaa) - 项目创建者和维护者

### 特别感谢
- 所有提交Issue和Pull Request的贡献者
- 台球厅行业专家提供的业务指导
- 开源社区提供的优秀组件和工具

---

## 更新日志

### v1.0.0 (计划中)
- 基础台球厅管理功能
- 球台状态管理
- 客户管理系统
- 基础计费功能

---

**如果这个项目对您有帮助，请给我们一个 ⭐ Star！**