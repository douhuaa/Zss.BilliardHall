<script setup>
import { onLaunch, onShow, onHide } from '@dcloudio/uni-app';

onLaunch(() => {
  console.log('App Launch');
});

onShow(() => {
  console.log('App Show');
});

onHide(() => {
  console.log('App Hide');
});
</script>

<style>
/* 全局样式 */
page {
  background-color: #f8f8f8;
  font-size: 16px;
  line-height: 1.6;
}

/* 通用布局 */
.container {
  padding: 20rpx;
}

.card {
  background-color: #ffffff;
  border-radius: 10rpx;
  padding: 20rpx;
  margin-bottom: 20rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.1);
}

/* 通用按钮 */
.btn-primary {
  background-color: #07c160;
  color: #ffffff;
  border-radius: 10rpx;
  padding: 20rpx;
  text-align: center;
  font-size: 32rpx;
}

.btn-secondary {
  background-color: #ffffff;
  color: #07c160;
  border: 2rpx solid #07c160;
  border-radius: 10rpx;
  padding: 20rpx;
  text-align: center;
  font-size: 32rpx;
}

/* 文字样式 */
.text-primary {
  color: #07c160;
}

.text-secondary {
  color: #999999;
}

.text-danger {
  color: #fa5151;
}

.text-center {
  text-align: center;
}

/* Flex布局 */
.flex {
  display: flex;
}

.flex-between {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.flex-center {
  display: flex;
  justify-content: center;
  align-items: center;
}

.flex-column {
  display: flex;
  flex-direction: column;
}
</style>
