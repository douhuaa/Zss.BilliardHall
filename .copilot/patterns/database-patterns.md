# 数据库设计模式 (Database Design Patterns)

## 表结构设计原则 (Table Structure Design Principles)

### 1. 命名约定 (Naming Conventions)

```sql
-- 表名：PascalCase，复数形式
CREATE TABLE BilliardTables (...)
CREATE TABLE Reservations (...)
CREATE TABLE Customers (...)

-- 字段名：PascalCase
CREATE TABLE BilliardTables (
    Id UNIQUEIDENTIFIER,
    TableNumber INT,
    HourlyRate DECIMAL(10,2),
    CreatedAt DATETIME2
);

-- 索引命名：IX_表名_字段名
CREATE INDEX IX_BilliardTables_Status ON BilliardTables(Status);
CREATE INDEX IX_BilliardTables_HallId_Status ON BilliardTables(HallId, Status);

-- 外键命名：FK_表名_引用表名
ALTER TABLE BilliardTables ADD CONSTRAINT FK_BilliardTables_BilliardHalls 
    FOREIGN KEY (HallId) REFERENCES BilliardHalls(Id);

-- 唯一约束：UK_表名_字段名
ALTER TABLE BilliardTables ADD CONSTRAINT UK_BilliardTables_HallId_Number 
    UNIQUE (HallId, TableNumber);
```

### 2. 标准字段模式 (Standard Field Patterns)

```sql
-- 基础实体表模板
CREATE TABLE {TableName} (
    -- 主键 (必须)
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    
    -- 审计字段 (必须)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- 软删除支持 (可选)
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(50) NULL,
    
    -- 版本控制 (并发控制)
    Version ROWVERSION NOT NULL,
    
    -- 业务字段
    -- ...
    
    -- 索引
    INDEX IX_{TableName}_CreatedAt (CreatedAt),
    INDEX IX_{TableName}_UpdatedAt (UpdatedAt),
    INDEX IX_{TableName}_IsDeleted (IsDeleted)
);
```

### 3. 数据类型选择指南

```sql
-- 字符串类型
Name NVARCHAR(100) NOT NULL,           -- 短文本，有长度限制
Description NVARCHAR(MAX) NULL,        -- 长文本，无长度限制
Code NVARCHAR(20) NOT NULL,            -- 代码/编号，固定格式

-- 数字类型
Amount DECIMAL(18,2) NOT NULL,         -- 金额，精确计算
Percentage DECIMAL(5,4) NULL,          -- 百分比 (0.0000-9.9999)
Count INT NOT NULL DEFAULT 0,          -- 计数
Id UNIQUEIDENTIFIER NOT NULL,          -- 唯一标识符

-- 日期时间类型
CreatedAt DATETIME2(7) NOT NULL,       -- 精确到 100 纳秒
BirthDate DATE NULL,                   -- 只需要日期
StartTime TIME(0) NULL,                -- 只需要时间

-- 布尔类型
IsActive BIT NOT NULL DEFAULT 1,       -- 布尔值
Status TINYINT NOT NULL,               -- 状态枚举 (0-255)

-- 二进制类型
Photo VARBINARY(MAX) NULL,             -- 文件内容
Thumbnail VARBINARY(8000) NULL,        -- 小文件
```

## 核心业务表设计 (Core Business Table Design)

### 1. 台球厅表 (BilliardHalls)

```sql
CREATE TABLE BilliardHalls (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    
    -- 基本信息
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    
    -- 地址信息
    Street NVARCHAR(200) NOT NULL,
    City NVARCHAR(50) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    PostalCode NVARCHAR(10) NULL,
    Country NVARCHAR(50) NOT NULL DEFAULT 'China',
    
    -- 营业信息
    Capacity INT NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Active, 2: Inactive, 3: Maintenance
    
    -- 营业时间 (JSON 存储)
    OperatingHours NVARCHAR(MAX) NULL
        CONSTRAINT CK_BilliardHalls_OperatingHours CHECK (ISJSON(OperatingHours) = 1),
    
    -- 审计字段
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedBy NVARCHAR(50) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    Version ROWVERSION NOT NULL,
    
    -- 索引
    INDEX IX_BilliardHalls_Status (Status) WHERE IsDeleted = 0,
    INDEX IX_BilliardHalls_City (City) WHERE IsDeleted = 0,
    INDEX IX_BilliardHalls_CreatedAt (CreatedAt),
    
    -- 约束
    CONSTRAINT CK_BilliardHalls_Status CHECK (Status IN (1, 2, 3)),
    CONSTRAINT CK_BilliardHalls_Capacity CHECK (Capacity >= 0)
);
```

### 2. 台球桌表 (BilliardTables)

```sql
CREATE TABLE BilliardTables (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    
    -- 关联信息
    HallId UNIQUEIDENTIFIER NOT NULL,
    
    -- 基本信息
    TableNumber INT NOT NULL,
    Type TINYINT NOT NULL, -- 1: Chinese8Ball, 2: American9Ball, 3: Snooker, 4: Carom
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Available, 2: Occupied, 3: Reserved, 4: Maintenance, 5: OutOfOrder
    
    -- 价格信息
    HourlyRateAmount DECIMAL(10,2) NOT NULL,
    HourlyRateCurrency NVARCHAR(3) NOT NULL DEFAULT 'CNY',
    
    -- 位置信息
    LocationX FLOAT NULL,
    LocationY FLOAT NULL,
    Floor TINYINT NOT NULL DEFAULT 1,
    Zone NVARCHAR(20) NULL,
    
    -- 特性 (JSON 数组)
    Features NVARCHAR(MAX) NULL
        CONSTRAINT CK_BilliardTables_Features CHECK (ISJSON(Features) = 1),
    
    -- 审计字段
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedBy NVARCHAR(50) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    Version ROWVERSION NOT NULL,
    
    -- 外键
    CONSTRAINT FK_BilliardTables_BilliardHalls 
        FOREIGN KEY (HallId) REFERENCES BilliardHalls(Id),
    
    -- 唯一约束
    CONSTRAINT UK_BilliardTables_HallId_TableNumber 
        UNIQUE (HallId, TableNumber) WHERE IsDeleted = 0,
    
    -- 检查约束
    CONSTRAINT CK_BilliardTables_Type CHECK (Type IN (1, 2, 3, 4)),
    CONSTRAINT CK_BilliardTables_Status CHECK (Status IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_BilliardTables_TableNumber CHECK (TableNumber > 0),
    CONSTRAINT CK_BilliardTables_HourlyRateAmount CHECK (HourlyRateAmount > 0),
    CONSTRAINT CK_BilliardTables_Floor CHECK (Floor > 0),
    
    -- 索引
    INDEX IX_BilliardTables_HallId (HallId) WHERE IsDeleted = 0,
    INDEX IX_BilliardTables_Status (Status) WHERE IsDeleted = 0,
    INDEX IX_BilliardTables_Type (Type) WHERE IsDeleted = 0,
    INDEX IX_BilliardTables_HallId_Status (HallId, Status) WHERE IsDeleted = 0,
    INDEX IX_BilliardTables_Location (LocationX, LocationY, Floor) WHERE IsDeleted = 0
);
```

### 3. 客户表 (Customers)

```sql
CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    
    -- 会员信息
    MembershipNumber NVARCHAR(20) NOT NULL,
    MembershipLevel TINYINT NOT NULL DEFAULT 1, -- 1: Bronze, 2: Silver, 3: Gold, 4: Platinum, 5: Diamond
    
    -- 基本信息
    Name NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NULL,
    Gender TINYINT NULL, -- 1: Male, 2: Female, 3: Other
    BirthDate DATE NULL,
    
    -- 地址信息
    Street NVARCHAR(200) NULL,
    City NVARCHAR(50) NULL,
    Province NVARCHAR(50) NULL,
    PostalCode NVARCHAR(10) NULL,
    
    -- 统计信息
    TotalSpentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalSpentCurrency NVARCHAR(3) NOT NULL DEFAULT 'CNY',
    VisitCount INT NOT NULL DEFAULT 0,
    LastVisitAt DATETIME2 NULL,
    
    -- 状态
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Active, 2: Inactive, 3: Suspended, 4: Blacklisted
    
    -- 备注
    Notes NVARCHAR(MAX) NULL,
    
    -- 审计字段
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedBy NVARCHAR(50) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    Version ROWVERSION NOT NULL,
    
    -- 唯一约束
    CONSTRAINT UK_Customers_MembershipNumber 
        UNIQUE (MembershipNumber) WHERE IsDeleted = 0,
    CONSTRAINT UK_Customers_Phone 
        UNIQUE (Phone) WHERE IsDeleted = 0,
    
    -- 检查约束
    CONSTRAINT CK_Customers_MembershipLevel CHECK (MembershipLevel IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_Customers_Gender CHECK (Gender IN (1, 2, 3)),
    CONSTRAINT CK_Customers_Status CHECK (Status IN (1, 2, 3, 4)),
    CONSTRAINT CK_Customers_TotalSpentAmount CHECK (TotalSpentAmount >= 0),
    CONSTRAINT CK_Customers_VisitCount CHECK (VisitCount >= 0),
    
    -- 索引
    INDEX IX_Customers_MembershipNumber (MembershipNumber) WHERE IsDeleted = 0,
    INDEX IX_Customers_Phone (Phone) WHERE IsDeleted = 0,
    INDEX IX_Customers_Email (Email) WHERE IsDeleted = 0 AND Email IS NOT NULL,
    INDEX IX_Customers_MembershipLevel (MembershipLevel) WHERE IsDeleted = 0,
    INDEX IX_Customers_Status (Status) WHERE IsDeleted = 0,
    INDEX IX_Customers_LastVisitAt (LastVisitAt) WHERE IsDeleted = 0
);
```

### 4. 预约表 (Reservations)

```sql
CREATE TABLE Reservations (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    
    -- 关联信息
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    TableId UNIQUEIDENTIFIER NOT NULL,
    
    -- 时间信息
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    DurationMinutes AS DATEDIFF(MINUTE, StartTime, EndTime) PERSISTED,
    
    -- 费用信息
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'CNY',
    
    -- 状态信息
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Pending, 2: Confirmed, 3: InProgress, 4: Completed, 5: Cancelled, 6: NoShow
    PaymentStatus TINYINT NOT NULL DEFAULT 1, -- 1: Unpaid, 2: PartialPaid, 3: Paid, 4: Refunded
    
    -- 附加信息
    Notes NVARCHAR(500) NULL,
    CancellationReason NVARCHAR(200) NULL,
    
    -- 审计字段
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedBy NVARCHAR(50) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    Version ROWVERSION NOT NULL,
    
    -- 外键
    CONSTRAINT FK_Reservations_Customers 
        FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Reservations_BilliardTables 
        FOREIGN KEY (TableId) REFERENCES BilliardTables(Id),
    
    -- 检查约束
    CONSTRAINT CK_Reservations_Status CHECK (Status IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT CK_Reservations_PaymentStatus CHECK (PaymentStatus IN (1, 2, 3, 4)),
    CONSTRAINT CK_Reservations_TimeRange CHECK (EndTime > StartTime),
    CONSTRAINT CK_Reservations_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Reservations_DurationMinutes CHECK (DurationMinutes >= 30), -- 最少30分钟
    
    -- 索引
    INDEX IX_Reservations_CustomerId (CustomerId) WHERE IsDeleted = 0,
    INDEX IX_Reservations_TableId (TableId) WHERE IsDeleted = 0,
    INDEX IX_Reservations_StartTime (StartTime) WHERE IsDeleted = 0,
    INDEX IX_Reservations_EndTime (EndTime) WHERE IsDeleted = 0,
    INDEX IX_Reservations_Status (Status) WHERE IsDeleted = 0,
    INDEX IX_Reservations_PaymentStatus (PaymentStatus) WHERE IsDeleted = 0,
    INDEX IX_Reservations_TableId_TimeRange (TableId, StartTime, EndTime) 
        WHERE IsDeleted = 0 AND Status IN (1, 2, 3), -- 用于冲突检查
    INDEX IX_Reservations_CreatedAt (CreatedAt) WHERE IsDeleted = 0
);
```

## 关系设计模式 (Relationship Design Patterns)

### 1. 一对多关系 (One-to-Many)

```sql
-- 台球厅 -> 台球桌
ALTER TABLE BilliardTables 
ADD CONSTRAINT FK_BilliardTables_BilliardHalls 
    FOREIGN KEY (HallId) REFERENCES BilliardHalls(Id)
    ON DELETE RESTRICT; -- 防止意外删除

-- 客户 -> 预约
ALTER TABLE Reservations 
ADD CONSTRAINT FK_Reservations_Customers 
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
    ON DELETE RESTRICT;
```

### 2. 多对多关系 (Many-to-Many)

```sql
-- 客户 <-> 台球桌 (通过预约历史)
-- 使用中间表模式
CREATE TABLE CustomerTableHistory (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    TableId UNIQUEIDENTIFIER NOT NULL,
    PlayCount INT NOT NULL DEFAULT 1,
    TotalDurationMinutes INT NOT NULL DEFAULT 0,
    LastPlayedAt DATETIME2 NOT NULL,
    
    CONSTRAINT FK_CustomerTableHistory_Customers 
        FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_CustomerTableHistory_BilliardTables 
        FOREIGN KEY (TableId) REFERENCES BilliardTables(Id),
        
    CONSTRAINT UK_CustomerTableHistory_Customer_Table 
        UNIQUE (CustomerId, TableId),
        
    INDEX IX_CustomerTableHistory_CustomerId (CustomerId),
    INDEX IX_CustomerTableHistory_TableId (TableId)
);
```

### 3. 自引用关系 (Self-Referencing)

```sql
-- 员工组织架构
CREATE TABLE Staff (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Position NVARCHAR(50) NOT NULL,
    ManagerId UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_Staff_Manager 
        FOREIGN KEY (ManagerId) REFERENCES Staff(Id),
        
    INDEX IX_Staff_ManagerId (ManagerId)
);
```

## 索引设计模式 (Index Design Patterns)

### 1. 查询优化索引

```sql
-- 复合索引 (最常用的查询组合)
CREATE INDEX IX_Reservations_TableId_Status_TimeRange 
ON Reservations (TableId, Status, StartTime, EndTime)
WHERE IsDeleted = 0;

-- 覆盖索引 (包含所有需要的列)
CREATE INDEX IX_BilliardTables_HallId_Covering 
ON BilliardTables (HallId)
INCLUDE (TableNumber, Type, Status, HourlyRateAmount)
WHERE IsDeleted = 0;

-- 部分索引 (只索引有效记录)
CREATE INDEX IX_Customers_Active_Phone 
ON Customers (Phone)
WHERE IsDeleted = 0 AND Status = 1;
```

### 2. 唯一性约束索引

```sql
-- 业务唯一性
CREATE UNIQUE INDEX UK_BilliardTables_HallId_Number_Active 
ON BilliardTables (HallId, TableNumber)
WHERE IsDeleted = 0;

-- 条件唯一性 (同一时间段同一台球桌只能有一个有效预约)
CREATE UNIQUE INDEX UK_Reservations_TableId_TimeOverlap_Active 
ON Reservations (TableId, StartTime, EndTime)
WHERE IsDeleted = 0 AND Status IN (1, 2, 3); -- Pending, Confirmed, InProgress
```

## 触发器模式 (Trigger Patterns)

### 1. 审计触发器

```sql
-- 更新时间自动维护
CREATE TRIGGER TR_BilliardTables_UpdatedAt
ON BilliardTables
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE BilliardTables
    SET UpdatedAt = GETUTCDATE(),
        UpdatedBy = SYSTEM_USER
    FROM BilliardTables bt
    INNER JOIN inserted i ON bt.Id = i.Id;
END;
```

### 2. 业务规则触发器

```sql
-- 预约冲突检查
CREATE TRIGGER TR_Reservations_ConflictCheck
ON Reservations
FOR INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 检查时间冲突
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Reservations r ON r.TableId = i.TableId
        WHERE r.Id != i.Id
          AND r.IsDeleted = 0
          AND r.Status IN (1, 2, 3) -- Pending, Confirmed, InProgress
          AND (
              (i.StartTime >= r.StartTime AND i.StartTime < r.EndTime) OR
              (i.EndTime > r.StartTime AND i.EndTime <= r.EndTime) OR
              (i.StartTime <= r.StartTime AND i.EndTime >= r.EndTime)
          )
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, '预约时间冲突，该时间段台球桌已被预约', 1;
    END;
END;
```

## 视图模式 (View Patterns)

### 1. 业务视图

```sql
-- 台球桌可用性视图
CREATE VIEW vw_TableAvailability
AS
SELECT 
    bt.Id,
    bt.HallId,
    bh.Name AS HallName,
    bt.TableNumber,
    bt.Type,
    bt.Status,
    bt.HourlyRateAmount,
    bt.LocationX,
    bt.LocationY,
    bt.Floor,
    bt.Zone,
    -- 当前是否有活跃预约
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM Reservations r 
            WHERE r.TableId = bt.Id 
              AND r.IsDeleted = 0 
              AND r.Status IN (2, 3) -- Confirmed, InProgress
              AND GETUTCDATE() BETWEEN r.StartTime AND r.EndTime
        ) THEN 0 
        ELSE 1 
    END AS IsCurrentlyAvailable,
    -- 下次可用时间
    (
        SELECT MIN(r.EndTime)
        FROM Reservations r
        WHERE r.TableId = bt.Id
          AND r.IsDeleted = 0
          AND r.Status IN (1, 2, 3) -- Pending, Confirmed, InProgress
          AND r.EndTime > GETUTCDATE()
    ) AS NextAvailableTime
FROM BilliardTables bt
INNER JOIN BilliardHalls bh ON bt.HallId = bh.Id
WHERE bt.IsDeleted = 0 
  AND bh.IsDeleted = 0
  AND bt.Status IN (1, 3); -- Available, Reserved
```

### 2. 统计视图

```sql
-- 客户统计视图
CREATE VIEW vw_CustomerStatistics
AS
SELECT 
    c.Id,
    c.MembershipNumber,
    c.Name,
    c.MembershipLevel,
    c.TotalSpentAmount,
    c.VisitCount,
    c.LastVisitAt,
    -- 最近30天预约次数
    COUNT(r30.Id) AS ReservationsLast30Days,
    -- 最近30天消费金额
    ISNULL(SUM(r30.TotalAmount), 0) AS SpentLast30Days,
    -- 最喜欢的台球桌类型
    (
        SELECT TOP 1 bt.Type
        FROM Reservations r
        INNER JOIN BilliardTables bt ON r.TableId = bt.Id
        WHERE r.CustomerId = c.Id 
          AND r.IsDeleted = 0 
          AND r.Status = 4 -- Completed
        GROUP BY bt.Type
        ORDER BY COUNT(*) DESC
    ) AS PreferredTableType
FROM Customers c
LEFT JOIN Reservations r30 ON c.Id = r30.CustomerId 
    AND r30.IsDeleted = 0 
    AND r30.CreatedAt >= DATEADD(DAY, -30, GETUTCDATE())
WHERE c.IsDeleted = 0
GROUP BY c.Id, c.MembershipNumber, c.Name, c.MembershipLevel, 
         c.TotalSpentAmount, c.VisitCount, c.LastVisitAt;
```

## 存储过程模式 (Stored Procedure Patterns)

### 1. 业务流程存储过程

```sql
-- 创建预约的存储过程
CREATE PROCEDURE sp_CreateReservation
    @CustomerId UNIQUEIDENTIFIER,
    @TableId UNIQUEIDENTIFIER,
    @StartTime DATETIME2,
    @EndTime DATETIME2,
    @Notes NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ReservationId UNIQUEIDENTIFIER = NEWID();
    DECLARE @HourlyRate DECIMAL(10,2);
    DECLARE @DurationHours DECIMAL(10,2);
    DECLARE @TotalAmount DECIMAL(18,2);
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- 检查台球桌是否存在且可用
        IF NOT EXISTS (
            SELECT 1 FROM BilliardTables 
            WHERE Id = @TableId 
              AND IsDeleted = 0 
              AND Status IN (1, 3) -- Available, Reserved
        )
        BEGIN
            THROW 50002, '台球桌不存在或不可用', 1;
        END;
        
        -- 获取小时费率
        SELECT @HourlyRate = HourlyRateAmount 
        FROM BilliardTables 
        WHERE Id = @TableId;
        
        -- 计算费用
        SET @DurationHours = CAST(DATEDIFF(MINUTE, @StartTime, @EndTime) AS DECIMAL(10,2)) / 60.0;
        SET @TotalAmount = @HourlyRate * @DurationHours;
        
        -- 创建预约
        INSERT INTO Reservations (
            Id, CustomerId, TableId, StartTime, EndTime, 
            TotalAmount, Currency, Status, PaymentStatus, 
            Notes, CreatedBy
        )
        VALUES (
            @ReservationId, @CustomerId, @TableId, @StartTime, @EndTime,
            @TotalAmount, 'CNY', 1, 1, -- Pending, Unpaid
            @Notes, @CreatedBy
        );
        
        -- 更新台球桌状态为预约
        UPDATE BilliardTables 
        SET Status = 3, -- Reserved
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @CreatedBy
        WHERE Id = @TableId;
        
        COMMIT TRANSACTION;
        
        -- 返回创建的预约信息
        SELECT @ReservationId AS ReservationId, @TotalAmount AS TotalAmount;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

## 数据库函数模式 (Database Function Patterns)

### 1. 标量函数

```sql
-- 计算两个时间段的重叠分钟数
CREATE FUNCTION fn_GetTimeOverlapMinutes(
    @Start1 DATETIME2,
    @End1 DATETIME2,
    @Start2 DATETIME2,
    @End2 DATETIME2
)
RETURNS INT
AS
BEGIN
    DECLARE @OverlapStart DATETIME2 = CASE WHEN @Start1 > @Start2 THEN @Start1 ELSE @Start2 END;
    DECLARE @OverlapEnd DATETIME2 = CASE WHEN @End1 < @End2 THEN @End1 ELSE @End2 END;
    
    RETURN CASE 
        WHEN @OverlapEnd > @OverlapStart 
        THEN DATEDIFF(MINUTE, @OverlapStart, @OverlapEnd)
        ELSE 0
    END;
END;
```

### 2. 表值函数

```sql
-- 获取指定日期范围内的可用时间段
CREATE FUNCTION fn_GetAvailableTimeSlots(
    @TableId UNIQUEIDENTIFIER,
    @Date DATE,
    @OpenTime TIME,
    @CloseTime TIME,
    @SlotDurationMinutes INT = 60
)
RETURNS TABLE
AS
RETURN
(
    WITH TimeSlots AS (
        SELECT 
            CAST(@Date AS DATETIME2) + CAST(@OpenTime AS DATETIME2) AS SlotStart,
            DATEADD(MINUTE, @SlotDurationMinutes, 
                CAST(@Date AS DATETIME2) + CAST(@OpenTime AS DATETIME2)) AS SlotEnd
        
        UNION ALL
        
        SELECT 
            DATEADD(MINUTE, @SlotDurationMinutes, SlotStart),
            DATEADD(MINUTE, @SlotDurationMinutes * 2, SlotStart)
        FROM TimeSlots
        WHERE DATEADD(MINUTE, @SlotDurationMinutes, SlotStart) < 
              CAST(@Date AS DATETIME2) + CAST(@CloseTime AS DATETIME2)
    )
    SELECT 
        SlotStart,
        SlotEnd,
        CASE WHEN EXISTS (
            SELECT 1 FROM Reservations r
            WHERE r.TableId = @TableId
              AND r.IsDeleted = 0
              AND r.Status IN (1, 2, 3) -- Pending, Confirmed, InProgress
              AND (
                  (SlotStart >= r.StartTime AND SlotStart < r.EndTime) OR
                  (SlotEnd > r.StartTime AND SlotEnd <= r.EndTime) OR
                  (SlotStart <= r.StartTime AND SlotEnd >= r.EndTime)
              )
        ) THEN 0 ELSE 1 END AS IsAvailable
    FROM TimeSlots
);
```

---

> 以上数据库设计模式应在所有数据库相关开发中严格遵循，确保数据一致性、性能和可维护性。