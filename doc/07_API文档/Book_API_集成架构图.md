# Book API 集成架构图

## 整体架构

```mermaid
graph TB
    subgraph "UniApp 前端"
        A[用户界面<br/>book-list.vue] --> B[API 模块<br/>book.js]
        B --> C[请求封装<br/>request.js]
    end
    
    subgraph "网络层"
        C --> D[HTTP Request<br/>Bearer Token]
        D --> E[HTTP Response<br/>JSON]
        E --> C
    end
    
    subgraph "ABP 后端"
        D --> F[API 端点<br/>/api/app/book]
        F --> G[BookAppService]
        G --> H[IRepository]
        H --> I[(PostgreSQL<br/>数据库)]
        I --> H
        H --> G
        G --> F
        F --> E
    end
    
    subgraph "认证授权"
        J[OpenIddict<br/>认证服务器] -.Token验证.-> F
    end
```

## 数据流详解

### 1. 获取图书列表流程

```mermaid
sequenceDiagram
    participant User as 用户
    participant Page as book-list.vue
    participant API as book.js
    participant Request as request.js
    participant Backend as /api/app/book
    participant Service as BookAppService
    participant DB as 数据库

    User->>Page: 访问图书列表页面
    Page->>Page: onMounted() 触发
    Page->>API: getBookList({skipCount:0, maxResultCount:10})
    API->>Request: get('/api/app/book', params)
    Request->>Request: 添加 Authorization 头
    Request->>Backend: GET /api/app/book?skipCount=0&maxResultCount=10
    Backend->>Service: GetListAsync(PagedAndSortedResultRequestDto)
    Service->>DB: 查询图书数据
    DB-->>Service: 返回数据
    Service-->>Backend: PagedResultDto<BookDto>
    Backend-->>Request: HTTP 200 + JSON
    Request-->>API: 解析响应
    API-->>Page: {items:[], totalCount:n}
    Page->>Page: 更新 bookList.value
    Page-->>User: 显示图书列表
```

### 2. 认证流程

```mermaid
sequenceDiagram
    participant User as 用户
    participant App as UniApp
    participant Auth as 认证模块
    participant OIDC as OpenIddict
    participant Storage as uni.storage

    User->>App: 打开应用
    App->>Storage: 读取 token
    alt Token 不存在
        App->>User: 跳转登录页
        User->>Auth: 输入账号密码
        Auth->>OIDC: 登录请求
        OIDC-->>Auth: Access Token
        Auth->>Storage: 保存 token
        Auth->>App: 登录成功
    else Token 存在
        App->>App: 使用现有 token
    end
    
    App->>Backend: API 请求 + Bearer Token
    Backend->>OIDC: 验证 Token
    OIDC-->>Backend: 验证结果
    
    alt Token 有效
        Backend-->>App: 返回数据
    else Token 无效
        Backend-->>App: 401 Unauthorized
        App->>Storage: 清除 token
        App->>User: 跳转登录页
    end
```

### 3. 错误处理流程

```mermaid
flowchart TD
    A[发起 API 请求] --> B{请求是否成功?}
    B -->|成功| C[返回数据]
    B -->|失败| D{检查错误类型}
    
    D -->|401 未授权| E[清除 Token]
    E --> F[跳转登录页]
    
    D -->|403 无权限| G[显示权限错误提示]
    
    D -->|404 未找到| H[显示资源不存在]
    
    D -->|500 服务器错误| I[显示服务器错误]
    
    D -->|网络错误| J[显示网络错误]
    
    G --> K[返回错误]
    H --> K
    I --> K
    J --> K
    
    C --> L[业务逻辑处理]
    K --> M[用户界面反馈]
```

## 组件关系图

```mermaid
graph LR
    subgraph "页面层"
        A1[book-list.vue<br/>图书列表页]
        A2[mine.vue<br/>我的页面]
    end
    
    subgraph "API 层"
        B1[book.js<br/>图书 API]
        B2[auth.js<br/>认证 API]
        B3[user.js<br/>用户 API]
    end
    
    subgraph "工具层"
        C1[request.js<br/>HTTP 封装]
        C2[utils/*<br/>工具函数]
    end
    
    A1 --> B1
    A2 --> B1
    A2 --> B2
    A2 --> B3
    
    B1 --> C1
    B2 --> C1
    B3 --> C1
    
    C1 --> C2
```

## ABP 约定式路由映射

```mermaid
graph LR
    subgraph "Application Service"
        S[BookAppService]
        S1["GetListAsync()"]
        S2["GetAsync(id)"]
        S3["CreateAsync()"]
        S4["UpdateAsync(id)"]
        S5["DeleteAsync(id)"]
        
        S --> S1
        S --> S2
        S --> S3
        S --> S4
        S --> S5
    end
    
    subgraph "REST API Endpoints"
        E1["GET /api/app/book"]
        E2["GET /api/app/book/{id}"]
        E3["POST /api/app/book"]
        E4["PUT /api/app/book/{id}"]
        E5["DELETE /api/app/book/{id}"]
    end
    
    S1 -.自动映射.-> E1
    S2 -.自动映射.-> E2
    S3 -.自动映射.-> E3
    S4 -.自动映射.-> E4
    S5 -.自动映射.-> E5
```

## 数据模型

```mermaid
classDiagram
    class BookDto {
        +Guid id
        +string name
        +BookType type
        +DateTime publishDate
        +float price
        +DateTime creationTime
        +Guid? creatorId
        +DateTime? lastModificationTime
        +Guid? lastModifierId
    }
    
    class PagedResultDto {
        +List~BookDto~ items
        +long totalCount
    }
    
    class BookType {
        <<enumeration>>
        Undefined = 0
        Adventure = 1
        Biography = 2
        Dystopia = 3
        Fantastic = 4
        Horror = 5
        Science = 6
        ScienceFiction = 7
        Poetry = 8
    }
    
    class CreateUpdateBookDto {
        +string name
        +BookType type
        +DateTime publishDate
        +float price
    }
    
    PagedResultDto --> BookDto
    BookDto --> BookType
    CreateUpdateBookDto --> BookType
```

## 权限控制

```mermaid
graph TD
    A[API 请求] --> B{是否携带 Token?}
    B -->|否| C[401 未授权]
    B -->|是| D{Token 是否有效?}
    D -->|否| C
    D -->|是| E{检查操作类型}
    
    E -->|GET 列表/详情| F{是否有 Default 权限?}
    E -->|POST 创建| G{是否有 Create 权限?}
    E -->|PUT 更新| H{是否有 Edit 权限?}
    E -->|DELETE 删除| I{是否有 Delete 权限?}
    
    F -->|是| J[允许访问]
    F -->|否| K[403 无权限]
    
    G -->|是| J
    G -->|否| K
    
    H -->|是| J
    H -->|否| K
    
    I -->|是| J
    I -->|否| K
```

## 部署架构

```mermaid
graph TB
    subgraph "客户端"
        U1[用户浏览器/微信]
        U2[UniApp H5]
        U3[微信小程序]
    end
    
    subgraph "前端服务"
        F[Nginx<br/>静态资源服务器]
    end
    
    subgraph "后端服务"
        A[API 网关]
        B[HttpApi.Host<br/>ASP.NET Core]
        C[OpenIddict<br/>认证服务器]
    end
    
    subgraph "数据层"
        D[(PostgreSQL<br/>数据库)]
        E[(Redis<br/>缓存)]
    end
    
    U1 --> F
    U2 --> F
    U3 --> A
    F --> A
    A --> B
    A --> C
    B --> D
    B --> E
    C --> D
```

## 性能优化策略

```mermaid
mindmap
  root((性能优化))
    前端优化
      分页加载
      虚拟列表
      图片懒加载
      请求防抖
      本地缓存
    网络优化
      HTTP/2
      GZIP 压缩
      CDN 加速
      请求合并
    后端优化
      数据库索引
      查询优化
      Redis 缓存
      异步处理
      连接池
    架构优化
      负载均衡
      读写分离
      微服务拆分
      消息队列
```

## 监控与日志

```mermaid
graph LR
    subgraph "监控指标"
        M1[API 响应时间]
        M2[错误率]
        M3[并发量]
        M4[数据库性能]
    end
    
    subgraph "日志收集"
        L1[Serilog]
        L2[结构化日志]
        L3[OpenTelemetry]
    end
    
    subgraph "可视化"
        V1[Grafana]
        V2[日志查询]
        V3[告警系统]
    end
    
    M1 --> L1
    M2 --> L1
    M3 --> L1
    M4 --> L1
    
    L1 --> L2
    L2 --> L3
    
    L3 --> V1
    L3 --> V2
    V1 --> V3
    V2 --> V3
```

## 相关文档

- [接口清单](./接口清单.md) - 详细的 API 端点说明
- [README](./README.md) - API 文档总览
- [认证与授权](./认证与授权.md) - 认证机制说明
- [实现总结](../../IMPLEMENTATION_SUMMARY.md) - 实现细节和使用指南
