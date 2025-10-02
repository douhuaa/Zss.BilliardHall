# 故障排查指南 / Troubleshooting Guide

## 常见问题 / Common Issues

### 1. 无法获得数据 - 授权错误 / Cannot Get Data - Authorization Error

#### 问题描述 / Problem Description
访问图书列表时显示"无法获得数据，请检查客户端授权"或返回 401/403 错误。

#### 原因分析 / Root Cause
Book API 需要身份验证和权限才能访问：
- **401 错误**: 未登录或 Token 已过期
- **403 错误**: 已登录但没有访问权限

#### 解决方案 / Solution

##### 步骤 1: 检查是否已登录 / Check Authentication Status

```javascript
// 在浏览器控制台或代码中检查
const token = uni.getStorageSync('token');
console.log('Token exists:', !!token);
console.log('Token value:', token);
```

**如果没有 Token**:
1. 导航到登录页面
2. 使用有效的账号密码登录
3. 登录成功后会自动保存 Token

##### 步骤 2: 验证 Token 格式 / Verify Token Format

Token 应该是 ****** 格式的字符串，例如：
```
Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6Ij...
```

**检查代码**:
```javascript
// 在 src/utils/request.js 中
const token = uni.getStorageSync('token');
header: {
  'Authorization': token ? `****** : ''
}
```

##### 步骤 3: 检查用户权限 / Check User Permissions

Book API 需要以下权限：
- **查看图书列表/详情**: `BilliardHall.Books.Default`
- **创建图书**: `BilliardHall.Books.Create`
- **编辑图书**: `BilliardHall.Books.Edit`
- **删除图书**: `BilliardHall.Books.Delete`

**如何检查权限**:
1. 使用 Swagger UI (https://localhost:44393/swagger)
2. 登录后点击 "Authorize" 按钮
3. 尝试调用 `/api/app/book` 端点
4. 如果返回 403，说明当前用户没有权限

**如何授予权限**:
1. 使用管理员账号登录
2. 导航到权限管理页面
3. 为用户或角色授予 `BilliardHall.Books` 相关权限
4. 用户重新登录后权限生效

##### 步骤 4: 检查后端配置 / Check Backend Configuration

**验证 CORS 配置**:
确保后端允许前端域名访问：

```json
// appsettings.json
{
  "App": {
    "CorsOrigins": "http://localhost:3000,https://localhost:3001"
  }
}
```

**验证认证服务**:
确保 OpenIddict 服务正常运行，可以在 Swagger UI 中测试。

##### 步骤 5: 使用测试账号 / Use Test Account

如果以上步骤都正常，尝试使用管理员测试账号：

```
用户名: admin
密码: 1q2w3E*
```

管理员账号默认拥有所有权限。

#### 前端代码改进 / Frontend Code Improvements

最新版本的 `book-list.vue` 已经添加了：

1. **登录检查**: 页面加载时自动检查是否已登录
2. **友好提示**: 未登录时显示登录提示界面
3. **错误处理**: 区分 401 和 403 错误，显示相应提示
4. **重试机制**: 授权错误后可以点击重试按钮

**UI 改进**:
```vue
<!-- 未登录提示 -->
<view v-if="!isAuthenticated" class="auth-prompt">
  <text class="prompt-icon">🔒</text>
  <text class="prompt-title">需要登录</text>
  <text class="prompt-text">查看图书列表需要登录账号</text>
  <button class="btn-login" @click="goToLogin">前往登录</button>
</view>

<!-- 授权错误提示 -->
<view v-else-if="authError" class="error-state">
  <text class="error-icon">⚠️</text>
  <text class="error-title">无权限访问</text>
  <text class="error-text">{{ authError }}</text>
  <button class="btn-retry" @click="loadBookList">重试</button>
</view>
```

---

### 2. Token 过期问题 / Token Expiration

#### 问题描述
使用中突然返回 401 错误，提示"未授权"。

#### 原因
Access Token 有过期时间（通常 1-24 小时）。

#### 解决方案

**方法 1: 重新登录**
```javascript
// 清除过期 token
uni.removeStorageSync('token');
// 跳转登录页
uni.navigateTo({ url: '/pages/login/login' });
```

**方法 2: 实现 Token 刷新机制** (未来改进)
```javascript
// 在 src/api/auth.js 中添加
export function refreshToken() {
  return post('/api/auth/refresh-token');
}

// 在 request.js 中自动刷新
if (res.statusCode === 401) {
  try {
    const newToken = await refreshToken();
    uni.setStorageSync('token', newToken);
    // 重试原请求
  } catch (error) {
    // 刷新失败，跳转登录
  }
}
```

---

### 3. 网络连接问题 / Network Connection Issues

#### 问题描述
请求长时间无响应或显示"网络错误"。

#### 检查清单

1. **检查后端服务是否运行**:
```bash
# 检查后端是否在运行
curl http://localhost:5000/api/app/book
# 或访问 Swagger UI
# https://localhost:44393/swagger
```

2. **检查 API 地址配置**:
```javascript
// .env.development
VUE_APP_API_URL=http://localhost:5000

// 检查 src/utils/request.js
const BASE_URL = process.env.VUE_APP_API_URL || 'http://localhost:5000';
```

3. **检查防火墙/网络设置**:
- 确保端口 5000 或 44393 未被防火墙阻止
- 在微信开发者工具中，关闭"不校验合法域名"

4. **检查 HTTPS 证书** (生产环境):
- 确保 SSL 证书有效
- 检查域名配置正确

---

### 4. 数据格式问题 / Data Format Issues

#### 问题描述
API 返回 200 但前端无法正确显示数据。

#### 解决方案

**检查响应格式**:
```javascript
const response = await getBookList();
console.log('Response:', response);
console.log('Items:', response.items);
console.log('Total:', response.totalCount);
```

**ABP 标准响应格式**:
```json
{
  "items": [...],
  "totalCount": 100
}
```

**常见错误**:
- 响应包裹在额外的 `data` 字段中
- `items` 字段名称不匹配
- `totalCount` 类型错误（字符串 vs 数字）

---

### 5. 权限不足问题 / Insufficient Permissions

#### 问题描述
登录后仍然返回 403 错误。

#### 详细排查步骤

1. **检查用户角色**:
```sql
-- 在数据库中查询用户角色
SELECT u.UserName, r.Name as RoleName
FROM AbpUsers u
JOIN AbpUserRoles ur ON u.Id = ur.UserId
JOIN AbpRoles r ON ur.RoleId = r.Id;
```

2. **检查角色权限**:
```sql
-- 查询角色拥有的权限
SELECT r.Name as RoleName, p.Name as PermissionName
FROM AbpRoles r
JOIN AbpRolePermissions rp ON r.Id = rp.RoleId
JOIN AbpPermissionGrants p ON rp.PermissionGrantId = p.Id
WHERE p.Name LIKE 'BilliardHall.Books%';
```

3. **授予权限** (使用 ABP 管理界面):
   - 登录管理后台
   - 导航到 "权限管理" → "角色"
   - 选择用户所属角色
   - 勾选 `BilliardHall.Books` 权限
   - 保存

4. **程序化授予权限** (DbMigrator):
```csharp
// 在 DataSeedContributor 中
await _permissionDataSeeder.SeedAsync(
    RolePermissionValueProvider.ProviderName,
    "admin",
    new[] { "BilliardHall.Books", "BilliardHall.Books.Create", ... }
);
```

---

### 6. 开发环境特定问题 / Development Environment Issues

#### H5 开发模式

**CORS 错误**:
```
Access to XMLHttpRequest at 'http://localhost:5000/api/app/book' 
from origin 'http://localhost:3000' has been blocked by CORS policy
```

**解决**:
1. 后端添加 CORS 策略
2. 使用代理服务器（vite.config.js）

```javascript
// vite.config.js
export default {
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
}
```

#### 微信小程序开发模式

**域名白名单**:
微信小程序要求配置合法域名。

**临时解决** (开发阶段):
在微信开发者工具中：
- 右上角"详情"
- "本地设置"
- 勾选"不校验合法域名、web-view（业务域名）、TLS 版本以及 HTTPS 证书"

**正式解决** (生产环境):
1. 在微信公众平台配置服务器域名
2. 域名必须使用 HTTPS
3. 需要备案

---

## 调试技巧 / Debugging Tips

### 1. 启用详细日志

```javascript
// 在 request.js 中添加日志
export function request(options) {
  console.log('[API Request]', {
    url: BASE_URL + options.url,
    method: options.method,
    data: options.data,
    headers: options.header
  });
  
  return new Promise((resolve, reject) => {
    // ... 请求代码
    
    console.log('[API Response]', {
      url: BASE_URL + options.url,
      statusCode: res.statusCode,
      data: res.data
    });
  });
}
```

### 2. 使用 Swagger UI 测试

1. 访问 https://localhost:44393/swagger
2. 点击 "Authorize" 登录
3. 测试 Book API 端点
4. 查看实际的请求和响应格式

### 3. 使用浏览器开发者工具

**Network 面板**:
- 查看实际发送的请求
- 检查请求头（Authorization）
- 查看响应状态码和内容

**Console 面板**:
- 查看错误日志
- 执行调试代码

### 4. 后端日志

```bash
# 查看后端日志
cd src/Zss.BilliardHall/src/Zss.BilliardHall.HttpApi.Host
dotnet run

# 日志会显示：
# - 收到的请求
# - 认证信息
# - 授权检查结果
# - 异常详情
```

---

## 快速诊断清单 / Quick Diagnostic Checklist

使用此清单快速定位问题：

- [ ] 后端服务是否运行？
- [ ] 前端配置的 API 地址是否正确？
- [ ] 用户是否已登录（检查 token）？
- [ ] Token 格式是否正确（Bearer xxx）？
- [ ] Token 是否过期？
- [ ] 用户是否有 `BilliardHall.Books` 权限？
- [ ] CORS 配置是否正确？
- [ ] 网络连接是否正常？
- [ ] 是否有防火墙阻止？
- [ ] 响应数据格式是否正确？

---

## 联系支持 / Contact Support

如果以上方法都无法解决问题，请：

1. **收集以下信息**:
   - 错误截图
   - 浏览器控制台日志
   - Network 面板的请求详情
   - 后端日志

2. **提供环境信息**:
   - 操作系统
   - 浏览器版本
   - Node.js 版本
   - 是否在微信开发者工具中

3. **创建 Issue**:
   - 在 GitHub 仓库创建 Issue
   - 使用 "Bug Report" 模板
   - 详细描述问题和复现步骤

---

## 参考文档 / References

- [快速开始指南](QUICK_START.md)
- [API 文档](../doc/07_API文档/README.md)
- [实现总结](../IMPLEMENTATION_SUMMARY.md)
- [ABP 官方文档 - 授权](https://docs.abp.io/en/abp/latest/Authorization)
- [UniApp 官方文档](https://uniapp.dcloud.net.cn/)
