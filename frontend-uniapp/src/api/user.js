/**
 * 用户相关API
 */
import { get, put } from '@/utils/request';

/**
 * 获取用户信息
 */
export function getUserInfo() {
  return get('/api/user/info');
}

/**
 * 更新用户信息
 * @param {Object} data 用户信息
 */
export function updateUserInfo(data) {
  return put('/api/user/info', data);
}

/**
 * 获取账户余额
 */
export function getBalance() {
  return get('/api/user/balance');
}

/**
 * 获取积分
 */
export function getPoints() {
  return get('/api/user/points');
}

/**
 * 充值
 * @param {Object} data 充值数据
 */
export function recharge(data) {
  return post('/api/user/recharge', data);
}
