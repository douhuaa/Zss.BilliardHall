<template>
  <view class="container">
    <view class="header">
      <text class="title">自助台球系统</text>
      <text class="subtitle">智能化台球厅管理</text>
    </view>
    
    <view class="content">
      <view class="card-section">
        <text class="section-title">快速操作</text>
        
        <view class="action-grid">
          <view class="action-item" @tap="scanQR">
            <image class="action-icon" src="/static/scan-icon.png" />
            <text class="action-text">扫码开台</text>
          </view>
          
          <view class="action-item" @tap="viewHistory">
            <image class="action-icon" src="/static/history-icon.png" />
            <text class="action-text">消费记录</text>
          </view>
          
          <view class="action-item" @tap="viewProfile">
            <image class="action-icon" src="/static/profile-icon.png" />
            <text class="action-text">个人中心</text>
          </view>
          
          <view class="action-item" @tap="contactService">
            <image class="action-icon" src="/static/service-icon.png" />
            <text class="action-text">客服服务</text>
          </view>
        </view>
      </view>
      
      <view class="card-section">
        <text class="section-title">台桌状态</text>
        <view class="table-list">
          <view class="table-item" v-for="table in tables" :key="table.id" 
                :class="{'available': table.status === 'idle', 'occupied': table.status === 'inuse'}">
            <text class="table-number">{{ table.number }}号台</text>
            <text class="table-status">{{ getStatusText(table.status) }}</text>
          </view>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      tables: [
        { id: 1, number: 1, status: 'idle' },
        { id: 2, number: 2, status: 'inuse' },
        { id: 3, number: 3, status: 'idle' },
        { id: 4, number: 4, status: 'idle' }
      ]
    }
  },
  onLoad() {
    this.loadTables()
  },
  onPullDownRefresh() {
    this.loadTables()
    setTimeout(() => {
      uni.stopPullDownRefresh()
    }, 1000)
  },
  methods: {
    scanQR() {
      uni.scanCode({
        success: (res) => {
          console.log('扫码结果:', res.result)
          // 处理扫码结果
          this.handleScanResult(res.result)
        },
        fail: (err) => {
          console.error('扫码失败:', err)
          uni.showToast({
            title: '扫码失败',
            icon: 'none'
          })
        }
      })
    },
    
    handleScanResult(result) {
      // 解析扫码结果，跳转到相应页面
      uni.navigateTo({
        url: `/pages/session/session?tableId=${result}`
      })
    },
    
    viewHistory() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    
    viewProfile() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    
    contactService() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    
    loadTables() {
      // TODO: 从API加载台桌状态
      console.log('加载台桌状态')
    },
    
    getStatusText(status) {
      const statusMap = {
        'idle': '空闲',
        'inuse': '使用中',
        'maintenance': '维护中'
      }
      return statusMap[status] || '未知'
    }
  }
}
</script>

<style>
.container {
  padding: 20rpx;
  background-color: #f5f5f5;
  min-height: 100vh;
}

.header {
  text-align: center;
  margin-bottom: 40rpx;
}

.title {
  font-size: 48rpx;
  font-weight: bold;
  color: #333;
  display: block;
  margin-bottom: 10rpx;
}

.subtitle {
  font-size: 28rpx;
  color: #666;
}

.card-section {
  background: white;
  border-radius: 20rpx;
  padding: 30rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 4rpx 20rpx rgba(0,0,0,0.1);
}

.section-title {
  font-size: 32rpx;
  font-weight: bold;
  color: #333;
  margin-bottom: 30rpx;
  display: block;
}

.action-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30rpx;
}

.action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 30rpx;
  background: #f8f9fa;
  border-radius: 16rpx;
  transition: background-color 0.3s;
}

.action-item:active {
  background: #e9ecef;
}

.action-icon {
  width: 60rpx;
  height: 60rpx;
  margin-bottom: 15rpx;
}

.action-text {
  font-size: 26rpx;
  color: #333;
}

.table-list {
  display: flex;
  flex-wrap: wrap;
  gap: 20rpx;
}

.table-item {
  flex: 1;
  min-width: 200rpx;
  padding: 20rpx;
  border-radius: 12rpx;
  text-align: center;
  border: 2rpx solid #ddd;
}

.table-item.available {
  background: #d4edda;
  border-color: #28a745;
}

.table-item.occupied {
  background: #f8d7da;
  border-color: #dc3545;
}

.table-number {
  font-size: 28rpx;
  font-weight: bold;
  display: block;
  margin-bottom: 10rpx;
}

.table-status {
  font-size: 24rpx;
  color: #666;
}
</style>