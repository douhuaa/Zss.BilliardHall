/**
 * HTTP请求封装
 */

const BASE_URL = process.env.VUE_APP_API_URL || 'http://localhost:5000';

/**
 * 发送HTTP请求
 * @param {Object} options 请求配置
 */
export function request(options) {
  return new Promise((resolve, reject) => {
    const token = uni.getStorageSync('token');
    
    uni.request({
      url: BASE_URL + options.url,
      method: options.method || 'GET',
      data: options.data || {},
      header: {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
        ...options.header
      },
      success: (res) => {
        if (res.statusCode === 200) {
          resolve(res.data);
        } else if (res.statusCode === 401) {
          // 未授权，跳转登录
          uni.removeStorageSync('token');
          uni.navigateTo({
            url: '/pages/login/login'
          });
          reject(new Error('未授权，请重新登录'));
        } else {
          reject(new Error(res.data.message || '请求失败'));
        }
      },
      fail: (err) => {
        reject(err);
      }
    });
  });
}

/**
 * GET请求
 */
export function get(url, params) {
  return request({
    url,
    method: 'GET',
    data: params
  });
}

/**
 * POST请求
 */
export function post(url, data) {
  return request({
    url,
    method: 'POST',
    data
  });
}

/**
 * PUT请求
 */
export function put(url, data) {
  return request({
    url,
    method: 'PUT',
    data
  });
}

/**
 * DELETE请求
 */
export function del(url, params) {
  return request({
    url,
    method: 'DELETE',
    data: params
  });
}
