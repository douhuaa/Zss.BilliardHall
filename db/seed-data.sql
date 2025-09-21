-- 种子数据 (Seed Data)
-- 用于开发和测试环境

SET NAMES utf8mb4;

-- 插入测试门店
INSERT INTO store (id, name, city, status) VALUES
(1, '测试台球厅1号店', '北京', 1),
(2, '测试台球厅2号店', '上海', 1);

-- 插入测试球台
INSERT INTO billiard_table (id, store_id, code, status) VALUES
(1, 1, 'T001', 0),
(2, 1, 'T002', 0), 
(3, 1, 'T003', 0),
(4, 2, 'T001', 0),
(5, 2, 'T002', 0);

-- 插入测试用户
INSERT INTO user (id, phone, nickname, level, source) VALUES
(1, '13800138001', '测试用户1', 0, 'qr_scan'),
(2, '13800138002', '测试用户2', 1, 'referral'),
(-1, NULL, 'SYSTEM', -1, 'system');

-- 插入测试设备
INSERT INTO device (id, store_id, table_id, type, sn, firmware_version, status) VALUES
(1, 1, 1, 1, 'DEV001', 'v1.0.0', 1),
(2, 1, 2, 1, 'DEV002', 'v1.0.0', 1),
(3, 1, 3, 1, 'DEV003', 'v1.0.0', 1),
(4, 2, 4, 1, 'DEV004', 'v1.0.0', 1),
(5, 2, 5, 1, 'DEV005', 'v1.0.0', 1);

-- 插入默认计费规则
INSERT INTO pricing_rule (store_id, name, rule_type, price_per_minute, active_from, active_to, status) VALUES
(1, '1号店标准计费', 1, 120, '2024-01-01 00:00:00', '2030-12-31 23:59:59', 1),
(2, '2号店标准计费', 1, 100, '2024-01-01 00:00:00', '2030-12-31 23:59:59', 1);

-- 插入测试事件（示例）
INSERT INTO event_log (event_type, biz_id, user_id, session_id, payload, occurred_at) VALUES
('system_start', 'SYS001', -1, NULL, '{"message": "System initialized", "version": "v1.0.0"}', NOW()),
('store_open', 'STORE001', -1, NULL, '{"store_id": 1, "operator": "admin"}', NOW()),
('store_open', 'STORE002', -1, NULL, '{"store_id": 2, "operator": "admin"}', NOW());

-- 提交事务
COMMIT;