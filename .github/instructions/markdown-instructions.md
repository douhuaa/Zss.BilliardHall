# Markdown 使用与 lint 说明（仓库级）

本文件提供简洁、可执行的仓库级 Markdown 编写与 lint 推荐，旨在修复常见的 markdownlint 报错（例如 MD029、MD022、MD032、MD034）。

## 目标

- 保持 README 与文档样式一致，降低 CI 报错概率。
- 提供可复制的示例与本地/CI 运行命令，便于在 PR 中自检文档质量。

## 快速检查清单（PR 前）

1. 在标题、列表和代码块周围保留空行（避免 MD022 / MD032）。
1. 有序列表统一使用 `1.` 起始（避免 MD029）。
1. 避免裸 URL，使用链接语法（避免 MD034）。
1. 代码块请使用三反引号并指定语言（例如 \`\`\`powershell、\`\`\`json、\`\`\`csharp）。

## 常见规则与示例

### MD029 — 有序列表前缀

错误示例：

```markdown
3. 项目 A
4. 项目 B
```

推荐写法：

```markdown
1. 项目 A
1. 项目 B
```

说明：渲染器会自动编号，统一使用 `1.` 可以避免 lint 报错并便于维护。

### MD022 / MD032 — 标题与列表/段落周围的空行

错误示例：

```markdown
## 标题
- 列表项
```

修复示例：

```markdown
## 标题

- 列表项
```

说明：在标题与紧随其后的列表或段落之间加一个空行，能消除相关 lint 警告并提高可读性。

### MD034 — 裸 URL

错误示例：

```markdown
参见 https://abp.io/docs/latest/cli
```

修复示例：

```markdown
参见 [ABP CLI 文档](https://abp.io/docs/latest/cli)
```

说明：请使用 Markdown 链接语法来代替裸 URL，以便满足 lint 规则并获得更好的渲染效果。

## 在本地运行 lint（PowerShell）

可选：全局安装（若你希望多仓库重复使用）

```powershell
npm install -g markdownlint-cli
```

在仓库根运行（使用 .gitignore 忽略项）

```powershell
markdownlint "**/*.md" --ignore-path .gitignore
```

使用 npx（无需全局安装）

```powershell
npx markdownlint-cli "**/*.md"
```

## 推荐的 `.markdownlint.json`（示例）

将该文件放在仓库根可以覆盖或禁用特定规则。示例：禁用 MD029（仅在团队确实需要视觉编号时使用）。

```json
{
  "MD029": false
}
```

## CI 建议（GitHub Actions 思路）

- 在 CI 中加入一条任务运行 `npx markdownlint-cli "**/*.md"`，以防文档回归。
- 若需要，我可以为仓库创建一个 `.github/workflows/markdownlint.yml` 的示例工作流并提交。

## 结束语

本文档已整理并去重，保留关键示例与运行命令。请选择下一步操作：

- 添加 `.markdownlint.json` 到仓库根并提交
- 创建并提交一个 GitHub Actions 工作流用于 CI
- 两者都做
- 仅保留当前更改（无需进一步操作）

回复你想要的选项，我会直接创建并验证相应文件。
