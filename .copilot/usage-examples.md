# 架构和模板使用示例 (Schema & Template Usage Examples)

## 概述 (Overview)

本文档提供了如何在开发过程中有效使用 `.copilot/schemas` 和 `.copilot/templates` 的实际示例。这些示例展示了如何与 GitHub Copilot 协作，生成符合项目标准的高质量代码。

## 架构使用示例 (Schema Usage Examples)

### 示例 1: 创建新的业务实体

**场景**: 为台球厅系统添加会员卡管理功能

**Copilot 提示词**:
```markdown
基于 schemas/entities.json 中的 Customer 定义和 schemas/abp-entities.json 架构，
创建一个 MembershipCard 实体，包含以下属性：
- 卡号 (CardNumber): 唯一标识，格式为 "MC" + 8位数字
- 会员ID (MemberId): 关联到 Customer 实体
- 卡类型 (CardType): 枚举 (Standard, Premium, VIP, Diamond)
- 余额 (Balance): 金额类型，支持充值和消费
- 有效期 (ExpiryDate): 日期类型
- 状态 (Status): 枚举 (Active, Suspended, Expired, Cancelled)

要求：
1. 继承 ABP 的 FullAuditedEntity<Guid>
2. 实现 IMultiTenant 接口
3. 添加充值、消费、冻结等业务方法
4. 包含完整的数据验证注解
```

**预期输出**: 符合 ABP 规范的实体类，包含所有必需的属性、验证和业务方法。

### 示例 2: 设计 API 响应格式

**场景**: 为会员卡查询 API 设计响应格式

**Copilot 提示词**:
```markdown
遵循 schemas/api-responses.json 架构规范，为 MembershipCard 实体设计以下 API 响应：

1. 单个会员卡查询响应 (GET /api/app/membership-cards/{id})
2. 会员卡列表查询响应 (GET /api/app/membership-cards)
3. 会员卡余额查询响应 (GET /api/app/membership-cards/{id}/balance)
4. 充值操作响应 (POST /api/app/membership-cards/{id}/recharge)

要求包含：
- 标准的响应包装格式
- 完整的分页信息
- 详细的错误处理格式
- 请求追踪信息
```

**预期输出**: 标准化的 API 响应 DTO 类，符合项目的统一响应格式。

### 示例 3: 配置 Aspire 服务编排

**场景**: 为会员卡服务添加 Redis 缓存支持

**Copilot 提示词**:
```markdown
参考 schemas/aspire-config.json 架构，为 MembershipCard 服务配置以下基础设施：

1. 添加 Redis 缓存用于会员卡余额查询
2. 配置 MySQL 数据库连接
3. 启用 OpenTelemetry 监控和追踪
4. 添加健康检查端点
5. 配置服务发现和负载均衡

要求：
- 遵循 Aspire 配置架构规范
- 包含开发和生产环境配置
- 添加适当的资源依赖关系
- 配置监控和日志记录
```

**预期输出**: 完整的 Aspire 应用编排配置，包含所有必需的基础设施服务。

## 模板使用示例 (Template Usage Examples)

### 示例 4: 生成应用服务层代码

**场景**: 为 MembershipCard 实体生成完整的应用服务

**Copilot 提示词**:
```markdown
使用 templates/abp-application-service-template.md 模板，为 MembershipCard 实体生成完整的应用服务代码：

模板变量：
- {EntityName} = MembershipCard
- {EntityNameLower} = membershipCard  
- {EntityNamePlural} = MembershipCards
- {EntityNamePluralLower} = membershipCards
- {ModuleName} = BilliardHall

特殊业务方法：
1. RechargeAsync(Guid id, RechargeInput input) - 充值功能
2. ConsumeAsync(Guid id, ConsumeInput input) - 消费功能
3. FreezeAsync(Guid id, string reason) - 冻结卡片
4. UnfreezeAsync(Guid id) - 解冻卡片
5. GetBalanceHistoryAsync(Guid id, GetBalanceHistoryInput input) - 余额变动历史

要求包含：
- 完整的 CRUD 操作
- 权限验证和异常处理
- 批量操作支持
- 统计和报表功能
- 业务规则验证
```

**预期输出**: 包含接口定义、服务实现和相关 DTO 的完整应用服务代码。

## 组合使用示例 (Combined Usage Examples)

### 示例 5: 端到端功能开发

**场景**: 完整实现会员积分系统

**步骤 1: 实体设计**
```markdown
基于 schemas/abp-entities.json 和 schemas/entities.json 架构，创建以下实体：

1. PointsAccount (积分账户)
   - 关联到 Customer
   - 当前积分余额
   - 积分等级
   - 有效期管理

2. PointsTransaction (积分交易记录)  
   - 交易类型 (Earn, Redeem, Expire, Transfer)
   - 交易金额和积分
   - 相关业务实体引用
   - 交易时间和状态

3. PointsRule (积分规则)
   - 消费金额与积分比例
   - 特殊活动奖励规则
   - 积分有效期规则
```

**步骤 2: API 设计**
```markdown
遵循 schemas/api-responses.json 架构，设计积分系统的 RESTful API：

- GET /api/app/points-accounts/{customerId} - 查询积分账户
- POST /api/app/points-accounts/{customerId}/earn - 赚取积分
- POST /api/app/points-accounts/{customerId}/redeem - 兑换积分  
- GET /api/app/points-transactions - 积分交易历史
- GET /api/app/points-rules - 积分规则查询

包含完整的错误处理、分页和验证响应格式。
```

**步骤 3: 应用服务实现**
```markdown
使用 templates/abp-application-service-template.md 模板，为积分系统实体生成应用服务：

- PointsAccountAppService
- PointsTransactionAppService  
- PointsRuleAppService

包含完整的业务逻辑、权限控制和事件发布。
```

**步骤 4: 基础设施配置**
```markdown
参考 schemas/aspire-config.json 架构，配置积分系统的基础设施：

- Redis 缓存积分余额和规则
- 消息队列处理积分异步计算
- OpenTelemetry 监控积分交易性能
- 定时任务处理积分过期逻辑
```

## 质量检查和验证 (Quality Assurance & Validation)

### 代码生成后的验证清单

1. **架构符合性检查**
   - [ ] 实体是否继承正确的 ABP 基类
   - [ ] 是否实现了必要的接口 (IMultiTenant, ISoftDelete 等)
   - [ ] 属性验证注解是否完整
   - [ ] 业务方法是否遵循领域驱动设计原则

2. **API 规范检查**
   - [ ] 响应格式是否符合统一标准
   - [ ] 错误处理是否完整
   - [ ] 分页信息是否正确
   - [ ] HTTP 状态码使用是否合适

3. **代码质量检查**
   - [ ] 命名是否遵循项目约定
   - [ ] 异常处理是否完善
   - [ ] 权限验证是否正确
   - [ ] 单元测试覆盖率是否达标

### 自动化验证脚本

使用项目提供的验证脚本检查架构符合性：

```bash
# 验证 JSON Schema 文件
python3 .copilot/validate-schemas.py

# 验证生成的代码 (需要项目特定的验证工具)
python3 .copilot/validate-generated-code.py

# 运行单元测试
dotnet test
```

## 最佳实践建议 (Best Practice Recommendations)

1. **渐进式开发**: 先从简单的实体开始，逐步添加复杂的业务逻辑
2. **架构先行**: 在编写代码前，先明确使用哪些架构和模板
3. **验证频繁**: 每生成一部分代码就进行验证，及早发现问题
4. **文档更新**: 新增实体或API时，同步更新相关文档和架构定义
5. **团队协作**: 确保团队成员都了解并遵循架构和模板使用规范

---

> 这些示例展示了如何有效利用项目的架构定义和代码模板，与 GitHub Copilot 协作生成高质量、一致性强的代码。建议在实际开发中参考这些示例，逐步完善项目的开发流程。