-- Schema v1.0 for V0.1 MVP (仅保留核心表)
-- MySQL 8+ DDL with UTF-8 support
-- 范围：开台→计费→支付→关台 闭环

SET NAMES utf8mb4;
SET time_zone = '+00:00';

-- ============================================
-- 核心业务表 (V0.1 必需)
-- ============================================

-- 门店表
CREATE TABLE store (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  name VARCHAR(100) NOT NULL COMMENT '门店名称',
  city VARCHAR(64) COMMENT '城市',
  status TINYINT NOT NULL DEFAULT 1 COMMENT '1=营业 0=停业',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB COMMENT='门店信息';

-- 球台表
CREATE TABLE billiard_table (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  store_id BIGINT NOT NULL COMMENT '所属门店',
  code VARCHAR(64) NOT NULL COMMENT '球台编号',
  status TINYINT NOT NULL DEFAULT 0 COMMENT '0=空闲 1=使用中 2=维修',
  last_session_id BIGINT NULL COMMENT '最近会话ID',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uk_table_code (store_id, code),
  INDEX idx_table_store (store_id),
  CONSTRAINT fk_table_store FOREIGN KEY (store_id) REFERENCES store(id)
) ENGINE=InnoDB COMMENT='台球桌信息';

-- 用户表
CREATE TABLE user (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  phone VARCHAR(32) UNIQUE COMMENT '手机号',
  nickname VARCHAR(64) COMMENT '昵称',
  level INT DEFAULT 0 COMMENT '用户等级',
  source VARCHAR(64) COMMENT '注册来源',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB COMMENT='用户信息';

-- 会话表（核心）
CREATE TABLE table_session (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  session_no VARCHAR(64) NOT NULL UNIQUE COMMENT '会话编号',
  session_token VARCHAR(128) NULL COMMENT '幂等令牌',
  table_id BIGINT NOT NULL COMMENT '球台ID',
  user_id BIGINT NOT NULL COMMENT '用户ID',
  start_time DATETIME NOT NULL COMMENT '开始时间',
  end_time DATETIME NULL COMMENT '结束时间',
  status TINYINT NOT NULL DEFAULT 0 COMMENT '0=进行中 1=已结束 2=已取消',
  total_minutes INT NULL COMMENT '总时长（分钟）',
  billing_amount BIGINT NULL COMMENT '计费金额（分）',
  pay_status TINYINT NOT NULL DEFAULT 0 COMMENT '0=未支付 1=已支付',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_session_table_status (table_id, status),
  INDEX idx_session_user (user_id),
  INDEX idx_session_token (session_token),
  CONSTRAINT fk_session_table FOREIGN KEY (table_id) REFERENCES billiard_table(id),
  CONSTRAINT fk_session_user FOREIGN KEY (user_id) REFERENCES user(id)
) ENGINE=InnoDB COMMENT='台球会话记录';

-- 计费快照表
CREATE TABLE billing_snapshot (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  session_id BIGINT NOT NULL COMMENT '会话ID',
  original_minutes INT NOT NULL COMMENT '原始分钟数',
  unit_price_cents BIGINT NOT NULL COMMENT '单价（分/分钟）',
  rule_applied JSON COMMENT '应用的规则',
  detail_breakdown JSON COMMENT '计费明细',
  final_amount BIGINT NOT NULL COMMENT '最终金额（分）',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_bs_session (session_id),
  CONSTRAINT fk_bs_session FOREIGN KEY (session_id) REFERENCES table_session(id)
) ENGINE=InnoDB COMMENT='计费快照';

-- 支付订单表
CREATE TABLE payment_order (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  payment_no VARCHAR(64) NOT NULL UNIQUE COMMENT '支付单号',
  session_id BIGINT NOT NULL COMMENT '会话ID',
  user_id BIGINT NOT NULL COMMENT '用户ID',
  amount BIGINT NOT NULL COMMENT '金额（分）',
  channel VARCHAR(32) NOT NULL COMMENT '支付渠道',
  status TINYINT NOT NULL DEFAULT 0 COMMENT '0=待支付 1=成功 2=失败 3=取消',
  request_payload JSON NULL COMMENT '请求参数',
  notify_payload JSON NULL COMMENT '回调参数',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_pay_session_status (session_id, status),
  INDEX idx_pay_user (user_id),
  CONSTRAINT fk_pay_session FOREIGN KEY (session_id) REFERENCES table_session(id),
  CONSTRAINT fk_pay_user FOREIGN KEY (user_id) REFERENCES user(id)
) ENGINE=InnoDB COMMENT='支付订单';

-- ============================================
-- 设备管理表
-- ============================================

-- 设备表
CREATE TABLE device (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  store_id BIGINT NOT NULL COMMENT '所属门店',
  table_id BIGINT NULL COMMENT '关联球台',
  type TINYINT NOT NULL COMMENT '1=灯控器 2=计时器',
  sn VARCHAR(64) NOT NULL UNIQUE COMMENT '设备序列号',
  firmware_version VARCHAR(64) COMMENT '固件版本',
  status TINYINT NOT NULL DEFAULT 1 COMMENT '1=在线 0=离线',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_dev_store (store_id),
  INDEX idx_dev_table (table_id),
  CONSTRAINT fk_dev_store FOREIGN KEY (store_id) REFERENCES store(id)
) ENGINE=InnoDB COMMENT='设备信息';

-- 设备心跳表
CREATE TABLE device_heartbeat (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  device_id BIGINT NOT NULL COMMENT '设备ID',
  ts DATETIME NOT NULL COMMENT '心跳时间戳',
  metrics_json JSON NULL COMMENT '设备指标',
  online TINYINT NOT NULL DEFAULT 1 COMMENT '在线状态',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_hb_device_ts (device_id, ts DESC),
  CONSTRAINT fk_hb_device FOREIGN KEY (device_id) REFERENCES device(id)
) ENGINE=InnoDB COMMENT='设备心跳记录'
PARTITION BY RANGE (YEAR(ts)) (
  PARTITION p2024 VALUES LESS THAN (2025),
  PARTITION p2025 VALUES LESS THAN (2026),
  PARTITION p_future VALUES LESS THAN MAXVALUE
);

-- ============================================
-- 事件追踪表
-- ============================================

-- 事件日志表
CREATE TABLE event_log (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  event_type VARCHAR(64) NOT NULL COMMENT '事件类型',
  biz_id VARCHAR(64) NULL COMMENT '业务ID',
  user_id BIGINT NULL COMMENT '用户ID',
  session_id BIGINT NULL COMMENT '会话ID',
  payload JSON NULL COMMENT '事件数据',
  occurred_at DATETIME NOT NULL COMMENT '事件发生时间',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_event_type_time (event_type, occurred_at),
  INDEX idx_event_session (session_id),
  INDEX idx_event_user (user_id)
) ENGINE=InnoDB COMMENT='事件追踪日志';

-- ============================================
-- 配置表（简化版）
-- ============================================

-- 基础计费规则（V0.1 仅支持固定单价）
CREATE TABLE pricing_rule (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  store_id BIGINT NULL COMMENT '门店ID（NULL=全局）',
  name VARCHAR(100) NOT NULL COMMENT '规则名称',
  rule_type TINYINT NOT NULL DEFAULT 1 COMMENT '1=固定单价',
  price_per_minute BIGINT NOT NULL COMMENT '每分钟价格（分）',
  active_from DATETIME NOT NULL COMMENT '生效开始时间',
  active_to DATETIME NOT NULL COMMENT '生效结束时间',
  status TINYINT NOT NULL DEFAULT 1 COMMENT '1=启用 0=禁用',
  is_deleted TINYINT NOT NULL DEFAULT 0,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_pricing_store_status (store_id, status, active_from, active_to)
) ENGINE=InnoDB COMMENT='计费规则';

-- ============================================
-- 幂等控制表
-- ============================================

-- 支付回调幂等表
CREATE TABLE payment_callback_idempotent (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  callback_key VARCHAR(128) NOT NULL UNIQUE COMMENT '回调唯一键',
  payment_no VARCHAR(64) NOT NULL COMMENT '支付单号',
  processed TINYINT NOT NULL DEFAULT 0 COMMENT '是否已处理',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_callback_payment (payment_no)
) ENGINE=InnoDB COMMENT='支付回调幂等控制';

-- ============================================
-- 初始数据
-- ============================================

-- 插入默认计费规则
INSERT INTO pricing_rule (name, rule_type, price_per_minute, active_from, active_to) VALUES
('默认计费', 1, 100, '2024-01-01 00:00:00', '2030-12-31 23:59:59');
