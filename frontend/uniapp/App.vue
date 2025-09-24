<script>
import { createSSRApp } from 'vue'
export default {
  onLaunch: function() {
    console.log('App Launch')
    
    // 初始化应用配置
    this.initAppConfig()
  },
  onShow: function() {
    console.log('App Show')
  },
  onHide: function() {
    console.log('App Hide')
  },
  methods: {
    initAppConfig() {
      // 设置API基础URL
      uni.setStorageSync('apiBaseUrl', 'https://localhost:5001/api')
      
      // 设置默认请求头
      uni.setStorageSync('defaultHeaders', {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    }
  }
}
</script>

<style>
/*每个页面公共css */
page {
  background-color: #f5f5f5;
}

/* 通用样式 */
.btn-primary {
  background-color: #007AFF;
  color: white;
  border: none;
  border-radius: 8rpx;
  padding: 20rpx 40rpx;
  font-size: 28rpx;
}

.btn-secondary {
  background-color: transparent;
  color: #007AFF;
  border: 1rpx solid #007AFF;
  border-radius: 8rpx;
  padding: 20rpx 40rpx;
  font-size: 28rpx;
}

.text-center {
  text-align: center;
}

.mt-20 {
  margin-top: 20rpx;
}

.mb-20 {
  margin-bottom: 20rpx;
}

.p-20 {
  padding: 20rpx;
}
</style>