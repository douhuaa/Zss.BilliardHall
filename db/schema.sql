-- =============================================================================
-- 自助台球厅系统数据库初始化脚本 v1.0
-- 创建时间: 2024年9月
-- 数据库: MySQL 8.0+ / PostgreSQL 13+
-- 编码: UTF8MB4
-- 说明: 遵循第6章开发规范，使用中文注释
-- =============================================================================

-- 创建数据库（如果不存在）
CREATE DATABASE IF NOT EXISTS `billiard_hall` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci 
COMMENT '自助台球厅系统数据库';

USE `billiard_hall`;

-- =============================================================================
-- 1. 门店管理表
-- =============================================================================

-- 门店信息表
CREATE TABLE `stores` (
    `id` VARCHAR(36) NOT NULL COMMENT '门店ID（GUID）',
    `store_name` VARCHAR(100) NOT NULL COMMENT '门店名称',
    `store_code` VARCHAR(50) NOT NULL COMMENT '门店编号',
    `address` VARCHAR(200) DEFAULT NULL COMMENT '门店地址',
    `phone` VARCHAR(20) DEFAULT NULL COMMENT '联系电话',
    `manager_name` VARCHAR(50) DEFAULT NULL COMMENT '店长姓名',
    `business_hours` JSON DEFAULT NULL COMMENT '营业时间（JSON格式）',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    `creator_id` VARCHAR(36) DEFAULT NULL COMMENT '创建者ID',
    `last_modification_time` DATETIME(6) DEFAULT NULL COMMENT '最后修改时间',
    `last_modifier_id` VARCHAR(36) DEFAULT NULL COMMENT '最后修改者ID',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_stores_code` (`store_code`),
    INDEX `idx_stores_name` (`store_name`),
    INDEX `idx_stores_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='门店信息表';

-- =============================================================================
-- 2. 台球桌管理表
-- =============================================================================

-- 台球桌信息表
CREATE TABLE `billiard_tables` (
    `id` VARCHAR(36) NOT NULL COMMENT '台球桌ID（GUID）',
    `store_id` VARCHAR(36) NOT NULL COMMENT '门店ID',
    `table_number` VARCHAR(50) NOT NULL COMMENT '台球桌编号',
    `table_name` VARCHAR(100) NOT NULL COMMENT '台球桌名称',
    `status` INT NOT NULL DEFAULT 0 COMMENT '台球桌状态：0=空闲,1=使用中,2=已预约,3=维护中,4=故障',
    `location` VARCHAR(200) DEFAULT NULL COMMENT '位置描述',
    `base_price` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '基础价格（元/小时）',
    `device_id` VARCHAR(100) DEFAULT NULL COMMENT '设备ID',
    `qr_code_content` VARCHAR(500) DEFAULT NULL COMMENT '二维码内容',
    `last_heartbeat_time` DATETIME(6) DEFAULT NULL COMMENT '最后心跳时间',
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `remarks` VARCHAR(500) DEFAULT NULL COMMENT '备注',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    `creator_id` VARCHAR(36) DEFAULT NULL COMMENT '创建者ID',
    `last_modification_time` DATETIME(6) DEFAULT NULL COMMENT '最后修改时间',
    `last_modifier_id` VARCHAR(36) DEFAULT NULL COMMENT '最后修改者ID',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_tables_store_number` (`store_id`, `table_number`),
    INDEX `idx_tables_status` (`status`),
    INDEX `idx_tables_store` (`store_id`),
    INDEX `idx_tables_device` (`device_id`),
    CONSTRAINT `fk_tables_store` FOREIGN KEY (`store_id`) REFERENCES `stores`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='台球桌信息表';

-- =============================================================================
-- 3. 用户管理表
-- =============================================================================

-- 用户信息表（简化版，可与ABP用户系统集成）
CREATE TABLE `users` (
    `id` VARCHAR(36) NOT NULL COMMENT '用户ID（GUID）',
    `username` VARCHAR(50) NOT NULL COMMENT '用户名',
    `nickname` VARCHAR(50) DEFAULT NULL COMMENT '昵称',
    `phone` VARCHAR(20) DEFAULT NULL COMMENT '手机号',
    `avatar_url` VARCHAR(500) DEFAULT NULL COMMENT '头像URL',
    `wechat_openid` VARCHAR(100) DEFAULT NULL COMMENT '微信OpenID',
    `wechat_unionid` VARCHAR(100) DEFAULT NULL COMMENT '微信UnionID',
    `member_level` INT NOT NULL DEFAULT 0 COMMENT '会员等级：0=普通用户,1=银卡,2=金卡,3=钻石',
    `total_consumption` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '总消费金额',
    `balance` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '账户余额',
    `points` INT NOT NULL DEFAULT 0 COMMENT '积分',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    `last_login_time` DATETIME(6) DEFAULT NULL COMMENT '最后登录时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_users_username` (`username`),
    UNIQUE KEY `uk_users_phone` (`phone`),
    UNIQUE KEY `uk_users_wechat_openid` (`wechat_openid`),
    INDEX `idx_users_member_level` (`member_level`),
    INDEX `idx_users_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户信息表';

-- =============================================================================
-- 4. 会话管理表
-- =============================================================================

-- 台球会话表
CREATE TABLE `table_sessions` (
    `id` VARCHAR(36) NOT NULL COMMENT '会话ID（GUID）',
    `billiard_table_id` VARCHAR(36) NOT NULL COMMENT '台球桌ID',
    `user_id` VARCHAR(36) NOT NULL COMMENT '用户ID',
    `store_id` VARCHAR(36) NOT NULL COMMENT '门店ID',
    `status` INT NOT NULL DEFAULT 0 COMMENT '会话状态：0=进行中,1=已完成,2=已取消,3=待结算',
    `start_time` DATETIME(6) NOT NULL COMMENT '开始时间',
    `end_time` DATETIME(6) DEFAULT NULL COMMENT '结束时间',
    `billing_minutes` INT NOT NULL DEFAULT 0 COMMENT '计费时长（分钟）',
    `base_hourly_rate` DECIMAL(10,2) NOT NULL COMMENT '基础价格（元/小时）',
    `discount_amount` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '折扣金额',
    `total_amount` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '总金额',
    `actual_paid_amount` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '实际支付金额',
    `payment_status` INT NOT NULL DEFAULT 0 COMMENT '支付状态：0=待支付,1=支付成功,2=支付失败,3=已退款',
    `reservation_id` VARCHAR(36) DEFAULT NULL COMMENT '预约ID（如果是预约开台）',
    `remarks` VARCHAR(500) DEFAULT NULL COMMENT '备注',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    `creator_id` VARCHAR(36) DEFAULT NULL COMMENT '创建者ID',
    `last_modification_time` DATETIME(6) DEFAULT NULL COMMENT '最后修改时间',
    `last_modifier_id` VARCHAR(36) DEFAULT NULL COMMENT '最后修改者ID',
    PRIMARY KEY (`id`),
    INDEX `idx_sessions_table` (`billiard_table_id`),
    INDEX `idx_sessions_user` (`user_id`),
    INDEX `idx_sessions_store` (`store_id`),
    INDEX `idx_sessions_status` (`status`),
    INDEX `idx_sessions_start_time` (`start_time`),
    INDEX `idx_sessions_payment_status` (`payment_status`),
    CONSTRAINT `fk_sessions_table` FOREIGN KEY (`billiard_table_id`) REFERENCES `billiard_tables`(`id`),
    CONSTRAINT `fk_sessions_user` FOREIGN KEY (`user_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_sessions_store` FOREIGN KEY (`store_id`) REFERENCES `stores`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='台球会话表';

-- =============================================================================
-- 5. 支付管理表
-- =============================================================================

-- 支付订单表
CREATE TABLE `payment_orders` (
    `id` VARCHAR(36) NOT NULL COMMENT '支付订单ID（GUID）',
    `order_no` VARCHAR(50) NOT NULL COMMENT '订单号',
    `session_id` VARCHAR(36) NOT NULL COMMENT '会话ID',
    `user_id` VARCHAR(36) NOT NULL COMMENT '用户ID',
    `store_id` VARCHAR(36) NOT NULL COMMENT '门店ID',
    `payment_method` VARCHAR(50) NOT NULL COMMENT '支付方式：wechat,alipay,balance',
    `payment_channel` VARCHAR(50) DEFAULT NULL COMMENT '支付渠道：jsapi,native,app,h5',
    `order_amount` DECIMAL(10,2) NOT NULL COMMENT '订单金额',
    `paid_amount` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '实际支付金额',
    `status` INT NOT NULL DEFAULT 0 COMMENT '支付状态：0=待支付,1=支付成功,2=支付失败,3=已退款',
    `third_party_order_no` VARCHAR(100) DEFAULT NULL COMMENT '第三方订单号',
    `third_party_response` JSON DEFAULT NULL COMMENT '第三方响应数据（JSON格式）',
    `paid_time` DATETIME(6) DEFAULT NULL COMMENT '支付时间',
    `expired_time` DATETIME(6) DEFAULT NULL COMMENT '过期时间',
    `remarks` VARCHAR(500) DEFAULT NULL COMMENT '备注',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    `creator_id` VARCHAR(36) DEFAULT NULL COMMENT '创建者ID',
    `last_modification_time` DATETIME(6) DEFAULT NULL COMMENT '最后修改时间',
    `last_modifier_id` VARCHAR(36) DEFAULT NULL COMMENT '最后修改者ID',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_payment_order_no` (`order_no`),
    INDEX `idx_payment_session` (`session_id`),
    INDEX `idx_payment_user` (`user_id`),
    INDEX `idx_payment_store` (`store_id`),
    INDEX `idx_payment_status` (`status`),
    INDEX `idx_payment_method` (`payment_method`),
    INDEX `idx_payment_third_party` (`third_party_order_no`),
    CONSTRAINT `fk_payment_session` FOREIGN KEY (`session_id`) REFERENCES `table_sessions`(`id`),
    CONSTRAINT `fk_payment_user` FOREIGN KEY (`user_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_payment_store` FOREIGN KEY (`store_id`) REFERENCES `stores`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='支付订单表';

-- =============================================================================
-- 6. 设备管理表
-- =============================================================================

-- 设备心跳记录表
CREATE TABLE `device_heartbeats` (
    `id` VARCHAR(36) NOT NULL COMMENT '记录ID（GUID）',
    `device_id` VARCHAR(100) NOT NULL COMMENT '设备ID',
    `billiard_table_id` VARCHAR(36) DEFAULT NULL COMMENT '台球桌ID',
    `heartbeat_time` DATETIME(6) NOT NULL COMMENT '心跳时间',
    `device_status` JSON DEFAULT NULL COMMENT '设备状态数据（JSON格式）',
    `ip_address` VARCHAR(45) DEFAULT NULL COMMENT 'IP地址',
    `signal_strength` INT DEFAULT NULL COMMENT '信号强度',
    `battery_level` INT DEFAULT NULL COMMENT '电量水平',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_heartbeat_device` (`device_id`),
    INDEX `idx_heartbeat_table` (`billiard_table_id`),
    INDEX `idx_heartbeat_time` (`heartbeat_time`),
    CONSTRAINT `fk_heartbeat_table` FOREIGN KEY (`billiard_table_id`) REFERENCES `billiard_tables`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='设备心跳记录表';

-- =============================================================================
-- 7. 事件日志表
-- =============================================================================

-- 业务事件日志表
CREATE TABLE `event_logs` (
    `id` VARCHAR(36) NOT NULL COMMENT '事件ID（GUID）',
    `event_name` VARCHAR(100) NOT NULL COMMENT '事件名称',
    `event_type` VARCHAR(50) NOT NULL COMMENT '事件类型',
    `aggregate_id` VARCHAR(36) DEFAULT NULL COMMENT '聚合根ID',
    `aggregate_type` VARCHAR(100) DEFAULT NULL COMMENT '聚合根类型',
    `event_data` JSON DEFAULT NULL COMMENT '事件数据（JSON格式）',
    `user_id` VARCHAR(36) DEFAULT NULL COMMENT '用户ID',
    `store_id` VARCHAR(36) DEFAULT NULL COMMENT '门店ID',
    `session_id` VARCHAR(36) DEFAULT NULL COMMENT '会话ID',
    `platform` VARCHAR(50) DEFAULT NULL COMMENT '平台：web,miniapp,app',
    `ip_address` VARCHAR(45) DEFAULT NULL COMMENT 'IP地址',
    `user_agent` VARCHAR(500) DEFAULT NULL COMMENT '用户代理',
    `event_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '事件时间',
    `creation_time` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_events_name` (`event_name`),
    INDEX `idx_events_type` (`event_type`),
    INDEX `idx_events_aggregate` (`aggregate_id`, `aggregate_type`),
    INDEX `idx_events_user` (`user_id`),
    INDEX `idx_events_store` (`store_id`),
    INDEX `idx_events_session` (`session_id`),
    INDEX `idx_events_time` (`event_time`),
    CONSTRAINT `fk_events_user` FOREIGN KEY (`user_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_events_store` FOREIGN KEY (`store_id`) REFERENCES `stores`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='业务事件日志表';

-- =============================================================================
-- 8. 初始化数据
-- =============================================================================

-- 插入默认门店
INSERT INTO `stores` (`id`, `store_name`, `store_code`, `address`, `phone`, `manager_name`, `is_active`, `creation_time`) 
VALUES 
    ('550e8400-e29b-41d4-a716-446655440000', '总店', 'STORE001', '北京市朝阳区xxx街道xxx号', '010-12345678', '张经理', 1, NOW(6)),
    ('550e8400-e29b-41d4-a716-446655440001', '分店A', 'STORE002', '北京市海淀区xxx街道xxx号', '010-87654321', '李经理', 1, NOW(6));

-- 插入示例台球桌
INSERT INTO `billiard_tables` (`id`, `store_id`, `table_number`, `table_name`, `status`, `location`, `base_price`, `is_enabled`, `creation_time`) 
VALUES 
    ('660e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440000', '001', '1号台', 0, '一楼左侧', 50.00, 1, NOW(6)),
    ('660e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440000', '002', '2号台', 0, '一楼中间', 50.00, 1, NOW(6)),
    ('660e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440000', '003', '3号台', 0, '一楼右侧', 60.00, 1, NOW(6)),
    ('660e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440001', '001', '分店1号台', 0, '二楼VIP区', 80.00, 1, NOW(6));

-- 插入示例用户
INSERT INTO `users` (`id`, `username`, `nickname`, `phone`, `member_level`, `is_active`, `creation_time`) 
VALUES 
    ('770e8400-e29b-41d4-a716-446655440000', 'testuser001', '测试用户1', '13800138001', 0, 1, NOW(6)),
    ('770e8400-e29b-41d4-a716-446655440001', 'testuser002', '测试用户2', '13800138002', 1, 1, NOW(6));

-- =============================================================================
-- 创建完成
-- =============================================================================

-- 显示表创建结果
SELECT 
    TABLE_NAME AS '表名',
    TABLE_COMMENT AS '表注释',
    TABLE_ROWS AS '记录数'
FROM 
    INFORMATION_SCHEMA.TABLES 
WHERE 
    TABLE_SCHEMA = 'billiard_hall' 
    AND TABLE_TYPE = 'BASE TABLE'
ORDER BY 
    TABLE_NAME;

-- 输出完成信息
SELECT '自助台球厅系统数据库初始化完成！' AS '执行结果';
SELECT '数据库版本: v1.0' AS '版本信息';
SELECT DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s') AS '完成时间';