<template>
  <view class="container">
    <view class="user-card">
      <view v-if="!isLogin" class="login-prompt" @click="goToLogin">
        <text class="avatar">👤</text>
        <text class="login-text">点击登录</text>
      </view>
      <view v-else class="user-info">
        <image class="avatar" :src="userInfo.avatar" mode="aspectFill"></image>
        <view class="user-detail">
          <text class="username">{{ userInfo.nickname }}</text>
          <text class="phone">{{ userInfo.phone }}</text>
        </view>
      </view>
    </view>

    <view v-if="isLogin" class="balance-card card">
      <view class="balance-item">
        <text class="balance-value">¥{{ balance }}</text>
        <text class="balance-label">账户余额</text>
      </view>
      <view class="balance-item">
        <text class="balance-value">{{ points }}</text>
        <text class="balance-label">积分</text>
      </view>
    </view>

    <view class="menu-list">
      <view class="menu-item" @click="goToOrders">
        <text class="menu-icon">📝</text>
        <text class="menu-title">我的订单</text>
        <text class="menu-arrow">›</text>
      </view>
      <view class="menu-item" @click="goToRecharge">
        <text class="menu-icon">💰</text>
        <text class="menu-title">充值</text>
        <text class="menu-arrow">›</text>
      </view>
      <view class="menu-item" @click="goToMembership">
        <text class="menu-icon">👑</text>
        <text class="menu-title">会员套餐</text>
        <text class="menu-arrow">›</text>
      </view>
      <view class="menu-item" @click="goToBookList">
        <text class="menu-icon">📚</text>
        <text class="menu-title">图书列表</text>
        <text class="menu-arrow">›</text>
      </view>
      <view class="menu-item" @click="goToSettings">
        <text class="menu-icon">⚙️</text>
        <text class="menu-title">设置</text>
        <text class="menu-arrow">›</text>
      </view>
    </view>

    <view v-if="isLogin" class="logout-section">
      <button class="logout-btn" @click="handleLogout">退出登录</button>
    </view>
  </view>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const isLogin = ref(false);
const userInfo = ref({
  nickname: '用户昵称',
  phone: '138****5678',
  avatar: ''
});
const balance = ref('0.00');
const points = ref('0');

onMounted(() => {
  checkLogin();
});

const checkLogin = () => {
  // TODO: 检查登录状态
  const token = uni.getStorageSync('token');
  isLogin.value = !!token;
  
  if (isLogin.value) {
    loadUserData();
  }
};

const loadUserData = async () => {
  // TODO: 从API加载用户数据
};

const goToLogin = () => {
  uni.navigateTo({
    url: '/pages/login/login'
  });
};

const goToOrders = () => {
  uni.showToast({
    title: '功能开发中',
    icon: 'none'
  });
};

const goToRecharge = () => {
  uni.showToast({
    title: '功能开发中',
    icon: 'none'
  });
};

const goToMembership = () => {
  uni.showToast({
    title: '功能开发中',
    icon: 'none'
  });
};

const goToBookList = () => {
  uni.navigateTo({
    url: '/pages/book/book-list'
  });
};

const goToSettings = () => {
  uni.showToast({
    title: '功能开发中',
    icon: 'none'
  });
};

const handleLogout = () => {
  uni.showModal({
    title: '退出登录',
    content: '确定要退出登录吗？',
    success: (res) => {
      if (res.confirm) {
        uni.removeStorageSync('token');
        uni.removeStorageSync('userInfo');
        isLogin.value = false;
        uni.showToast({
          title: '已退出登录',
          icon: 'success'
        });
      }
    }
  });
};
</script>

<style scoped>
.container {
  min-height: 100vh;
  background-color: #f8f8f8;
}

.user-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 60rpx 40rpx;
}

.login-prompt {
  display: flex;
  align-items: center;
}

.login-prompt .avatar {
  width: 120rpx;
  height: 120rpx;
  background-color: rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 64rpx;
  margin-right: 30rpx;
}

.login-text {
  color: #ffffff;
  font-size: 32rpx;
}

.user-info {
  display: flex;
  align-items: center;
}

.user-info .avatar {
  width: 120rpx;
  height: 120rpx;
  border-radius: 50%;
  margin-right: 30rpx;
  background-color: rgba(255, 255, 255, 0.3);
}

.user-detail {
  flex: 1;
}

.username {
  display: block;
  color: #ffffff;
  font-size: 36rpx;
  font-weight: bold;
  margin-bottom: 10rpx;
}

.phone {
  display: block;
  color: rgba(255, 255, 255, 0.9);
  font-size: 28rpx;
}

.balance-card {
  display: flex;
  margin: 20rpx;
  padding: 40rpx;
}

.balance-item {
  flex: 1;
  text-align: center;
}

.balance-item:first-child {
  border-right: 1rpx solid #e0e0e0;
}

.balance-value {
  display: block;
  font-size: 48rpx;
  font-weight: bold;
  color: #fa5151;
  margin-bottom: 10rpx;
}

.balance-label {
  display: block;
  font-size: 24rpx;
  color: #999999;
}

.menu-list {
  background-color: #ffffff;
  margin: 20rpx;
  border-radius: 20rpx;
  overflow: hidden;
}

.menu-item {
  display: flex;
  align-items: center;
  padding: 30rpx 40rpx;
  border-bottom: 1rpx solid #f0f0f0;
}

.menu-item:last-child {
  border-bottom: none;
}

.menu-icon {
  font-size: 44rpx;
  margin-right: 20rpx;
}

.menu-title {
  flex: 1;
  font-size: 28rpx;
  color: #333333;
}

.menu-arrow {
  font-size: 40rpx;
  color: #cccccc;
}

.logout-section {
  padding: 40rpx;
}

.logout-btn {
  width: 100%;
  height: 80rpx;
  background-color: #ffffff;
  color: #fa5151;
  font-size: 28rpx;
  border-radius: 10rpx;
  border: 1rpx solid #fa5151;
}
</style>
