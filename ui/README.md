# BilliardHall 前端 (Nuxt + ABP Vue 风格)

## 功能定位

- 作为 ABP 后端 (OpenIddict) 的 SPA / 管理界面入口
- 使用 OIDC 授权码 + 刷新令牌模式

## 快速开始

```bash
# 复制环境变量模板
cp .env.example .env
# 根据需要修改 ClientSecret / API Endpoint
# 安装依赖 (建议已全局安装 pnpm)
pnpm install
# 启动开发
pnpm dev
```

浏览器访问 <https://localhost:3000> （若出现自签名证书警告，可临时信任）。

## 主要环境变量 (.env)

详见根项目 `doc/08_配置管理/Secrets管理.md` 对照表。

| 变量 | 必填 | 说明 |
|------|------|------|
| NUXT_AUTHORITY_URL | 是 | OIDC 授权服务器地址 |
| NUXT_CLIENT_ID | 是 | 必须与后端 OpenIddict 种子一致 |
| NUXT_CLIENT_SECRET | 视客户端类型 | 机密型客户端需要，公共客户端留空 |
| NUXT_SCOPE | 是 | 建议: openid profile email BilliardHall |
| NUXT_ABP_API_ENDPOINT | 是 | 后端 API 根地址 |
| NUXT_SESSION_SECRET | 是 | 随机 Base64，用于会话/加密用途 |

## 与后端的对应

- OpenIddict 客户端配置在 `OpenIddictDataSeedContributor` 中通过 `OpenIddict:Applications:Abp_Vue:*` 读取
- 若需修改重定向 URI，对应更新：
  - `.env` 中 `NUXT_REDIRECT_URI` / `NUXT_POST_LOGOUT_REDIRECT_URI`
  - 后端配置节 `OpenIddict:Applications:Abp_Vue:RootUrl`

## 生成 API Type (后续可扩展)

> 当前未集成自动代码生成脚本；若接入 swagger 生成，可添加：

```jsonc
// package.json scripts 片段示例
"generate:api": "openapi-typescript https://localhost:44388/swagger/v1/swagger.json -o generated/api.d.ts"
```

## 代码结构（初始最小化）

- `nuxt.config.ts` 运行时公开配置注入
- `.env.example` 变量模板

后续可扩展：

- 授权拦截插件 / Axios 请求封装
- 动态菜单与权限指令
- UI 组件库接入（Element Plus / Naive UI 等）

## 开发建议

- 使用 `pnpm` 保持锁文件一致性
- 严格 TS，开启 ESLint（已预留脚本）
- 区分公共与机密客户端：公共客户端不要配置 ClientSecret

## TODO (后续可在 Issue 中跟踪)

- [ ] 接入 OpenId Connect 登录逻辑封装（Composable / Plugin）
- [ ] 添加统一错误处理与重试机制
- [ ] API 客户端生成与缓存策略
- [ ] 前端日志/性能指标上报 OTLP
- [ ] 生产打包 Dockerfile / Nginx 配置
