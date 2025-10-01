/**
 * 计费会话相关API
 */
import { get, post, put } from '@/utils/request';

/**
 * 开启计费会话
 * @param {Object} data 会话数据
 */
export function startSession(data) {
  return post('/api/sessions/start', data);
}

/**
 * 获取会话详情
 * @param {string} sessionId 会话ID
 */
export function getSessionDetail(sessionId) {
  return get(`/api/sessions/${sessionId}`);
}

/**
 * 获取当前活动会话
 */
export function getCurrentSession() {
  return get('/api/sessions/current');
}

/**
 * 暂停会话
 * @param {string} sessionId 会话ID
 */
export function pauseSession(sessionId) {
  return put(`/api/sessions/${sessionId}/pause`);
}

/**
 * 继续会话
 * @param {string} sessionId 会话ID
 */
export function resumeSession(sessionId) {
  return put(`/api/sessions/${sessionId}/resume`);
}

/**
 * 结束会话
 * @param {string} sessionId 会话ID
 */
export function endSession(sessionId) {
  return put(`/api/sessions/${sessionId}/end`);
}

/**
 * 获取会话列表
 * @param {Object} params 查询参数
 */
export function getSessionList(params) {
  return get('/api/sessions', params);
}
