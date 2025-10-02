# 7. API 文档

## 7.1 API 概述

本系统基于 ABP Framework 构建，采用 RESTful API 架构。API 遵循 ABP 的约定式路由规则：

### 端点规则

ABP Framework 自动将 Application Service 转换为 REST API 端点：

- **基础路径**: `/api/app/{service-name}`
- **命名转换**: 自动将 `AppService` 后缀转换为小写的服务名
- **HTTP 方法映射**:
  - `GetAsync` → GET
  - `GetListAsync` → GET (with query parameters)
  - `CreateAsync` → POST
  - `UpdateAsync` → PUT
  - `DeleteAsync` → DELETE

### 示例

`BookAppService` 会自动映射为以下端点：

- `GET /api/app/book` - 获取图书列表
- `GET /api/app/book/{id}` - 获取单个图书
- `POST /api/app/book` - 创建图书
- `PUT /api/app/book/{id}` - 更新图书
- `DELETE /api/app/book/{id}` - 删除图书

### API 特性

- **自动分页**: 支持 `skipCount` 和 `maxResultCount` 参数
- **排序**: 支持 `sorting` 参数（如 "Name DESC"）
- **认证**: 基于 OpenIddict 的 OAuth 2.0 / OIDC
- **授权**: 基于 ABP Permission 系统
- **审计日志**: 自动记录 API 调用

---

## 7.2 认证与授权

详见 [认证与授权文档](./认证与授权.md)

### 快速上手

1. **获取访问令牌** (Access Token)
2. **在请求头中携带令牌**:
   ```
   Authorization: Bearer {access_token}
   ```

### UniApp 集成

在 `src/utils/request.js` 中已经封装了自动添加认证头的逻辑：

```javascript
const token = uni.getStorageSync('token');
header: {
  'Authorization': token ? `Bearer ${token}` : ''
}
```

---

## 7.3 接口清单

详见 [接口清单文档](./接口清单.md)

### 已实现的 API 模块

- ✅ **图书管理 API** (Book API) - 完整的 CRUD 操作
- 🚧 台球桌管理 API (Table API)
- 🚧 计费会话 API (Session API)
- 🚧 支付 API (Payment API)
- 🚧 用户管理 API (User API)

---

## 7.4 Swagger / OpenAPI 文档

### 访问 Swagger UI

启动 HttpApi.Host 后，访问：

**开发环境**: https://localhost:44393/swagger

### Swagger 配置

在 `BilliardHallHttpApiHostModule.cs` 中配置：

```csharp
context.Services.AddAbpSwaggerGenWithOidc(
    configuration["AuthServer:Authority"]!,
    ["BilliardHall"],
    [AbpSwaggerOidcFlows.AuthorizationCode],
    // ...
);
```

### 使用 Swagger 测试 API

1. 点击右上角的 "Authorize" 按钮
2. 使用 Authorization Code 流程登录
3. 选择要测试的 API 端点
4. 填写参数并执行

### 导出 OpenAPI 规范

已导出的 OpenAPI 规范文件: [Swagger导出.json](./Swagger导出.json)

---

## 7.5 错误处理

详见 [错误码说明文档](./错误码说明.md)

标准 HTTP 状态码：
- 200: 成功
- 201: 创建成功
- 204: 无内容（删除成功）
- 400: 请求参数错误
- 401: 未授权
- 403: 无权限
- 404: 资源不存在
- 500: 服务器错误

---

## 7.6 前端集成指南

### UniApp 集成

在 `frontend-uniapp/src/api/` 目录下创建对应的 API 模块文件：

```javascript
import { get, post, put, del } from '@/utils/request';

export function getBookList(params) {
  return get('/api/app/book', params);
}
```

示例页面: [book-list.vue](../../frontend-uniapp/src/pages/book/book-list.vue)

### 请求封装

所有 API 请求通过 `src/utils/request.js` 统一管理，自动处理：
- 认证令牌注入
- 请求/响应拦截
- 错误处理
- 401 自动跳转登录

---

文件位置：`doc/07_API文档/`
