# UniApp 快速开始指南

## ⚠️ 重要提示

**Book API 需要身份验证**：
- 📌 必须先登录才能访问图书列表
- 📌 需要 `BilliardHall.Books.Default` 权限
- 📌 如遇到授权错误，请查看 [故障排查指南](TROUBLESHOOTING.md)

## 🚀 快速启动

### 安装依赖

```bash
cd frontend-uniapp
npm install
```

### 开发运行

#### H5 开发
```bash
npm run dev:h5
```
访问: http://localhost:3000

#### 微信小程序开发
```bash
npm run dev:mp-weixin
```
然后在微信开发者工具导入 `dist/dev/mp-weixin` 目录

### 生产构建

```bash
# H5
npm run build:h5

# 微信小程序
npm run build:mp-weixin
```

---

## 📚 Book API 快速使用

### 1. 引入 API

```javascript
import { getBookList, getBook, createBook, updateBook, deleteBook } from '@/api/book';
```

### 2. 获取图书列表

```javascript
const loadBooks = async () => {
  try {
    const response = await getBookList({
      skipCount: 0,        // 跳过的记录数
      maxResultCount: 10,  // 每页数量
      sorting: 'Name'      // 排序字段
    });
    
    console.log('图书列表:', response.items);
    console.log('总数:', response.totalCount);
  } catch (error) {
    uni.showToast({
      title: error.message || '加载失败',
      icon: 'none'
    });
  }
};
```

### 3. 获取单个图书

```javascript
const loadBookDetail = async (bookId) => {
  try {
    const book = await getBook(bookId);
    console.log('图书详情:', book);
  } catch (error) {
    console.error('加载失败:', error);
  }
};
```

### 4. 创建图书

```javascript
const addBook = async () => {
  try {
    const newBook = await createBook({
      name: '示例图书',
      type: 1,  // BookType.Adventure
      publishDate: '2024-01-01T00:00:00',
      price: 29.99
    });
    console.log('创建成功:', newBook);
  } catch (error) {
    console.error('创建失败:', error);
  }
};
```

### 5. 更新图书

```javascript
const updateBookInfo = async (bookId) => {
  try {
    const updated = await updateBook(bookId, {
      name: '更新后的图书名称',
      type: 2,
      publishDate: '2024-01-01T00:00:00',
      price: 39.99
    });
    console.log('更新成功:', updated);
  } catch (error) {
    console.error('更新失败:', error);
  }
};
```

### 6. 删除图书

```javascript
const removeBook = async (bookId) => {
  try {
    await deleteBook(bookId);
    uni.showToast({
      title: '删除成功',
      icon: 'success'
    });
  } catch (error) {
    console.error('删除失败:', error);
  }
};
```

---

## 📖 BookType 枚举

| 值 | 名称 | 中文 |
|----|------|------|
| 0 | Undefined | 未定义 |
| 1 | Adventure | 冒险 |
| 2 | Biography | 传记 |
| 3 | Dystopia | 反乌托邦 |
| 4 | Fantastic | 奇幻 |
| 5 | Horror | 恐怖 |
| 6 | Science | 科学 |
| 7 | ScienceFiction | 科幻 |
| 8 | Poetry | 诗歌 |

---

## 🔐 认证说明

### 登录

```javascript
import { loginWithSms } from '@/api/auth';

const handleLogin = async (phone, code) => {
  try {
    const response = await loginWithSms({ phone, code });
    // 保存 token
    uni.setStorageSync('token', response.token);
    uni.showToast({
      title: '登录成功',
      icon: 'success'
    });
  } catch (error) {
    uni.showToast({
      title: '登录失败',
      icon: 'none'
    });
  }
};
```

### 退出登录

```javascript
import { logout } from '@/api/auth';

const handleLogout = async () => {
  try {
    await logout();
    uni.removeStorageSync('token');
    uni.navigateTo({
      url: '/pages/login/login'
    });
  } catch (error) {
    console.error('退出失败:', error);
  }
};
```

### Token 自动处理

所有 API 请求会自动：
- 从 `uni.storage` 读取 token
- 添加到请求头: `Authorization: Bearer {token}`
- 401 错误时自动清除 token 并跳转登录页

---

## 🎨 在 Vue 组件中使用

### 完整示例

```vue
<template>
  <view class="container">
    <!-- 加载状态 -->
    <view v-if="loading" class="loading">
      <text>加载中...</text>
    </view>
    
    <!-- 图书列表 -->
    <view v-else class="book-list">
      <view v-for="book in books" :key="book.id" class="book-item">
        <text class="book-name">{{ book.name }}</text>
        <text class="book-price">¥{{ book.price }}</text>
      </view>
    </view>
    
    <!-- 加载更多 -->
    <button v-if="hasMore" @click="loadMore">加载更多</button>
  </view>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getBookList } from '@/api/book';

const books = ref([]);
const loading = ref(false);
const page = ref(0);
const hasMore = ref(true);

onMounted(() => {
  loadBooks();
});

const loadBooks = async () => {
  if (loading.value) return;
  
  try {
    loading.value = true;
    const response = await getBookList({
      skipCount: page.value * 10,
      maxResultCount: 10
    });
    
    books.value = [...books.value, ...response.items];
    hasMore.value = books.value.length < response.totalCount;
    page.value++;
  } catch (error) {
    uni.showToast({
      title: '加载失败',
      icon: 'none'
    });
  } finally {
    loading.value = false;
  }
};

const loadMore = () => {
  loadBooks();
};
</script>

<style scoped>
.container {
  padding: 20rpx;
}

.loading {
  text-align: center;
  padding: 40rpx;
}

.book-item {
  padding: 20rpx;
  border-bottom: 1rpx solid #eee;
}

.book-name {
  font-size: 32rpx;
  font-weight: bold;
}

.book-price {
  color: #ff0000;
  font-size: 28rpx;
}
</style>
```

---

## 🛠️ 环境配置

### 开发环境 (.env.development)

```env
VUE_APP_API_URL=http://localhost:5000
```

### 生产环境 (.env.production)

```env
VUE_APP_API_URL=https://api.yourdomain.com
```

---

## 🐛 调试技巧

### 1. 查看网络请求

**微信开发者工具**:
- 打开 "调试器" 面板
- 切换到 "Network" 标签
- 查看请求和响应详情

**H5 浏览器**:
- 按 F12 打开开发者工具
- 切换到 "Network" 面板
- 查看 API 请求

### 2. 打印调试信息

```javascript
const loadBooks = async () => {
  console.log('开始加载图书列表...');
  
  const response = await getBookList({ skipCount: 0, maxResultCount: 10 });
  console.log('API 响应:', response);
  console.log('图书数量:', response.items.length);
  console.log('总数:', response.totalCount);
};
```

### 3. 使用 Swagger 测试后端

访问: https://localhost:44393/swagger

1. 点击 "Authorize" 按钮登录
2. 选择要测试的 API
3. 填写参数并执行
4. 查看响应结果

---

## ❓ 常见问题

### Q1: API 请求返回 401

**原因**: 未登录或 token 过期

**解决**:
```javascript
// 检查 token
const token = uni.getStorageSync('token');
if (!token) {
  uni.navigateTo({ url: '/pages/login/login' });
}
```

### Q2: API 请求返回 403

**原因**: 没有权限

**解决**: 确保用户有相应的权限:
- 查看列表/详情: `BilliardHall.Books.Default`
- 创建: `BilliardHall.Books.Create`
- 编辑: `BilliardHall.Books.Edit`
- 删除: `BilliardHall.Books.Delete`

### Q3: 跨域问题 (CORS)

**H5 开发环境**:
在后端 `appsettings.json` 中配置:
```json
{
  "App": {
    "CorsOrigins": "http://localhost:3000"
  }
}
```

### Q4: 数据不更新

**检查**:
1. 是否正确使用 `ref()` 或 `reactive()`
2. 是否在 `try-catch` 中更新数据
3. 是否正确处理异步

```javascript
// ❌ 错误
let books = [];  // 不是响应式的

// ✅ 正确
const books = ref([]);  // 响应式
```

---

## 📚 相关文档

- [API 模块使用说明](src/api/README.md)
- [后端 API 文档](../doc/07_API文档/README.md)
- [接口清单](../doc/07_API文档/接口清单.md)
- [实现总结](../IMPLEMENTATION_SUMMARY.md)
- [架构图](../doc/07_API文档/Book_API_集成架构图.md)

---

## 💡 最佳实践

### 1. 错误处理

始终使用 try-catch:
```javascript
try {
  const data = await getBookList();
  // 处理数据
} catch (error) {
  // 错误处理
  uni.showToast({
    title: error.message || '操作失败',
    icon: 'none'
  });
}
```

### 2. 加载状态

显示加载状态提升用户体验:
```javascript
const loading = ref(false);

const loadData = async () => {
  loading.value = true;
  try {
    // API 调用
  } finally {
    loading.value = false;
  }
};
```

### 3. 防止重复请求

```javascript
const loading = ref(false);

const loadData = async () => {
  if (loading.value) return;  // 正在加载中，阻止重复请求
  
  loading.value = true;
  try {
    // API 调用
  } finally {
    loading.value = false;
  }
};
```

### 4. 分页加载

```javascript
const page = ref(0);
const hasMore = ref(true);

const loadMore = async () => {
  if (!hasMore.value) return;
  
  const response = await getBookList({
    skipCount: page.value * 10,
    maxResultCount: 10
  });
  
  books.value.push(...response.items);
  hasMore.value = books.value.length < response.totalCount;
  page.value++;
};
```

---

## 🎯 下一步

1. **查看示例页面**: [pages/book/book-list.vue](src/pages/book/book-list.vue)
2. **阅读 API 文档**: [src/api/README.md](src/api/README.md)
3. **启动后端服务**: 见后端文档
4. **开始开发**: 基于 Book API 模式创建其他业务模块

---

## 📞 需要帮助?

- 查看 [README](README.md)
- 查看 [API 文档](../doc/07_API文档/README.md)
- 提交 Issue

Happy Coding! 🎉
