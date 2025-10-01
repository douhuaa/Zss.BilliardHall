# Zss台球厅管理系统 - UniApp移动端

## 项目简介

这是台球厅管理系统的移动端应用，基于 UniApp 框架开发，支持编译为微信小程序、H5 和 App。

### 主要功能

- 🔐 用户登录认证（短信验证码、微信授权）
- 📷 扫码开台
- ⏱️ 实时计费显示
- 💳 在线支付（微信支付、支付宝）
- 📊 消费记录查询
- 👤 个人中心管理

## 技术栈

- **框架**: UniApp + Vue 3
- **构建工具**: Vite
- **开发语言**: JavaScript
- **UI**: 自定义组件

## 项目结构

```
frontend-uniapp/
├── src/
│   ├── api/              # API接口
│   │   ├── auth.js       # 认证相关
│   │   ├── session.js    # 计费会话
│   │   ├── payment.js    # 支付相关
│   │   └── user.js       # 用户相关
│   ├── components/       # 公共组件
│   ├── pages/            # 页面
│   │   ├── index/        # 首页
│   │   ├── login/        # 登录页
│   │   ├── scan/         # 扫码页
│   │   ├── session/      # 计费详情
│   │   ├── payment/      # 支付页
│   │   └── mine/         # 个人中心
│   ├── static/           # 静态资源
│   ├── utils/            # 工具函数
│   │   └── request.js    # HTTP请求封装
│   ├── App.vue           # 应用入口
│   ├── main.js           # 主入口文件
│   ├── manifest.json     # 应用配置
│   └── pages.json        # 页面配置
├── .env.development      # 开发环境变量
├── .env.production       # 生产环境变量
├── index.html            # H5入口
├── package.json          # 项目配置
├── vite.config.js        # Vite配置
└── README.md             # 项目说明
```

## 快速开始

### 环境要求

- Node.js >= 16.x
- npm >= 8.x

### 安装依赖

```bash
npm install
```

### 开发运行

#### 微信小程序

```bash
npm run dev:mp-weixin
```

运行后将 `dist/dev/mp-weixin` 目录导入微信开发者工具。

#### H5

```bash
npm run dev:h5
```

访问 http://localhost:3000

### 生产构建

#### 微信小程序

```bash
npm run build:mp-weixin
```

构建产物在 `dist/build/mp-weixin` 目录。

#### H5

```bash
npm run build:h5
```

构建产物在 `dist/build/h5` 目录。

## 配置说明

### 环境变量

- `.env.development`: 开发环境配置
- `.env.production`: 生产环境配置

主要配置项：
- `VUE_APP_API_URL`: 后端API地址

### 微信小程序配置

需要在 `src/manifest.json` 中配置：
- `mp-weixin.appid`: 微信小程序 AppID

## API 集成

所有 API 请求通过 `src/utils/request.js` 统一管理，自动处理：
- 认证 Token
- 请求/响应拦截
- 错误处理

API 模块位于 `src/api/` 目录，包括：
- `auth.js`: 登录、注册、验证码
- `session.js`: 开台、计费、结束会话
- `payment.js`: 支付、订单查询
- `user.js`: 用户信息、余额、积分

## 开发规范

### 命名规范

- 文件名: 小写，单词用连字符分隔 (kebab-case)
- 组件名: PascalCase
- 变量名: camelCase
- 常量名: UPPER_SNAKE_CASE

### 代码规范

- 使用 2 空格缩进
- 使用单引号
- 每行代码不超过 100 字符
- 函数应添加注释说明

### Git 提交规范

遵循 Conventional Commits 规范：

- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 重构
- `test`: 测试相关
- `chore`: 构建/工具相关

示例：
```
feat(login): 添加微信登录功能
fix(payment): 修复支付回调异常
```

## 调试技巧

### 微信小程序调试

1. 在微信开发者工具中打开项目
2. 使用开发者工具的调试功能
3. 查看 Console 和 Network 面板

### H5 调试

1. 使用浏览器开发者工具
2. 移动设备调试可使用 Chrome Remote Debugging

### 真机调试

微信小程序可使用微信开发者工具的预览功能，扫码在真机上测试。

## 常见问题

### 1. 页面白屏

- 检查页面路径是否在 `pages.json` 中正确配置
- 检查组件语法是否正确

### 2. API 请求失败

- 检查 API 地址配置
- 检查网络连接
- 查看 Console 中的错误信息

### 3. 微信小程序授权问题

- 确保在 `manifest.json` 中配置了正确的权限
- 检查 AppID 是否正确

## 待实现功能

- [ ] 球台预约
- [ ] 会员套餐购买
- [ ] 消息推送
- [ ] 分享功能
- [ ] 优惠券系统

## 相关文档

- [UniApp 官方文档](https://uniapp.dcloud.net.cn/)
- [Vue 3 文档](https://cn.vuejs.org/)
- [微信小程序文档](https://developers.weixin.qq.com/miniprogram/dev/framework/)
- [后端 API 文档](../doc/07_API文档/README.md)

## 许可证

MIT License

## 联系方式

- 项目仓库: [https://github.com/douhuaa/Zss.BilliardHall](https://github.com/douhuaa/Zss.BilliardHall)
- 技术支持: 开发团队
