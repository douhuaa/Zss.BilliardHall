<template>
  <view class="container">
    <view class="scan-area">
      <view class="scan-box">
        <view class="scan-line"></view>
      </view>
      <text class="scan-tip">请将二维码放入框内</text>
    </view>
    
    <view class="bottom-actions">
      <button class="scan-btn" @tap="startScan" :disabled="scanning">
        {{ scanning ? '扫描中...' : '开始扫描' }}
      </button>
      <button class="manual-btn" @tap="manualInput">手动输入台号</button>
    </view>
    
    <!-- 手动输入模态框 -->
    <uni-popup ref="popup" type="bottom">
      <view class="popup-content">
        <view class="popup-header">
          <text class="popup-title">请输入台号</text>
          <text class="popup-close" @tap="closePopup">×</text>
        </view>
        <view class="input-section">
          <input 
            class="table-input" 
            v-model="tableNumber" 
            placeholder="请输入台号" 
            type="number"
          />
          <button class="confirm-btn" @tap="confirmTable">确认开台</button>
        </view>
      </view>
    </uni-popup>
  </view>
</template>

<script>
export default {
  data() {
    return {
      scanning: false,
      tableNumber: ''
    }
  },
  
  onLoad() {
    // 页面加载时自动开始扫描
    this.startScan()
  },
  
  methods: {
    startScan() {
      this.scanning = true
      
      uni.scanCode({
        success: (res) => {
          console.log('扫码结果:', res.result)
          this.handleScanResult(res.result)
        },
        fail: (err) => {
          console.error('扫码失败:', err)
          uni.showToast({
            title: '扫码失败，请重试',
            icon: 'none'
          })
        },
        complete: () => {
          this.scanning = false
        }
      })
    },
    
    handleScanResult(result) {
      // 解析二维码结果，提取台号
      try {
        // 假设二维码格式为: table_123 或者直接是数字
        let tableId = result
        if (result.startsWith('table_')) {
          tableId = result.replace('table_', '')
        }
        
        // 验证台号是否有效
        if (this.isValidTableId(tableId)) {
          this.openTable(tableId)
        } else {
          throw new Error('无效的台号')
        }
      } catch (error) {
        uni.showModal({
          title: '扫码错误',
          content: '无效的二维码，请重新扫描或手动输入台号',
          showCancel: false
        })
      }
    },
    
    isValidTableId(tableId) {
      // 验证台号是否为有效数字且在合理范围内
      const num = parseInt(tableId)
      return !isNaN(num) && num > 0 && num <= 100
    },
    
    openTable(tableId) {
      // 跳转到台桌会话页面
      uni.navigateTo({
        url: `/pages/session/session?tableId=${tableId}`
      })
    },
    
    manualInput() {
      this.$refs.popup.open()
    },
    
    closePopup() {
      this.$refs.popup.close()
      this.tableNumber = ''
    },
    
    confirmTable() {
      if (!this.tableNumber) {
        uni.showToast({
          title: '请输入台号',
          icon: 'none'
        })
        return
      }
      
      if (!this.isValidTableId(this.tableNumber)) {
        uni.showToast({
          title: '台号无效',
          icon: 'none'
        })
        return
      }
      
      this.closePopup()
      this.openTable(this.tableNumber)
    }
  }
}
</script>

<style>
.container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #000;
  color: white;
}

.scan-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
}

.scan-box {
  width: 500rpx;
  height: 500rpx;
  border: 4rpx solid #007AFF;
  border-radius: 20rpx;
  position: relative;
  overflow: hidden;
}

.scan-line {
  width: 100%;
  height: 4rpx;
  background: #007AFF;
  position: absolute;
  top: 0;
  animation: scanMove 2s infinite;
  box-shadow: 0 0 20rpx #007AFF;
}

@keyframes scanMove {
  0% { top: 0; }
  50% { top: 496rpx; }
  100% { top: 0; }
}

.scan-tip {
  margin-top: 60rpx;
  font-size: 28rpx;
  color: #ccc;
  text-align: center;
}

.bottom-actions {
  padding: 60rpx 40rpx;
  display: flex;
  flex-direction: column;
  gap: 30rpx;
}

.scan-btn {
  background: #007AFF;
  color: white;
  border: none;
  border-radius: 50rpx;
  padding: 30rpx;
  font-size: 32rpx;
}

.scan-btn[disabled] {
  background: #666;
}

.manual-btn {
  background: transparent;
  color: #007AFF;
  border: 2rpx solid #007AFF;
  border-radius: 50rpx;
  padding: 30rpx;
  font-size: 28rpx;
}

.popup-content {
  background: white;
  border-radius: 20rpx 20rpx 0 0;
  padding: 0;
}

.popup-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 40rpx;
  border-bottom: 1rpx solid #eee;
}

.popup-title {
  font-size: 32rpx;
  font-weight: bold;
  color: #333;
}

.popup-close {
  font-size: 48rpx;
  color: #999;
}

.input-section {
  padding: 40rpx;
}

.table-input {
  width: 100%;
  padding: 30rpx;
  border: 1rpx solid #ddd;
  border-radius: 12rpx;
  font-size: 32rpx;
  margin-bottom: 30rpx;
  text-align: center;
}

.confirm-btn {
  width: 100%;
  background: #007AFF;
  color: white;
  border: none;
  border-radius: 12rpx;
  padding: 30rpx;
  font-size: 32rpx;
}
</style>