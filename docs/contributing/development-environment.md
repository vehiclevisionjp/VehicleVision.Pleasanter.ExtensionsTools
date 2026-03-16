# 開発環境構築ガイド

このドキュメントでは、VehicleVision.Pleasanter.ExtensionsTools プロジェクトの開発環境セットアップについて説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [必要なツール](#必要なツール)
- [.NET SDK](#net-sdk)
    - [インストール](#インストール)
    - [バージョン確認](#バージョン確認)
- [Node.js（推奨）](#nodejs推奨)
    - [インストール](#インストール-1)
    - [パッケージのインストール](#パッケージのインストール)
- [IDE（Visual Studio Code）](#idevisual-studio-code)
    - [推奨拡張機能](#推奨拡張機能)
    - [ビルドタスク](#ビルドタスク)
- [セットアップ確認](#セットアップ確認)
- [参考リンク](#参考リンク)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 必要なツール

| ツール   | 用途                             | 必須 |
| -------- | -------------------------------- | ---- |
| .NET SDK | ライブラリのビルド・テスト       | 必須 |
| Node.js  | ドキュメントのlint・フォーマット | 推奨 |
| VS Code  | 推奨エディタ                     | 推奨 |
| Git      | バージョン管理                   | 必須 |

---

## .NET SDK

### インストール

[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) をインストールする。

#### Windows

```powershell
winget install Microsoft.DotNet.SDK.10
```

#### macOS

```bash
brew install dotnet-sdk
```

#### Linux（Ubuntu/Debian）

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

### バージョン確認

```bash
dotnet --version
```

---

## Node.js（推奨）

ドキュメントのlint、フォーマット、TOC生成に使用する。

### インストール

[Node.js 公式サイト](https://nodejs.org/)からLTS版をインストールする。

```bash
node --version
npm --version
```

### パッケージのインストール

```bash
npm install
```

---

## IDE（Visual Studio Code）

### 推奨拡張機能

本プロジェクトでは以下の拡張機能を推奨している（`.vscode/extensions.json` に定義済み）。

| 拡張機能ID                       | 名称         | 用途                 |
| -------------------------------- | ------------ | -------------------- |
| `ms-dotnettools.csharp`          | C#           | C#言語サポート       |
| `ms-dotnettools.csdevkit`        | C# Dev Kit   | .NET開発の統合支援   |
| `esbenp.prettier-vscode`         | Prettier     | コードフォーマッター |
| `editorconfig.editorconfig`      | EditorConfig | エディタ設定の統一   |
| `davidanson.vscode-markdownlint` | markdownlint | Markdown構文チェック |
| `emeraldwalk.runonsave`          | RunOnSave    | 保存時のTOC自動更新  |

### ビルドタスク

プロジェクトには `.vscode/tasks.json` が含まれており、VS Code のタスク機能でビルドやドキュメント操作を実行できる。

#### ビルドタスク

| タスク            | 説明                         |
| ----------------- | ---------------------------- |
| `restore`         | NuGetパッケージの復元        |
| `build`           | プロジェクトのビルド（既定） |
| `build (Release)` | リリースビルド               |
| `clean`           | ビルド成果物のクリーン       |
| `rebuild`         | クリーン → 復元 → ビルド     |
| `format`          | C#コードのフォーマット       |

#### ドキュメントタスク（npm）

| タスク              | 説明                               |
| ------------------- | ---------------------------------- |
| `npm: lint:md`      | Markdownの構文チェック             |
| `npm: lint:md:fix`  | Markdownのlintエラーを自動修正     |
| `npm: format`       | Prettierでフォーマット             |
| `npm: format:check` | フォーマットのチェック（変更なし） |
| `npm: toc`          | doctocでTOCを一括更新              |
| `npm: toc:all`      | TOC更新 + フォーマットを一括実行   |
| `npm: pdf`          | 全MarkdownをPDFに変換              |
| `npm: pdf:wiki`     | WikiドキュメントのみPDF変換        |

#### タスクの実行方法

1. `Ctrl+Shift+P` でコマンドパレットを開く
2. `Tasks: Run Task` を選択
3. 実行したいタスクを選択

---

## セットアップ確認

以下のコマンドですべてが正しくセットアップされていることを確認する。

```bash
# .NET SDK
dotnet --version

# Node.js（推奨）
node --version
npm --version

# ビルド確認
dotnet restore
dotnet build
```

---

## 参考リンク

- [.NET ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/)
- [Node.js 公式サイト](https://nodejs.org/ja/)
- [Visual Studio Code ドキュメント](https://code.visualstudio.com/docs)
