<template>
  <view class="container">
    <view class="card table-info">
      <view class="table-number">{{ sessionData.tableNumber }}</view>
      <view class="table-status">计费中</view>
    </view>

    <view class="card time-info">
      <view class="info-row flex-between">
        <text class="label">开始时间</text>
        <text class="value">{{ sessionData.startTime }}</text>
      </view>
      <view class="info-row flex-between">
        <text class="label">已用时长</text>
        <text class="value highlight">{{ duration }}</text>
      </view>
      <view class="info-row flex-between">
        <text class="label">计费单价</text>
        <text class="value">{{ sessionData.pricePerHour }}元/小时</text>
      </view>
    </view>

    <view class="card amount-info">
      <text class="amount-label">当前费用</text>
      <text class="amount-value">¥{{ currentAmount }}</text>
    </view>

    <view class="action-buttons">
      <button class="btn-secondary" @click="pauseSession">暂停计费</button>
      <button class="btn-primary" @click="endSession">结束计费</button>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

const sessionData = ref({
  tableNumber: '5号台',
  startTime: '2024-01-15 14:30:00',
  pricePerHour: 30,
  status: 'active'
});

const startTimestamp = ref(Date.now());
const currentTime = ref(Date.now());
let timer = null;

onMounted(() => {
  // TODO: 从API加载会话数据
  loadSessionData();
  
  // 每秒更新时间
  timer = setInterval(() => {
    currentTime.value = Date.now();
  }, 1000);
});

onUnmounted(() => {
  if (timer) {
    clearInterval(timer);
  }
});

const loadSessionData = async () => {
  // TODO: 调用API获取会话详情
};

const duration = computed(() => {
  const seconds = Math.floor((currentTime.value - startTimestamp.value) / 1000);
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
});

const currentAmount = computed(() => {
  const hours = (currentTime.value - startTimestamp.value) / 1000 / 3600;
  return (hours * sessionData.value.pricePerHour).toFixed(2);
});

const pauseSession = () => {
  uni.showModal({
    title: '暂停计费',
    content: '确定要暂停当前计费吗？',
    success: (res) => {
      if (res.confirm) {
        // TODO: 调用暂停API
        uni.showToast({
          title: '已暂停计费',
          icon: 'success'
        });
      }
    }
  });
};

const endSession = () => {
  uni.showModal({
    title: '结束计费',
    content: '确定要结束当前计费并去支付吗？',
    success: (res) => {
      if (res.confirm) {
        uni.navigateTo({
          url: `/pages/payment/payment?amount=${currentAmount.value}`
        });
      }
    }
  });
};
</script>

<style scoped>
.container {
  padding: 20rpx;
  min-height: 100vh;
}

.table-info {
  text-align: center;
  padding: 60rpx 40rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
}

.table-number {
  font-size: 64rpx;
  font-weight: bold;
  margin-bottom: 20rpx;
}

.table-status {
  font-size: 28rpx;
  opacity: 0.9;
}

.time-info {
  padding: 40rpx;
}

.info-row {
  margin-bottom: 30rpx;
  font-size: 28rpx;
}

.info-row:last-child {
  margin-bottom: 0;
}

.label {
  color: #666666;
}

.value {
  color: #333333;
  font-weight: bold;
}

.value.highlight {
  color: #07c160;
  font-size: 32rpx;
}

.amount-info {
  text-align: center;
  padding: 60rpx 40rpx;
}

.amount-label {
  display: block;
  font-size: 28rpx;
  color: #666666;
  margin-bottom: 20rpx;
}

.amount-value {
  display: block;
  font-size: 80rpx;
  font-weight: bold;
  color: #fa5151;
}

.action-buttons {
  padding: 40rpx 0;
  display: flex;
  gap: 20rpx;
}

.action-buttons button {
  flex: 1;
}
</style>
