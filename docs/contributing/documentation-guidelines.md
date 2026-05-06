# ドキュメントガイドライン

このドキュメントでは、VehicleVision.Pleasanter.ExtensionsTools プロジェクトのドキュメント作成規約について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [基本原則](#基本原則)
    - [言語](#言語)
    - [対象読者とドキュメントの役割](#対象読者とドキュメントの役割)
- [ファイル構成](#ファイル構成)
    - [ディレクトリ構造](#ディレクトリ構造)
    - [ファイル命名規則](#ファイル命名規則)
- [Markdownスタイル](#markdownスタイル)
    - [基本ルール](#基本ルール)
    - [フォーマッター（Prettier）](#フォーマッターprettier)
    - [Linter（markdownlint）](#lintermarkdownlint)
- [npmスクリプト](#npmスクリプト)
- [TOC（目次）自動生成](#toc目次自動生成)
- [ドキュメント同期](#ドキュメント同期)
    - [更新ルール](#更新ルール)
- [Wiki の構成と運用](#wiki-の構成と運用)
    - [Wiki の構成](#wiki-の構成)
    - [Wiki リンクの記述規約](#wiki-リンクの記述規約)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 基本原則

### 言語

- ドキュメントは**日本語**で記述する
- コード内のコメント（XMLドキュメントコメント含む）も日本語

### 対象読者とドキュメントの役割

| ドキュメント         | 対象読者                         | 内容                                                 |
| -------------------- | -------------------------------- | ---------------------------------------------------- |
| `README.md`          | **利用者**（ユーザー）           | セットアップ、使用方法、設定、ライセンス             |
| `CONTRIBUTING.md`    | **開発者**（コントリビューター） | 開発環境構築、コーディング規約、テスト、ブランチ戦略 |
| `docs/wiki/`         | **利用者**（ユーザー）           | 各ツールの詳細な使用ガイド・リファレンス             |
| `docs/contributing/` | **開発者**（コントリビューター） | 各種ガイドラインの詳細                               |

- `README.md` には利用者が必要とする情報（セットアップ手順、使用方法、設定方法）を記載する
- `CONTRIBUTING.md` には開発者が必要とする情報（開発環境構築、コーディング規約、テスト手順）を記載する
- 開発者向けの詳細なツール設定（npm、lint 等）は `CONTRIBUTING.md` または `docs/contributing/` に記載する

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
    ├── extensions-diff-viewer.md
    └── extensions-sync-tool.md
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

| ルール         | 内容                                        |
| -------------- | ------------------------------------------- |
| 見出し         | ATX形式（`# H1`）を使用                     |
| 見出しレベル   | 1つずつ順番に（H1 → H2 → H3）               |
| リスト         | `-` を使用（`*` は不可）                    |
| コードブロック | バッククォート3つで囲む                     |
| 行の長さ       | 120文字以内（テーブル・コードブロック除外） |
| 絵文字         | 使用しない                                  |
| HTMLタグ       | 原則使用しない（`<br>` のみ許可）           |
| 型名           | バッククォートで囲む（例: \`string\`）      |

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

| スクリプト     | 説明                             |
| -------------- | -------------------------------- |
| `lint:md`      | Markdownの構文チェック           |
| `lint:md:fix`  | Markdownのlintエラーを自動修正   |
| `format`       | Prettierでフォーマット           |
| `format:check` | フォーマットのチェック           |
| `toc`          | doctocでTOCを一括更新            |
| `toc:file`     | 単一ファイルのTOCを更新          |
| `toc:all`      | TOC更新 + フォーマットを一括実行 |
| `pdf`          | 全MarkdownをPDFに変換            |
| `pdf:wiki`     | WikiドキュメントのみPDF変換      |

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

| 変更内容                     | 更新が必要なドキュメント                                   |
| ---------------------------- | ---------------------------------------------------------- |
| 公開APIの追加・変更          | `docs/wiki/` 配下の該当ドキュメント                        |
| ガイドラインの追加           | `CONTRIBUTING.md` および `.github/copilot-instructions.md` |
| プロジェクト設定の変更       | `README.md` および `.github/copilot-instructions.md`       |
| セキュリティ脆弱性の報告対応 | `README.md` の謝辞セクション（報告者名を追記）             |

---

## Wiki の構成と運用

### Wiki の構成

`docs/wiki/` はCIにより GitHub Wiki リポジトリへ自動同期される。体系的な構成を維持すること。

- `Home.md` はランディングページとして、プロジェクト概要とツール使用ガイドへのリンクを記載する
- 各ツールのガイドは独立したファイルとして作成する
- `Home.md` ではガイドをカテゴリ別にテーブル形式で整理する

### Wiki リンクの記述規約

`docs/wiki/` 配下のファイル間リンクには `.md` 拡張子を**付ける**こと。

```markdown
<!-- ○ 正しい（リポジトリ上でもリンクが機能する） -->

[ExtensionsSyncTool 使用ガイド](extensions-sync-tool.md)

<!-- × 誤り（リポジトリ上でリンクが機能しない） -->

[ExtensionsSyncTool 使用ガイド](extensions-sync-tool)
```

CI の Wiki 同期スクリプト（`docs/script/sync-docs-to-wiki.js`）が、同期時に `.md` 拡張子を自動で除去するため、GitHub Wiki 上でも正しくリンクが機能する。

#### 外部リンク

リポジトリ外へのリンク（README、CONTRIBUTING 等）は絶対 URL を使用する。

```markdown
[README](https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ExtensionsTools)
```
