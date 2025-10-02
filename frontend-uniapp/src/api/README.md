# API 模块说明

本目录包含与后端 API 交互的所有模块。每个模块对应后端的一个业务领域。

## 目录结构

```
api/
├── auth.js       # 认证相关 (登录、注册、验证码)
├── book.js       # 图书管理 (CRUD 操作)
├── payment.js    # 支付相关
├── session.js    # 计费会话
├── user.js       # 用户信息
└── README.md     # 本文件
```

## 使用方法

### 1. 引入 API 方法

```javascript
import { getBookList, getBook } from '@/api/book';
import { loginWithSms } from '@/api/auth';
```

### 2. 在组件中调用

```javascript
<script setup>
import { ref, onMounted } from 'vue';
import { getBookList } from '@/api/book';

const books = ref([]);

onMounted(async () => {
  try {
    const response = await getBookList({
      skipCount: 0,
      maxResultCount: 10
    });
    books.value = response.items;
  } catch (error) {
    console.error('加载失败:', error);
  }
});
</script>
```

## API 模块详解

### auth.js - 认证模块

提供用户认证相关功能：

- `sendSmsCode(phone)` - 发送短信验证码
- `loginWithSms(data)` - 短信验证码登录
- `loginWithWechat(data)` - 微信登录
- `logout()` - 退出登录
- `refreshToken()` - 刷新令牌

**示例**:
```javascript
import { loginWithSms } from '@/api/auth';

const handleLogin = async () => {
  try {
    const response = await loginWithSms({
      phone: '13800138000',
      code: '123456'
    });
    uni.setStorageSync('token', response.token);
  } catch (error) {
    console.error('登录失败:', error);
  }
};
```

### book.js - 图书管理模块

提供图书的完整 CRUD 操作：

- `getBookList(params)` - 获取图书列表（分页）
- `getBook(id)` - 获取单个图书详情
- `createBook(data)` - 创建图书
- `updateBook(id, data)` - 更新图书
- `deleteBook(id)` - 删除图书

**分页参数**:
- `skipCount`: 跳过的记录数（默认 0）
- `maxResultCount`: 最大返回数量（默认 10）
- `sorting`: 排序字段（如 "Name DESC"）

**示例**:
```javascript
import { getBookList, createBook } from '@/api/book';

// 获取列表
const loadBooks = async (page = 0) => {
  const response = await getBookList({
    skipCount: page * 10,
    maxResultCount: 10,
    sorting: 'Name'
  });
  return response;
};

// 创建图书
const addBook = async () => {
  const newBook = await createBook({
    name: '示例图书',
    type: 1,
    publishDate: '2024-01-01T00:00:00',
    price: 29.99
  });
  return newBook;
};
```

### session.js - 计费会话模块

提供台球桌计费会话管理功能（待实现）。

### payment.js - 支付模块

提供支付相关功能（待实现）。

### user.js - 用户模块

提供用户信息管理功能（待实现）。

## 请求封装

所有 API 调用都通过 `@/utils/request.js` 统一封装，自动处理：

### 自动添加认证令牌

```javascript
const token = uni.getStorageSync('token');
header: {
  'Authorization': token ? `Bearer ${token}` : ''
}
```

### 错误处理

- **401 未授权**: 自动清除 token，跳转登录页
- **其他错误**: 返回错误对象供调用方处理

### 统一请求方法

从 `@/utils/request.js` 导出：

- `get(url, params)` - GET 请求
- `post(url, data)` - POST 请求
- `put(url, data)` - PUT 请求
- `del(url, params)` - DELETE 请求

## 后端 API 规则

后端基于 ABP Framework，遵循约定式路由：

### 端点格式

```
/api/app/{service-name}/{action}
```

例如：
- `BookAppService` → `/api/app/book`
- `GetListAsync()` → `GET /api/app/book`
- `GetAsync(id)` → `GET /api/app/book/{id}`
- `CreateAsync()` → `POST /api/app/book`

### 响应格式

**列表响应**:
```json
{
  "items": [...],
  "totalCount": 100
}
```

**单个对象**:
```json
{
  "id": "guid",
  "name": "...",
  ...
}
```

**错误响应**:
```json
{
  "error": {
    "code": "...",
    "message": "...",
    "details": "..."
  }
}
```

## 最佳实践

### 1. 错误处理

始终使用 try-catch 处理异步请求：

```javascript
try {
  const result = await getBookList();
  // 处理成功
} catch (error) {
  console.error('请求失败:', error);
  uni.showToast({
    title: error.message || '请求失败',
    icon: 'none'
  });
}
```

### 2. 加载状态

在请求期间显示加载状态：

```javascript
const loading = ref(false);

const loadData = async () => {
  if (loading.value) return;
  
  try {
    loading.value = true;
    const data = await getBookList();
    // 处理数据
  } catch (error) {
    // 错误处理
  } finally {
    loading.value = false;
  }
};
```

### 3. 分页加载

实现下拉加载更多：

```javascript
const bookList = ref([]);
const page = ref(0);
const hasMore = ref(true);

const loadMore = async () => {
  if (!hasMore.value) return;
  
  const response = await getBookList({
    skipCount: page.value * 10,
    maxResultCount: 10
  });
  
  bookList.value.push(...response.items);
  page.value++;
  hasMore.value = bookList.value.length < response.totalCount;
};
```

### 4. Token 管理

登录成功后保存 token：

```javascript
const response = await loginWithSms({ phone, code });
uni.setStorageSync('token', response.token);
```

退出登录时清除 token：

```javascript
uni.removeStorageSync('token');
uni.navigateTo({ url: '/pages/login/login' });
```

## 环境配置

API 基础地址在 `.env` 文件中配置：

**.env.development**:
```
VUE_APP_API_URL=http://localhost:5000
```

**.env.production**:
```
VUE_APP_API_URL=https://api.example.com
```

## 调试技巧

### 1. 查看网络请求

在微信开发者工具或浏览器的 Network 面板中查看请求详情。

### 2. 打印请求响应

```javascript
const response = await getBookList();
console.log('响应数据:', response);
```

### 3. 使用 Swagger 测试

后端提供 Swagger UI，可以在浏览器中直接测试 API：

https://localhost:44393/swagger

## 相关文档

- [后端 API 文档](../../../doc/07_API文档/README.md)
- [接口清单](../../../doc/07_API文档/接口清单.md)
- [认证与授权](../../../doc/07_API文档/认证与授权.md)
- [UniApp 官方文档](https://uniapp.dcloud.net.cn/)

## 贡献指南

添加新的 API 模块时，请：

1. 在本目录创建新的 `.js` 文件
2. 导出清晰命名的函数
3. 添加 JSDoc 注释
4. 更新本 README 文件
5. 在后端文档中添加对应的 API 说明

示例：

```javascript
/**
 * 获取XXX列表
 * @param {Object} params 查询参数
 * @param {number} params.skipCount 跳过数量
 * @param {number} params.maxResultCount 最大返回数量
 * @returns {Promise<{items: Array, totalCount: number}>}
 */
export function getXxxList(params) {
  return get('/api/app/xxx', params);
}
```
