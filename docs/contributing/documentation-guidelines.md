# ドキュメントガイドライン

このドキュメントでは、{{PROJECT_NAME}} プロジェクトのドキュメント作成規約について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [ドキュメントガイドライン](#ドキュメントガイドライン)
  - [基本原則](#基本原則)
    - [言語](#言語)
    - [対象読者](#対象読者)
  - [ファイル構成](#ファイル構成)
    - [ディレクトリ構造](#ディレクトリ構造)
    - [ファイル命名規則](#ファイル命名規則)
      - [`docs/wiki/` 配下](#docswiki-配下)
      - [`docs/contributing/` 配下](#docscontributing-配下)
  - [Markdownスタイル](#markdownスタイル)
    - [基本ルール](#基本ルール)
    - [フォーマッター（Prettier）](#フォーマッターprettier)
    - [Linter（markdownlint）](#lintermarkdownlint)
  - [npmスクリプト](#npmスクリプト)
  - [TOC（目次）自動生成](#toc目次自動生成)
  - [ドキュメント同期](#ドキュメント同期)
    - [更新ルール](#更新ルール)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 基本原則

### 言語

- ドキュメントは**日本語**で記述する
- コード内のコメント（XMLドキュメントコメント含む）も日本語

### 対象読者

- プロジェクトの開発者・コントリビューター

---

## ファイル構成

### ディレクトリ構造

```text
docs/
├── contributing/
│   ├── branch-strategy.md
│   ├── ci-workflow.md
│   ├── coding-guidelines.md
│   ├── development-environment.md
│   ├── documentation-guidelines.md
│   └── testing-guidelines.md
├── script/
│   ├── decode-toc.js
│   ├── generate-pdf.js
│   ├── github-markdown.css
│   ├── sync-docs-to-wiki.js
│   └── toc-single.js
└── wiki/
    ├── Home.md
    └── (Wikiドキュメント)
```

### ファイル命名規則

#### `docs/wiki/` 配下

- ケバブケース（`example-document.md`）
- カテゴリごとにサブディレクトリで整理

#### `docs/contributing/` 配下

- ケバブケース（`coding-guidelines.md`）

---

## Markdownスタイル

### 基本ルール

| ルール                   | 内容                                      |
| ------------------------ | ----------------------------------------- |
| 見出し                   | ATX形式（`# H1`）を使用                  |
| 見出しレベル             | 1つずつ順番に（H1 → H2 → H3）           |
| リスト                   | `-` を使用（`*` は不可）                 |
| コードブロック           | バッククォート3つで囲む                  |
| 行の長さ               | 120文字以内（テーブル・コードブロック除外）|
| 絵文字                   | 使用しない                                |
| HTMLタグ                 | 原則使用しない（`<br>` のみ許可）         |
| 型名                     | バッククォートで囲む（例: \`string\`）    |

### フォーマッター（Prettier）

```bash
# フォーマット実行
npm run format

# フォーマットチェック（変更なし）
npm run format:check
```

### Linter（markdownlint）

```bash
# Lintチェック
npm run lint:md

# 自動修正
npm run lint:md:fix
```

---

## npmスクリプト

| スクリプト      | 説明                              |
| --------------- | --------------------------------- |
| `lint:md`       | Markdownの構文チェック            |
| `lint:md:fix`   | Markdownのlintエラーを自動修正    |
| `format`        | Prettierでフォーマット            |
| `format:check`  | フォーマットのチェック            |
| `toc`           | doctocでTOCを一括更新             |
| `toc:all`       | TOC更新 + フォーマットを一括実行  |
| `pdf`           | 全MarkdownをPDFに変換             |
| `pdf:wiki`      | WikiドキュメントのみPDF変換       |

---

## TOC（目次）自動生成

[doctoc](https://github.com/thlorenz/doctoc) を使用してTOCを自動生成する。

```bash
# 全ファイルのTOC更新
npm run toc

# TOC更新 + フォーマット
npm run toc:all
```

VS Codeでは **RunOnSave** 拡張機能により、`docs/` 配下のMarkdownファイル保存時にTOCが自動更新される。

---

## ドキュメント同期

### 更新ルール

| 変更内容                                | 更新が必要なドキュメント                                      |
| --------------------------------------- | ------------------------------------------------------------- |
| 公開APIの追加・変更                     | `docs/wiki/` 配下の該当ドキュメント                           |
| ガイドラインの追加                      | `CONTRIBUTING.md` および `.github/copilot-instructions.md`    |
| プロジェクト設定の変更                  | `README.md` および `.github/copilot-instructions.md`          |
| セキュリティ脆弱性の報告対応            | `README.md` の謝辞セクション（報告者名を追記）                |
