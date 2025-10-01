# 静态资源 (static)

本目录存放 UniApp 前端的通用可复用静态资源：SVG 图标 / 位图 / TabBar 图标 / 公共样式等。

> 新增：已引入一组基础 SVG（logo / table / user / payment / scan / session）以及 `icons-sprite.svg` 精灵文件，推荐后续优先使用 SVG 方案以减少位图冗余。

## 目录结构（建议规划）

```text
static/
├── logo.svg              # 品牌主标识
├── table.svg             # 台球桌图标
├── user.svg              # 用户图标
├── payment.svg           # 支付图标
├── scan.svg              # 扫码图标
├── session.svg           # 计费/会话图标
├── placeholder.svg       # 通用占位
├── icons-sprite.svg      # Symbol 精灵 (聚合)
├── tabbar/               # 底部导航栏位图（如仍需）
├── icons/                # 传统 PNG/JPG 图标（逐步收敛）
├── images/               # 业务图片/宣传位
└── styles/               # 样式相关
        └── common.css
```

## 资源清单说明

| 文件 | 用途 | 说明 |
|------|------|------|
| logo.svg | 品牌 | 可用于启动/导航/登录头部 |
| table.svg | 台球桌 | 设备/房态/列表场景 |
| user.svg | 用户/会员 | 个人中心 / 头像占位 |
| payment.svg | 支付/账单 | 订单、支付入口 |
| scan.svg | 扫码 | 扫码入口按钮 |
| session.svg | 计费会话 | 开台 / 进行中标签 |
| placeholder.svg | 占位 | 异步加载前的兜底展示 |
| icons-sprite.svg | Symbol 精灵 | 批量注入，减少重复 inline |

## 使用方式

### 1. 直接引用单个 SVG

```html
<img src="/src/static/table.svg" alt="table" />
```
在 uni-app (vue3) 模板：
```vue
<template>
    <image src="/src/static/scan.svg" mode="widthFix" />
</template>
```

> 发布多端时若路径适配问题，可尝试 `@/static/scan.svg` 或借助构建插件 raw 导入方式。

### 2. 使用 Symbol 精灵（`icons-sprite.svg`）

建议在 `App.vue` 顶层一次性注入：

```vue
<template>
    <div>
        <SvgSprite />
        <router-view />
    </div>
</template>

<script setup>
import sprite from '@/static/icons-sprite.svg?raw'
</script>

<template #components>
<!-- SvgSprite 组件示例 -->
<template>
    <div v-html="sprite" style="display:none" />
</template>
</template>
```
调用：
```html
<svg class="icon" width="24" height="24" aria-hidden="true">
    <use href="#icon-user" />
</svg>
```

可用 ID：`icon-user`, `icon-scan`, `icon-table`, `icon-payment`, `icon-session`

### 3. 动态换色 / 主题适配

所有精灵内使用 `currentColor` 的元素可通过外层 `color` 控制：

```html
<svg class="icon" style="color:#4aa3ff"><use href="#icon-payment"/></svg>
```

与暗色模式结合：
```css
.icon { transition: color .25s; }
@media (prefers-color-scheme: dark) {
    .icon { color: #4aa3ff; }
}
```

### 4. 占位图使用

```vue
<image v-if="loaded" :src="realUrl" mode="aspectFill" />
<image v-else src="/src/static/placeholder.svg" mode="aspectFit" />
```

## 命名规范

- 小写 + 中划线：`feature-action.svg`
- 语义清晰，避免 `icon1.svg` ❌
- 同语义多风格：`table-outline.svg` / `table-filled.svg`

## 新增 SVG 流程

1. 设计或获取资源 → 先用 [SVGOMG](https://jakearchibald.github.io/svgomg/) 压缩
2. 移除多余 `<metadata>` / `id` / `fill`（除非必要）
3. 若加入精灵：包裹为 `<symbol id="icon-xxx">...</symbol>` 并放入 `icons-sprite.svg`
4. 更新本 README 的“资源清单说明”表格
5. 提交信息：`feat(static): add xxx svg asset`


## 性能与优化建议

- 优先使用 SVG 代替 PNG（特别是线性/矢量图标）
- 大图或背景图使用现代格式 (webp/avif) 并保留 fallback（必要时）
- 避免超过 200KB 的单文件资源
- 若未来增长较多，可引入自动脚本生成 sprite（如 `svg-sprite-loader` 或 Rollup/Vite 插件）


## 可访问性 (a11y)

- 语义图标添加 `<title>` 或 `aria-label` 描述
- 纯装饰性：加 `aria-hidden="true"` 或通过 CSS 背景使用
- `<img>` 标签添加 `alt`；空装饰用 `alt=""`


## 兼容注意

| 场景 | 说明 |
|------|------|
| 微信小程序 | 直接 `<image>` 引用 svg 不支持，需：1) 转成 base64 2) 用组件内内联 `<svg>` 或使用 png 备份 |
| H5 | 完全支持 inline / symbol / external |
| 原生 App (uni-app) | inline svg 需要视运行环境内核，建议保留关键位图备份 |


## （附录）原 TabBar 图标说明（保留）

底部导航栏图标建议尺寸 81x81px：
- `tabbar/home.png` / `home-active.png`
- `tabbar/scan.png` / `scan-active.png`
- `tabbar/user.png` / `user-active.png`


若迁移为 SVG，可统一用 sprite 并在逻辑中切换 class。

## 图片优化建议

1. 使用压缩工具（tinypng, squoosh）
2. 优先 webp；不兼容端回退 png
3. 小图标：使用 svg（单色用 `currentColor`）
4. 统一通过脚本 CI 检查（可后续添加）

## 注意事项

- 避免中文文件名
- 不提交含版权风险的第三方素材
- 不在 SVG 中内嵌 base64 位图（除非说明原因）

---
如需补充更多图标，请在 PR 中注明用途，避免冗余。
