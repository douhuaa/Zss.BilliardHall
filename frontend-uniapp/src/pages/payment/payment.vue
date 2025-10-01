<template>
  <view class="container">
    <view class="card order-info">
      <view class="order-row flex-between">
        <text class="label">订单编号</text>
        <text class="value">{{ orderNo }}</text>
      </view>
      <view class="order-row flex-between">
        <text class="label">球台号</text>
        <text class="value">{{ tableNumber }}</text>
      </view>
      <view class="order-row flex-between">
        <text class="label">使用时长</text>
        <text class="value">{{ duration }}</text>
      </view>
    </view>

    <view class="card amount-card">
      <text class="amount-label">应付金额</text>
      <text class="amount-value">¥{{ amount }}</text>
    </view>

    <view class="card payment-methods">
      <text class="section-title">支付方式</text>
      <view class="method-list">
        <view
          v-for="method in paymentMethods"
          :key="method.id"
          class="method-item"
          :class="{ active: selectedMethod === method.id }"
          @click="selectedMethod = method.id"
        >
          <text class="method-icon">{{ method.icon }}</text>
          <text class="method-name">{{ method.name }}</text>
          <text class="method-radio">{{ selectedMethod === method.id ? '✓' : '' }}</text>
        </view>
      </view>
    </view>

    <view class="bottom-bar">
      <view class="total">
        <text class="total-label">合计：</text>
        <text class="total-amount">¥{{ amount }}</text>
      </view>
      <button class="pay-btn" @click="handlePay">立即支付</button>
    </view>
  </view>
</template>

<script setup>
import { ref, onLoad } from 'vue';

const orderNo = ref('202401150001');
const tableNumber = ref('5号台');
const duration = ref('2小时30分钟');
const amount = ref('0.00');
const selectedMethod = ref('wechat');

const paymentMethods = ref([
  { id: 'wechat', name: '微信支付', icon: '💚' },
  { id: 'alipay', name: '支付宝', icon: '💙' },
  { id: 'balance', name: '余额支付', icon: '💰' }
]);

onLoad((options) => {
  if (options.amount) {
    amount.value = options.amount;
  }
  // TODO: 从API加载订单详情
});

const handlePay = async () => {
  uni.showLoading({ title: '支付中...' });

  try {
    // TODO: 调用支付API
    switch (selectedMethod.value) {
      case 'wechat':
        await wechatPay();
        break;
      case 'alipay':
        await alipayPay();
        break;
      case 'balance':
        await balancePay();
        break;
    }

    uni.hideLoading();
    uni.showToast({
      title: '支付成功',
      icon: 'success'
    });

    setTimeout(() => {
      uni.switchTab({
        url: '/pages/index/index'
      });
    }, 1500);
  } catch (error) {
    uni.hideLoading();
    uni.showToast({
      title: error.message || '支付失败',
      icon: 'none'
    });
  }
};

const wechatPay = async () => {
  // TODO: 实现微信支付
  return new Promise((resolve) => {
    setTimeout(resolve, 1000);
  });
};

const alipayPay = async () => {
  // TODO: 实现支付宝支付
  return new Promise((resolve) => {
    setTimeout(resolve, 1000);
  });
};

const balancePay = async () => {
  // TODO: 实现余额支付
  return new Promise((resolve) => {
    setTimeout(resolve, 1000);
  });
};
</script>

<style scoped>
.container {
  padding: 20rpx;
  min-height: 100vh;
  padding-bottom: 200rpx;
}

.order-info {
  padding: 40rpx;
}

.order-row {
  margin-bottom: 30rpx;
  font-size: 28rpx;
}

.order-row:last-child {
  margin-bottom: 0;
}

.label {
  color: #666666;
}

.value {
  color: #333333;
  font-weight: bold;
}

.amount-card {
  text-align: center;
  padding: 60rpx 40rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
}

.amount-label {
  display: block;
  font-size: 28rpx;
  margin-bottom: 20rpx;
  opacity: 0.9;
}

.amount-value {
  display: block;
  font-size: 80rpx;
  font-weight: bold;
}

.payment-methods {
  padding: 40rpx;
}

.section-title {
  display: block;
  font-size: 32rpx;
  font-weight: bold;
  color: #333333;
  margin-bottom: 30rpx;
}

.method-list {
}

.method-item {
  display: flex;
  align-items: center;
  padding: 30rpx 20rpx;
  border: 2rpx solid #e0e0e0;
  border-radius: 10rpx;
  margin-bottom: 20rpx;
}

.method-item.active {
  border-color: #07c160;
  background-color: #f0fff4;
}

.method-icon {
  font-size: 48rpx;
  margin-right: 20rpx;
}

.method-name {
  flex: 1;
  font-size: 28rpx;
  color: #333333;
}

.method-radio {
  width: 40rpx;
  height: 40rpx;
  line-height: 40rpx;
  text-align: center;
  border: 2rpx solid #e0e0e0;
  border-radius: 50%;
  color: #07c160;
  font-weight: bold;
}

.method-item.active .method-radio {
  border-color: #07c160;
}

.bottom-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: #ffffff;
  padding: 20rpx 40rpx;
  box-shadow: 0 -4rpx 20rpx rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.total {
  flex: 1;
}

.total-label {
  font-size: 28rpx;
  color: #666666;
}

.total-amount {
  font-size: 40rpx;
  font-weight: bold;
  color: #fa5151;
}

.pay-btn {
  width: 300rpx;
  height: 80rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  font-size: 32rpx;
  border-radius: 40rpx;
  border: none;
  line-height: 80rpx;
  padding: 0;
}
</style>
