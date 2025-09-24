<template>
  <view class="container">
    <view class="header">
      <view class="welcome">
        <text class="title">欢迎使用自助台球厅</text>
        <text class="subtitle">扫码开台，畅享台球时光</text>
      </view>
    </view>

    <view class="menu-grid">
      <view class="menu-item" @click="scanQRCode">
        <view class="menu-icon">
          <text class="icon-scan">📷</text>
        </view>
        <text class="menu-text">扫码开台</text>
      </view>

      <view class="menu-item" @click="viewTables">
        <view class="menu-icon">
          <text class="icon-table">🎱</text>
        </view>
        <text class="menu-text">台球桌状态</text>
      </view>

      <view class="menu-item" @click="viewHistory">
        <view class="menu-icon">
          <text class="icon-history">📋</text>
        </view>
        <text class="menu-text">历史记录</text>
      </view>

      <view class="menu-item" @click="viewProfile">
        <view class="menu-icon">
          <text class="icon-profile">👤</text>
        </view>
        <text class="menu-text">个人中心</text>
      </view>
    </view>

    <view class="current-session" v-if="currentSession">
      <view class="session-header">
        <text class="session-title">当前游戏</text>
      </view>
      <view class="session-content">
        <view class="session-info">
          <text class="session-table">{{ currentSession.tableName }}</text>
          <text class="session-time">已游戏 {{ formatDuration(currentSession.duration) }}</text>
        </view>
        <view class="session-actions">
          <button class="btn btn-primary" @click="continueGame">继续游戏</button>
          <button class="btn btn-secondary" @click="endGame">结束游戏</button>
        </view>
      </view>
    </view>

    <view class="notice" v-if="notices.length > 0">
      <view class="notice-header">
        <text class="notice-title">系统公告</text>
      </view>
      <swiper class="notice-swiper" :autoplay="true" :interval="3000" :duration="500">
        <swiper-item v-for="notice in notices" :key="notice.id">
          <text class="notice-text">{{ notice.content }}</text>
        </swiper-item>
      </swiper>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

// 接口类型定义
interface CurrentSession {
  id: string
  tableName: string
  duration: number
}

interface Notice {
  id: string
  content: string
}

// 响应式数据
const currentSession = ref<CurrentSession | null>(null)
const notices = ref<Notice[]>([])

// 页面加载时获取数据
onMounted(async () => {
  await loadCurrentSession()
  await loadNotices()
})

// 加载当前会话
const loadCurrentSession = async () => {
  try {
    // 模拟API调用
    // const session = await getCurrentSession()
    // currentSession.value = session
    console.log('加载当前会话')
  } catch (error) {
    console.log('没有进行中的会话')
  }
}

// 加载系统公告
const loadNotices = async () => {
  try {
    // 模拟数据
    notices.value = [
      { id: '1', content: '欢迎使用自助台球系统，享受便捷服务！' },
      { id: '2', content: '系统维护通知：今晚22:00-23:00进行系统维护' }
    ]
  } catch (error) {
    console.error('获取公告失败:', error)
  }
}

// 扫码开台
const scanQRCode = () => {
  uni.navigateTo({
    url: '/pages/scan/scan'
  })
}

// 查看台球桌状态
const viewTables = () => {
  uni.navigateTo({
    url: '/pages/table/table'
  })
}

// 查看历史记录
const viewHistory = () => {
  uni.switchTab({
    url: '/pages/history/history'
  })
}

// 查看个人中心
const viewProfile = () => {
  uni.switchTab({
    url: '/pages/profile/profile'
  })
}

// 继续游戏
const continueGame = () => {
  if (currentSession.value) {
    uni.navigateTo({
      url: `/pages/session/session?sessionId=${currentSession.value.id}`
    })
  }
}

// 结束游戏
const endGame = () => {
  if (currentSession.value) {
    uni.navigateTo({
      url: `/pages/payment/payment?sessionId=${currentSession.value.id}`
    })
  }
}

// 格式化时长
const formatDuration = (minutes: number): string => {
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  return hours > 0 ? `${hours}时${mins}分` : `${mins}分钟`
}
</script>

<style scoped>
.container {
  padding: 20rpx;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  min-height: 100vh;
}

.header {
  text-align: center;
  padding: 60rpx 0;
}

.welcome .title {
  display: block;
  font-size: 48rpx;
  font-weight: bold;
  color: #FFFFFF;
  margin-bottom: 20rpx;
}

.welcome .subtitle {
  font-size: 28rpx;
  color: rgba(255, 255, 255, 0.8);
}

.menu-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 30rpx;
  margin: 60rpx 0;
}

.menu-item {
  background: rgba(255, 255, 255, 0.95);
  padding: 60rpx 40rpx;
  border-radius: 20rpx;
  text-align: center;
  box-shadow: 0 8rpx 25rpx rgba(0, 0, 0, 0.1);
  transition: transform 0.2s;
}

.menu-item:hover {
  transform: translateY(-4rpx);
}

.menu-icon {
  margin-bottom: 20rpx;
}

.menu-icon text {
  font-size: 60rpx;
}

.menu-text {
  font-size: 28rpx;
  color: #333;
  font-weight: 500;
}

.current-session {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 20rpx;
  margin: 40rpx 0;
  overflow: hidden;
  box-shadow: 0 8rpx 25rpx rgba(0, 0, 0, 0.1);
}

.session-header {
  background: #007AFF;
  padding: 30rpx;
  text-align: center;
}

.session-title {
  font-size: 32rpx;
  font-weight: bold;
  color: #FFFFFF;
}

.session-content {
  padding: 40rpx;
}

.session-info {
  text-align: center;
  margin-bottom: 40rpx;
}

.session-table {
  display: block;
  font-size: 36rpx;
  font-weight: bold;
  color: #333;
  margin-bottom: 10rpx;
}

.session-time {
  font-size: 28rpx;
  color: #666;
}

.session-actions {
  display: flex;
  gap: 20rpx;
}

.btn {
  flex: 1;
  padding: 24rpx;
  border-radius: 12rpx;
  text-align: center;
  font-size: 28rpx;
  border: none;
}

.btn-primary {
  background: #007AFF;
  color: #FFFFFF;
}

.btn-secondary {
  background: #F0F0F0;
  color: #666;
}

.notice {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 20rpx;
  margin: 40rpx 0;
  overflow: hidden;
}

.notice-header {
  background: #FF9500;
  padding: 20rpx 30rpx;
}

.notice-title {
  font-size: 28rpx;
  font-weight: bold;
  color: #FFFFFF;
}

.notice-swiper {
  height: 80rpx;
}

.notice-text {
  display: block;
  padding: 0 30rpx;
  line-height: 80rpx;
  font-size: 26rpx;
  color: #666;
}
</style>