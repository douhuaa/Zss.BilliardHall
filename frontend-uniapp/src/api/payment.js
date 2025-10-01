/**
 * 支付相关API
 */
import { get, post } from '@/utils/request';

/**
 * 创建支付订单
 * @param {Object} data 订单数据
 */
export function createPayment(data) {
  return post('/api/payments/create', data);
}

/**
 * 微信支付
 * @param {Object} data 支付数据
 */
export function wechatPay(data) {
  return post('/api/payments/wechat', data);
}

/**
 * 支付宝支付
 * @param {Object} data 支付数据
 */
export function alipayPay(data) {
  return post('/api/payments/alipay', data);
}

/**
 * 余额支付
 * @param {Object} data 支付数据
 */
export function balancePay(data) {
  return post('/api/payments/balance', data);
}

/**
 * 查询支付状态
 * @param {string} orderId 订单ID
 */
export function getPaymentStatus(orderId) {
  return get(`/api/payments/${orderId}/status`);
}

/**
 * 获取支付记录
 * @param {Object} params 查询参数
 */
export function getPaymentHistory(params) {
  return get('/api/payments/history', params);
}
