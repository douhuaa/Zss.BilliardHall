# 组件目录

## SvgIcon

通用 SVG 图标组件，支持两种模式：
1. 基于 `icons-sprite.svg` 的 `<symbol>` 引用 (推荐)
2. 回退加载 `src/static/<name>.svg` 单文件（需要构建器支持 `?component`）

### Props

| 名称 | 类型 | 默认 | 说明 |
|------|------|------|------|
| name | String | (必填) | 图标名称（对应 sprite 内去掉 `icon-` 前缀，如 `user` 对应 `#icon-user`） |
| size | Number/String | 24 | 图标尺寸，数值自动补 px |
| color | String | 继承 | 自定义颜色，不填使用 currentColor |
| spin | Boolean | false | 是否旋转动画（加载中、等待状态可用） |
| ariaLabel | String | '' | 无障碍文本；不填时用 name |
| fallback | Boolean | true | 是否回退加载单文件 svg（若 sprite 内无对应符号） |
| viewBox | String | '' | 手动指定 viewBox（通常不需要） |

### 基本用法

```vue
<script setup>
import SvgIcon from '@/components/SvgIcon.vue'
</script>
<template>
  <SvgIcon name="user" size="32" />
  <SvgIcon name="payment" color="#4aa3ff" />
  <SvgIcon name="scan" :spin="true" />
</template>
```

### 回退模式

如果 `icons-sprite.svg` 中缺少对应 `<symbol>`，组件会尝试动态加载：
 
```text
/src/static/<name>.svg?component
```

需确保 Vite 已开启对 `?component`（通常由 `@dcloudio/vite-plugin-uni` + 内置 svg loader 支持）。

### 动态颜色


```vue
<SvgIcon name="session" style="color:var(--primary-color)" />
```

内部 `currentColor` 会自动继承。

### 旋转图标

```vue
<SvgIcon name="scan" :spin="true" />
```

### 无障碍

```vue
<SvgIcon name="payment" aria-label="支付" />
```

### 统一注册（可选）

可在入口 `main.js` 中全局注册：
 
```js
import SvgIcon from '@/components/SvgIcon.vue'
app.component('SvgIcon', SvgIcon)
```

### 常见问题

1. 图标不显示：检查是否注入了 `icons-sprite.svg`（参考 static README 精灵注入）
2. 颜色不生效：确认 symbol 内元素未写死 fill；确保使用 `currentColor`
3. 小程序端兼容：不支持 `<use>` 的端需要改用内联或位图 fallback

### 后续可扩展

- 支持自动收集 `/src/static/icons/*.svg` 生成 sprite
- 支持 `type` + 语义映射（例如 `type="success"` 映射到 check 图标）
- 支持 `aria-hidden` 自动逻辑（若仅装饰）
