/**
 * 图书相关API
 */
import { get, post, put, del } from '@/utils/request';

/**
 * 获取图书列表
 * @param {Object} params 查询参数
 * @param {number} params.skipCount 跳过数量
 * @param {number} params.maxResultCount 最大返回数量
 * @param {string} params.sorting 排序字段
 */
export function getBookList(params) {
  return get('/api/app/book', params);
}

/**
 * 获取图书详情
 * @param {string} id 图书ID
 */
export function getBook(id) {
  return get(`/api/app/book/${id}`);
}

/**
 * 创建图书
 * @param {Object} data 图书数据
 * @param {string} data.name 图书名称
 * @param {number} data.type 图书类型
 * @param {string} data.publishDate 出版日期
 * @param {number} data.price 价格
 */
export function createBook(data) {
  return post('/api/app/book', data);
}

/**
 * 更新图书
 * @param {string} id 图书ID
 * @param {Object} data 图书数据
 */
export function updateBook(id, data) {
  return put(`/api/app/book/${id}`, data);
}

/**
 * 删除图书
 * @param {string} id 图书ID
 */
export function deleteBook(id) {
  return del(`/api/app/book/${id}`);
}
