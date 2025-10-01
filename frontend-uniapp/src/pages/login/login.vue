<template>
  <view class="login-container">
    <view class="login-header">
      <text class="logo">🎱</text>
      <text class="app-name">台球厅管理系统</text>
      <text class="app-slogan">智能自助，便捷体验</text>
    </view>

    <view class="login-form">
      <view class="input-group">
        <input
          class="input"
          v-model="phone"
          type="number"
          maxlength="11"
          placeholder="请输入手机号"
        />
      </view>
      <view class="input-group">
        <input
          class="input code-input"
          v-model="code"
          type="number"
          maxlength="6"
          placeholder="请输入验证码"
        />
        <button
          class="code-btn"
          :disabled="codeCountdown > 0"
          @click="sendCode"
        >
          {{ codeCountdown > 0 ? `${codeCountdown}秒后重试` : '获取验证码' }}
        </button>
      </view>
      <button class="login-btn" @click="handleLogin" :disabled="!canLogin">
        登录
      </button>
    </view>

    <view class="agreement">
      <label @click="agreed = !agreed">
        <text class="checkbox">{{ agreed ? '✓' : ' ' }}</text>
        <text class="agreement-text">
          我已阅读并同意《用户协议》和《隐私政策》
        </text>
      </label>
    </view>
  </view>
</template>

<script setup>
import { ref, computed } from 'vue';

const phone = ref('');
const code = ref('');
const codeCountdown = ref(0);
const agreed = ref(false);

const canLogin = computed(() => {
  return phone.value.length === 11 && code.value.length === 6 && agreed.value;
});

const sendCode = async () => {
  if (phone.value.length !== 11) {
    uni.showToast({
      title: '请输入正确的手机号',
      icon: 'none'
    });
    return;
  }

  try {
    // TODO: 调用发送验证码API
    uni.showToast({
      title: '验证码已发送',
      icon: 'success'
    });
    
    codeCountdown.value = 60;
    const timer = setInterval(() => {
      codeCountdown.value--;
      if (codeCountdown.value <= 0) {
        clearInterval(timer);
      }
    }, 1000);
  } catch (error) {
    uni.showToast({
      title: '发送失败',
      icon: 'none'
    });
  }
};

const handleLogin = async () => {
  if (!canLogin.value) return;

  try {
    uni.showLoading({ title: '登录中...' });
    // TODO: 调用登录API
    uni.hideLoading();
    uni.showToast({
      title: '登录成功',
      icon: 'success'
    });
    
    setTimeout(() => {
      uni.switchTab({
        url: '/pages/index/index'
      });
    }, 1000);
  } catch (error) {
    uni.hideLoading();
    uni.showToast({
      title: '登录失败',
      icon: 'none'
    });
  }
};
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 80rpx 40rpx;
}

.login-header {
  text-align: center;
  margin-bottom: 100rpx;
}

.logo {
  display: block;
  font-size: 120rpx;
  margin-bottom: 30rpx;
}

.app-name {
  display: block;
  color: #ffffff;
  font-size: 48rpx;
  font-weight: bold;
  margin-bottom: 20rpx;
}

.app-slogan {
  display: block;
  color: rgba(255, 255, 255, 0.9);
  font-size: 28rpx;
}

.login-form {
  background-color: #ffffff;
  border-radius: 20rpx;
  padding: 60rpx 40rpx;
  margin-bottom: 40rpx;
}

.input-group {
  position: relative;
  margin-bottom: 30rpx;
}

.input {
  width: 100%;
  height: 90rpx;
  border: 1rpx solid #e0e0e0;
  border-radius: 10rpx;
  padding: 0 30rpx;
  font-size: 28rpx;
}

.code-input {
  padding-right: 200rpx;
}

.code-btn {
  position: absolute;
  right: 0;
  top: 0;
  height: 90rpx;
  width: 180rpx;
  background-color: transparent;
  color: #667eea;
  font-size: 24rpx;
  border: none;
  padding: 0;
  line-height: 90rpx;
}

.code-btn[disabled] {
  color: #999999;
}

.login-btn {
  width: 100%;
  height: 90rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  font-size: 32rpx;
  border-radius: 10rpx;
  border: none;
  margin-top: 40rpx;
}

.login-btn[disabled] {
  opacity: 0.6;
}

.agreement {
  text-align: center;
}

.checkbox {
  display: inline-block;
  width: 32rpx;
  height: 32rpx;
  line-height: 32rpx;
  text-align: center;
  border: 2rpx solid #ffffff;
  border-radius: 6rpx;
  color: #ffffff;
  margin-right: 10rpx;
  vertical-align: middle;
}

.agreement-text {
  color: rgba(255, 255, 255, 0.9);
  font-size: 24rpx;
  vertical-align: middle;
}
</style>
