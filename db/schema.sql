-- 自助台球系统数据库初始化脚本 v1.0
-- 仅包含 V0.1 所需核心表

SET FOREIGN_KEY_CHECKS = 0;

-- 门店表
CREATE TABLE `store` (
    `id` int NOT NULL AUTO_INCREMENT,
    `name` varchar(100) NOT NULL COMMENT '门店名称',
    `address` varchar(500) DEFAULT NULL COMMENT '门店地址',
    `phone` varchar(20) DEFAULT NULL COMMENT '联系电话',
    `status` enum('active','inactive') NOT NULL DEFAULT 'active',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_store_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='门店表';

-- 球台表
CREATE TABLE `billiard_table` (
    `id` int NOT NULL AUTO_INCREMENT,
    `store_id` int NOT NULL COMMENT '所属门店',
    `table_number` varchar(20) NOT NULL COMMENT '球台编号',
    `qr_code` varchar(100) NOT NULL COMMENT '二维码标识',
    `status` enum('idle','in_use','reserved','maintenance') NOT NULL DEFAULT 'idle',
    `device_id` varchar(50) DEFAULT NULL COMMENT '关联设备ID',
    `hourly_rate` decimal(10,2) NOT NULL DEFAULT '50.00' COMMENT '小时费率',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_store_table` (`store_id`, `table_number`),
    UNIQUE KEY `uk_qr_code` (`qr_code`),
    KEY `idx_table_status` (`status`),
    KEY `idx_device_id` (`device_id`),
    FOREIGN KEY (`store_id`) REFERENCES `store`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='球台表';

-- 用户表
CREATE TABLE `user` (
    `id` int NOT NULL AUTO_INCREMENT,
    `openid` varchar(100) DEFAULT NULL COMMENT '微信openid',
    `unionid` varchar(100) DEFAULT NULL COMMENT '微信unionid',
    `phone` varchar(20) DEFAULT NULL COMMENT '手机号',
    `nickname` varchar(50) DEFAULT NULL COMMENT '昵称',
    `avatar` varchar(500) DEFAULT NULL COMMENT '头像URL',
    `user_type` enum('guest','member') NOT NULL DEFAULT 'guest',
    `balance` decimal(10,2) NOT NULL DEFAULT '0.00' COMMENT '账户余额',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_openid` (`openid`),
    UNIQUE KEY `uk_unionid` (`unionid`),
    UNIQUE KEY `uk_phone` (`phone`),
    KEY `idx_user_type` (`user_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户表';

-- 台次会话表
CREATE TABLE `table_session` (
    `id` int NOT NULL AUTO_INCREMENT,
    `session_id` varchar(50) NOT NULL COMMENT '会话唯一标识',
    `table_id` int NOT NULL COMMENT '球台ID',
    `user_id` int NOT NULL COMMENT '用户ID',
    `store_id` int NOT NULL COMMENT '门店ID',
    `start_time` timestamp NOT NULL COMMENT '开始时间',
    `end_time` timestamp NULL DEFAULT NULL COMMENT '结束时间',
    `duration_minutes` int DEFAULT NULL COMMENT '使用时长(分钟)',
    `status` enum('created','active','billing','timeout','completed') NOT NULL DEFAULT 'created',
    `session_token` varchar(100) DEFAULT NULL COMMENT '幂等令牌',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_session_id` (`session_id`),
    UNIQUE KEY `uk_session_token` (`session_token`),
    KEY `idx_table_id` (`table_id`),
    KEY `idx_user_id` (`user_id`),
    KEY `idx_store_id` (`store_id`),
    KEY `idx_status` (`status`),
    KEY `idx_start_time` (`start_time`),
    FOREIGN KEY (`table_id`) REFERENCES `billiard_table`(`id`),
    FOREIGN KEY (`user_id`) REFERENCES `user`(`id`),
    FOREIGN KEY (`store_id`) REFERENCES `store`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='台次会话表';

-- 支付订单表
CREATE TABLE `payment_order` (
    `id` int NOT NULL AUTO_INCREMENT,
    `payment_id` varchar(50) NOT NULL COMMENT '支付单号',
    `session_id` varchar(50) NOT NULL COMMENT '关联会话ID',
    `user_id` int NOT NULL COMMENT '用户ID',
    `amount` decimal(10,2) NOT NULL COMMENT '支付金额',
    `channel` enum('wechat','alipay','balance') NOT NULL COMMENT '支付渠道',
    `status` enum('created','paying','success','failed','expired') NOT NULL DEFAULT 'created',
    `third_party_order_id` varchar(100) DEFAULT NULL COMMENT '第三方订单号',
    `callback_received_at` timestamp NULL DEFAULT NULL COMMENT '回调接收时间',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_payment_id` (`payment_id`),
    KEY `idx_session_id` (`session_id`),
    KEY `idx_user_id` (`user_id`),
    KEY `idx_status` (`status`),
    KEY `idx_third_party_order_id` (`third_party_order_id`),
    FOREIGN KEY (`user_id`) REFERENCES `user`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='支付订单表';

-- 计费快照表
CREATE TABLE `billing_snapshot` (
    `id` int NOT NULL AUTO_INCREMENT,
    `session_id` varchar(50) NOT NULL COMMENT '会话ID',
    `snapshot_time` timestamp NOT NULL COMMENT '快照时间',
    `duration_minutes` int NOT NULL COMMENT '累计时长(分钟)',
    `base_amount` decimal(10,2) NOT NULL COMMENT '基础金额',
    `adjusted_amount` decimal(10,2) NOT NULL COMMENT '调整后金额',
    `discount_amount` decimal(10,2) DEFAULT '0.00' COMMENT '优惠金额',
    `final_amount` decimal(10,2) NOT NULL COMMENT '最终金额',
    `billing_rules` json DEFAULT NULL COMMENT '计费规则详情',
    `is_final` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否最终快照',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_session_id` (`session_id`),
    KEY `idx_snapshot_time` (`snapshot_time`),
    KEY `idx_is_final` (`is_final`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='计费快照表';

-- 设备表
CREATE TABLE `device` (
    `id` int NOT NULL AUTO_INCREMENT,
    `device_id` varchar(50) NOT NULL COMMENT '设备唯一标识',
    `device_type` enum('table_terminal','gateway','sensor') NOT NULL COMMENT '设备类型',
    `table_id` int DEFAULT NULL COMMENT '关联球台ID',
    `store_id` int NOT NULL COMMENT '所属门店',
    `status` enum('online','offline','warning','maintenance') NOT NULL DEFAULT 'offline',
    `firmware_version` varchar(20) DEFAULT NULL COMMENT '固件版本',
    `ip_address` varchar(45) DEFAULT NULL COMMENT 'IP地址',
    `last_heartbeat` timestamp NULL DEFAULT NULL COMMENT '最后心跳时间',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_device_id` (`device_id`),
    KEY `idx_table_id` (`table_id`),
    KEY `idx_store_id` (`store_id`),
    KEY `idx_status` (`status`),
    KEY `idx_last_heartbeat` (`last_heartbeat`),
    FOREIGN KEY (`table_id`) REFERENCES `billiard_table`(`id`),
    FOREIGN KEY (`store_id`) REFERENCES `store`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='设备表';

-- 设备心跳表
CREATE TABLE `device_heartbeat` (
    `id` int NOT NULL AUTO_INCREMENT,
    `device_id` varchar(50) NOT NULL COMMENT '设备ID',
    `heartbeat_time` timestamp NOT NULL COMMENT '心跳时间',
    `online_status` tinyint(1) NOT NULL DEFAULT '1' COMMENT '在线状态',
    `latency_ms` int DEFAULT NULL COMMENT '延迟毫秒',
    `battery_level` int DEFAULT NULL COMMENT '电池电量',
    `signal_strength` int DEFAULT NULL COMMENT '信号强度',
    `metrics` json DEFAULT NULL COMMENT '其他指标',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_device_id` (`device_id`),
    KEY `idx_heartbeat_time` (`heartbeat_time`),
    KEY `idx_online_status` (`online_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='设备心跳表';

-- 事件日志表
CREATE TABLE `event_log` (
    `id` int NOT NULL AUTO_INCREMENT,
    `event_id` varchar(50) NOT NULL COMMENT '事件唯一标识',
    `event_name` varchar(50) NOT NULL COMMENT '事件名称',
    `event_time` timestamp NOT NULL COMMENT '事件时间',
    `user_id` int DEFAULT NULL COMMENT '关联用户ID',
    `session_id` varchar(50) DEFAULT NULL COMMENT '关联会话ID',
    `store_id` int DEFAULT NULL COMMENT '关联门店ID',
    `device_id` varchar(50) DEFAULT NULL COMMENT '关联设备ID',
    `event_data` json DEFAULT NULL COMMENT '事件数据',
    `correlation_id` varchar(50) DEFAULT NULL COMMENT '关联请求ID',
    `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_event_id` (`event_id`),
    KEY `idx_event_name` (`event_name`),
    KEY `idx_event_time` (`event_time`),
    KEY `idx_user_id` (`user_id`),
    KEY `idx_session_id` (`session_id`),
    KEY `idx_store_id` (`store_id`),
    KEY `idx_correlation_id` (`correlation_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='事件日志表';

SET FOREIGN_KEY_CHECKS = 1;

-- 初始化测试数据
INSERT INTO `store` (`name`, `address`, `phone`) VALUES 
('测试门店1', '北京市朝阳区xxx街道', '010-12345678');

INSERT INTO `billiard_table` (`store_id`, `table_number`, `qr_code`, `hourly_rate`) VALUES 
(1, 'T001', 'TBL001-QR789', 50.00),
(1, 'T002', 'TBL002-QR790', 50.00);

-- 创建索引优化查询性能
-- 复合索引
CREATE INDEX idx_session_table_time ON `table_session`(`table_id`, `start_time`);
CREATE INDEX idx_payment_session_status ON `payment_order`(`session_id`, `status`);
CREATE INDEX idx_billing_session_final ON `billing_snapshot`(`session_id`, `is_final`);
CREATE INDEX idx_event_name_time ON `event_log`(`event_name`, `event_time`);
CREATE INDEX idx_heartbeat_device_time ON `device_heartbeat`(`device_id`, `heartbeat_time`);

-- 数据库版本信息
INSERT INTO `event_log` (`event_id`, `event_name`, `event_time`, `event_data`) VALUES 
('schema_init_v1', 'schema_initialized', NOW(), '{"version": "v1.0", "tables_created": 9}');