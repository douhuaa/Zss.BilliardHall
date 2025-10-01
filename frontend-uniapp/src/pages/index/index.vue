<template>
  <view class="container">
    <view class="banner">
      <text class="banner-title">欢迎使用台球厅管理系统</text>
      <text class="banner-subtitle">扫码开台，自助计费</text>
    </view>

    <view class="quick-actions">
      <view class="action-card" @click="goToScan">
        <text class="action-icon">📷</text>
        <text class="action-title">扫码开台</text>
      </view>
      <view class="action-card" @click="goToMine">
        <text class="action-icon">👤</text>
        <text class="action-title">我的会员</text>
      </view>
    </view>

    <view class="card">
      <view class="card-header">
        <text class="card-title">当前会话</text>
      </view>
      <view v-if="!currentSession" class="empty-state">
        <text class="empty-text">暂无进行中的会话</text>
      </view>
      <view v-else class="session-info">
        <view class="session-row flex-between">
          <text>球台号：</text>
          <text class="text-primary">{{ currentSession.tableNumber }}</text>
        </view>
        <view class="session-row flex-between">
          <text>开始时间：</text>
          <text>{{ currentSession.startTime }}</text>
        </view>
        <view class="session-row flex-between">
          <text>当前费用：</text>
          <text class="text-danger">¥{{ currentSession.amount }}</text>
        </view>
        <button class="btn-primary" @click="goToSession">查看详情</button>
      </view>
    </view>

    <view class="card">
      <view class="card-header">
        <text class="card-title">最近消费</text>
      </view>
      <view v-if="recentOrders.length === 0" class="empty-state">
        <text class="empty-text">暂无消费记录</text>
      </view>
      <view v-else>
        <view v-for="order in recentOrders" :key="order.id" class="order-item">
          <view class="flex-between">
            <view>
              <text class="order-table">{{ order.tableNumber }}</text>
              <text class="order-time">{{ order.date }}</text>
            </view>
            <text class="order-amount">¥{{ order.amount }}</text>
          </view>
        </view>
      </view>
    </view>
  </view>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const currentSession = ref(null);
const recentOrders = ref([]);

onMounted(() => {
  loadData();
});

const loadData = async () => {
  // TODO: 从API加载数据
};

const goToScan = () => {
  uni.navigateTo({
    url: '/pages/scan/scan'
  });
};

const goToMine = () => {
  uni.switchTab({
    url: '/pages/mine/mine'
  });
};

const goToSession = () => {
  uni.navigateTo({
    url: '/pages/session/session'
  });
};
</script>

<style scoped>
.container {
  padding: 20rpx;
  min-height: 100vh;
}

.banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20rpx;
  padding: 60rpx 40rpx;
  margin-bottom: 40rpx;
  text-align: center;
}

.banner-title {
  display: block;
  color: #ffffff;
  font-size: 44rpx;
  font-weight: bold;
  margin-bottom: 20rpx;
}

.banner-subtitle {
  display: block;
  color: rgba(255, 255, 255, 0.9);
  font-size: 28rpx;
}

.quick-actions {
  display: flex;
  justify-content: space-between;
  margin-bottom: 40rpx;
  gap: 20rpx;
}

.action-card {
  flex: 1;
  background-color: #ffffff;
  border-radius: 20rpx;
  padding: 40rpx 20rpx;
  text-align: center;
  box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.08);
}

.action-icon {
  display: block;
  font-size: 80rpx;
  margin-bottom: 20rpx;
}

.action-title {
  display: block;
  font-size: 28rpx;
  color: #333333;
}

.card-header {
  margin-bottom: 20rpx;
}

.card-title {
  font-size: 32rpx;
  font-weight: bold;
  color: #333333;
}

.empty-state {
  padding: 60rpx 0;
  text-align: center;
}

.empty-text {
  color: #999999;
  font-size: 28rpx;
}

.session-info {
  padding: 20rpx 0;
}

.session-row {
  margin-bottom: 20rpx;
  font-size: 28rpx;
}

.order-item {
  padding: 30rpx 0;
  border-bottom: 1rpx solid #eeeeee;
}

.order-item:last-child {
  border-bottom: none;
}

.order-table {
  display: block;
  font-size: 28rpx;
  color: #333333;
  margin-bottom: 10rpx;
}

.order-time {
  display: block;
  font-size: 24rpx;
  color: #999999;
}

.order-amount {
  font-size: 32rpx;
  font-weight: bold;
  color: #fa5151;
}
</style>
