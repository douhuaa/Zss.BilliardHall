<template>
  <view class="container">
    <view class="header">
      <text class="title">图书列表</text>
    </view>

    <!-- 未登录提示 -->
    <view v-if="!isAuthenticated" class="auth-prompt">
      <text class="prompt-icon">🔒</text>
      <text class="prompt-title">需要登录</text>
      <text class="prompt-text">查看图书列表需要登录账号</text>
      <button class="btn-login" @click="goToLogin">前往登录</button>
    </view>

    <!-- 加载状态 -->
    <view v-else-if="loading" class="loading-state">
      <text class="loading-text">加载中...</text>
    </view>

    <!-- 空状态 -->
    <view v-else-if="bookList.length === 0 && !authError" class="empty-state">
      <text class="empty-text">暂无图书数据</text>
    </view>

    <!-- 授权错误提示 -->
    <view v-else-if="authError" class="error-state">
      <text class="error-icon">⚠️</text>
      <text class="error-title">无权限访问</text>
      <text class="error-text">{{ authError }}</text>
      <button class="btn-retry" @click="loadBookList">重试</button>
    </view>

    <!-- 图书列表 -->
    <view v-else class="book-list">
      <view v-for="book in bookList" :key="book.id" class="book-item">
        <view class="book-info">
          <text class="book-name">{{ book.name }}</text>
          <view class="book-meta">
            <text class="book-type">{{ getBookTypeName(book.type) }}</text>
            <text class="book-date">{{ formatDate(book.publishDate) }}</text>
          </view>
        </view>
        <view class="book-price">
          <text class="price-label">¥</text>
          <text class="price-value">{{ book.price.toFixed(2) }}</text>
        </view>
      </view>
    </view>

    <!-- 分页加载更多 -->
    <view v-if="!loading && hasMore" class="load-more">
      <button class="btn-load-more" @click="loadMore">加载更多</button>
    </view>
    
    <view v-if="!loading && !hasMore && bookList.length > 0" class="no-more">
      <text class="no-more-text">没有更多数据了</text>
    </view>
  </view>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getBookList } from '@/api/book';

const loading = ref(false);
const bookList = ref([]);
const skipCount = ref(0);
const maxResultCount = ref(10);
const hasMore = ref(true);
const totalCount = ref(0);
const isAuthenticated = ref(false);
const authError = ref('');

onMounted(() => {
  checkAuth();
});

const checkAuth = () => {
  const token = uni.getStorageSync('token');
  isAuthenticated.value = !!token;
  
  if (isAuthenticated.value) {
    loadBookList();
  }
};

const goToLogin = () => {
  uni.navigateTo({
    url: '/pages/login/login'
  });
};

const loadBookList = async () => {
  if (loading.value) return;
  
  // 重新检查认证状态
  const token = uni.getStorageSync('token');
  if (!token) {
    isAuthenticated.value = false;
    return;
  }
  
  try {
    loading.value = true;
    authError.value = '';
    
    const params = {
      skipCount: skipCount.value,
      maxResultCount: maxResultCount.value,
      sorting: 'Name'
    };
    
    const response = await getBookList(params);
    
    if (response && response.items) {
      bookList.value = response.items;
      totalCount.value = response.totalCount || 0;
      hasMore.value = bookList.value.length < totalCount.value;
    }
  } catch (error) {
    console.error('加载图书列表失败:', error);
    
    // 处理授权错误
    if (error.message && error.message.includes('未授权')) {
      isAuthenticated.value = false;
      authError.value = '登录已过期，请重新登录';
    } else if (error.message && error.message.includes('权限')) {
      authError.value = '您没有访问图书列表的权限，请联系管理员';
    } else {
      authError.value = error.message || '加载失败，请稍后重试';
    }
    
    uni.showToast({
      title: authError.value || '加载失败',
      icon: 'none',
      duration: 3000
    });
  } finally {
    loading.value = false;
  }
};

const loadMore = async () => {
  if (loading.value || !hasMore.value) return;
  
  try {
    loading.value = true;
    skipCount.value += maxResultCount.value;
    
    const params = {
      skipCount: skipCount.value,
      maxResultCount: maxResultCount.value,
      sorting: 'Name'
    };
    
    const response = await getBookList(params);
    
    if (response && response.items) {
      bookList.value = [...bookList.value, ...response.items];
      totalCount.value = response.totalCount || 0;
      hasMore.value = bookList.value.length < totalCount.value;
    }
  } catch (error) {
    console.error('加载更多失败:', error);
    uni.showToast({
      title: error.message || '加载失败',
      icon: 'none',
      duration: 2000
    });
  } finally {
    loading.value = false;
  }
};

const getBookTypeName = (type) => {
  const typeMap = {
    0: '未定义',
    1: '冒险',
    2: '传记',
    3: '反乌托邦',
    4: '奇幻',
    5: '恐怖',
    6: '科学',
    7: '科幻',
    8: '诗歌'
  };
  return typeMap[type] || '未知';
};

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};
</script>

<style scoped>
.container {
  padding: 20rpx;
  min-height: 100vh;
  background-color: #f8f8f8;
}

.header {
  padding: 30rpx 20rpx;
  background-color: #ffffff;
  border-radius: 20rpx;
  margin-bottom: 20rpx;
}

.title {
  font-size: 40rpx;
  font-weight: bold;
  color: #333333;
}

.loading-state,
.empty-state {
  padding: 100rpx 0;
  text-align: center;
}

.loading-text,
.empty-text {
  color: #999999;
  font-size: 28rpx;
}

.book-list {
  background-color: #ffffff;
  border-radius: 20rpx;
  overflow: hidden;
}

.book-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 30rpx 20rpx;
  border-bottom: 1rpx solid #eeeeee;
}

.book-item:last-child {
  border-bottom: none;
}

.book-info {
  flex: 1;
  margin-right: 20rpx;
}

.book-name {
  display: block;
  font-size: 32rpx;
  font-weight: bold;
  color: #333333;
  margin-bottom: 10rpx;
}

.book-meta {
  display: flex;
  align-items: center;
  gap: 20rpx;
}

.book-type {
  font-size: 24rpx;
  color: #666666;
  padding: 4rpx 12rpx;
  background-color: #f0f0f0;
  border-radius: 8rpx;
}

.book-date {
  font-size: 24rpx;
  color: #999999;
}

.book-price {
  display: flex;
  align-items: baseline;
  color: #fa5151;
}

.price-label {
  font-size: 24rpx;
  margin-right: 4rpx;
}

.price-value {
  font-size: 36rpx;
  font-weight: bold;
}

.load-more {
  margin-top: 20rpx;
  padding: 20rpx;
}

.btn-load-more {
  width: 100%;
  padding: 20rpx;
  background-color: #ffffff;
  border: 1rpx solid #eeeeee;
  border-radius: 10rpx;
  font-size: 28rpx;
  color: #666666;
}

.no-more {
  margin-top: 20rpx;
  padding: 20rpx;
  text-align: center;
}

.no-more-text {
  font-size: 24rpx;
  color: #999999;
}

.auth-prompt,
.error-state {
  padding: 100rpx 40rpx;
  text-align: center;
  background-color: #ffffff;
  border-radius: 20rpx;
  margin: 20rpx;
}

.prompt-icon,
.error-icon {
  display: block;
  font-size: 120rpx;
  margin-bottom: 30rpx;
}

.prompt-title,
.error-title {
  display: block;
  font-size: 36rpx;
  font-weight: bold;
  color: #333333;
  margin-bottom: 20rpx;
}

.prompt-text,
.error-text {
  display: block;
  font-size: 28rpx;
  color: #666666;
  margin-bottom: 40rpx;
  line-height: 1.6;
}

.btn-login,
.btn-retry {
  width: 400rpx;
  height: 80rpx;
  line-height: 80rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  font-size: 28rpx;
  border-radius: 40rpx;
  border: none;
  margin: 0 auto;
}

.btn-retry {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}
</style>
