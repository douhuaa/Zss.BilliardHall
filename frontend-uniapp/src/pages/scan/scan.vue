<template>
  <view class="scan-container">
    <view class="scan-area">
      <view class="scan-box">
        <view class="scan-line"></view>
      </view>
      <text class="scan-tip">将二维码放入框内，即可自动扫描</text>
    </view>

    <view class="manual-input">
      <text class="manual-title">或手动输入台号</text>
      <view class="input-group">
        <input
          class="input"
          v-model="tableNumber"
          type="number"
          placeholder="请输入球台号"
        />
        <button class="confirm-btn" @click="confirmTable">确认</button>
      </view>
    </view>

    <button class="scan-btn" @click="startScan">开始扫码</button>
  </view>
</template>

<script setup>
import { ref } from 'vue';

const tableNumber = ref('');

const startScan = () => {
  uni.scanCode({
    success: (res) => {
      handleScanResult(res.result);
    },
    fail: (err) => {
      uni.showToast({
        title: '扫码失败',
        icon: 'none'
      });
    }
  });
};

const handleScanResult = (result) => {
  // TODO: 解析二维码，获取球台信息
  uni.navigateTo({
    url: `/pages/session/session?tableId=${result}`
  });
};

const confirmTable = () => {
  if (!tableNumber.value) {
    uni.showToast({
      title: '请输入球台号',
      icon: 'none'
    });
    return;
  }

  uni.navigateTo({
    url: `/pages/session/session?tableNumber=${tableNumber.value}`
  });
};
</script>

<style scoped>
.scan-container {
  min-height: 100vh;
  background-color: #000000;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 100rpx 40rpx;
}

.scan-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.scan-box {
  width: 500rpx;
  height: 500rpx;
  border: 4rpx solid #ffffff;
  border-radius: 20rpx;
  position: relative;
  overflow: hidden;
}

.scan-line {
  width: 100%;
  height: 4rpx;
  background: linear-gradient(90deg, transparent, #07c160, transparent);
  position: absolute;
  top: 0;
  animation: scan 2s linear infinite;
}

@keyframes scan {
  0% {
    top: 0;
  }
  100% {
    top: 100%;
  }
}

.scan-tip {
  color: #ffffff;
  font-size: 28rpx;
  margin-top: 40rpx;
}

.manual-input {
  width: 100%;
  background-color: #ffffff;
  border-radius: 20rpx;
  padding: 40rpx;
  margin-bottom: 40rpx;
}

.manual-title {
  display: block;
  text-align: center;
  color: #333333;
  font-size: 28rpx;
  margin-bottom: 30rpx;
}

.input-group {
  display: flex;
  gap: 20rpx;
}

.input {
  flex: 1;
  height: 80rpx;
  border: 1rpx solid #e0e0e0;
  border-radius: 10rpx;
  padding: 0 30rpx;
  font-size: 28rpx;
}

.confirm-btn {
  width: 160rpx;
  height: 80rpx;
  background-color: #07c160;
  color: #ffffff;
  font-size: 28rpx;
  border-radius: 10rpx;
  border: none;
  line-height: 80rpx;
  padding: 0;
}

.scan-btn {
  width: 100%;
  height: 90rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  font-size: 32rpx;
  border-radius: 10rpx;
  border: none;
}
</style>
