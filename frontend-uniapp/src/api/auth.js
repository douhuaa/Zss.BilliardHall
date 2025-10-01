/**
 * 认证相关API
 */
import { post } from '@/utils/request';

/**
 * 发送短信验证码
 * @param {string} phone 手机号
 */
export function sendSmsCode(phone) {
  return post('/api/auth/sms-code', { phone });
}

/**
 * 手机号验证码登录
 * @param {Object} data 登录数据
 */
export function loginWithSms(data) {
  return post('/api/auth/login/sms', data);
}

/**
 * 微信登录
 * @param {Object} data 微信用户信息
 */
export function loginWithWechat(data) {
  return post('/api/auth/login/wechat', data);
}

/**
 * 退出登录
 */
export function logout() {
  return post('/api/auth/logout');
}

/**
 * 刷新Token
 */
export function refreshToken() {
  return post('/api/auth/refresh-token');
}
