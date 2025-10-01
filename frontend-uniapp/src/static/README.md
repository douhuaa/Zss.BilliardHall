# 静态资源目录

本目录用于存放静态资源文件。

## 目录结构

```
static/
├── icons/       # 图标资源
├── images/      # 图片资源
├── tabbar/      # 底部导航栏图标
└── styles/      # 全局样式
    └── common.css
```

## 使用说明

### 图标

底部导航栏图标需要准备以下文件（建议尺寸 81x81px）：
- `tabbar/home.png` - 首页图标
- `tabbar/home-active.png` - 首页选中图标
- `tabbar/scan.png` - 扫码图标
- `tabbar/scan-active.png` - 扫码选中图标
- `tabbar/user.png` - 我的图标
- `tabbar/user-active.png` - 我的选中图标

### 图片

应用图标和启动图：
- `logo.png` - 应用Logo（建议 512x512px）

其他图标：
- `icons/scan.png` - 扫码图标
- `icons/user.png` - 用户图标
- `icons/wechat.png` - 微信图标

### 图片优化建议

1. 使用压缩工具减小图片体积
2. 尽量使用 WebP 格式
3. 小图标优先使用 SVG
4. 图片命名使用小写字母和连字符

## 注意事项

- 图片资源不要过大，建议单个文件不超过 200KB
- 避免使用中文文件名
- 确保图片格式兼容性（PNG、JPG、GIF、WebP）
