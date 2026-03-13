# VehicleVision.Pleasanter.ExtensionsTools

<!-- markdownlint-disable MD013 -->

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![Pleasanter](https://img.shields.io/badge/Pleasanter-1.3.13.0%2B-00A0E9)](https://pleasanter.org/) [![License](https://img.shields.io/badge/License-LGPL--2.1-blue.svg)](LICENSE)

<!-- markdownlint-enable MD013 -->

プリザンターの **Extensions テーブル**とローカルの **Parameters フォルダ**を双方向に同期するクロスプラットフォーム対応ツールです。CLI ツールに加え、WinMerge 風の差分ビューアデスクトップアプリを提供します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [セットアップ](#セットアップ)
    - [前提条件](#前提条件)
    - [クローン](#クローン)
    - [ビルド](#ビルド)
- [使用方法](#使用方法)
    - [設定](#設定)
    - [コマンド（CLI）](#コマンドcli)
        - [pull（DB → ファイル）](#pulldb--ファイル)
        - [push（ファイル → DB）](#pushファイル--db)
        - [ドライラン](#ドライラン)
    - [デスクトップ版（ExtensionsDiffViewer）](#デスクトップ版extensionsdiffviewer)
- [プロジェクト構成](#プロジェクト構成)
- [サードパーティライセンス](#サードパーティライセンス)
- [セキュリティ](#セキュリティ)
- [謝辞](#謝辞)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## セットアップ

### 前提条件

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) 以上
- [Node.js](https://nodejs.org/) （ドキュメントのlint・フォーマット用、推奨）
- [Git](https://git-scm.com/)

### クローン

```bash
git clone https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ExtensionsTools.git
cd VehicleVision.Pleasanter.ExtensionsTools
git submodule update --init --recursive
```

### ビルド

```bash
dotnet restore
dotnet build
```

#### ドキュメントツールのセットアップ（任意）

```bash
npm install
```

## 使用方法

### 設定

`src/ExtensionsSyncTool/appsettings.json` を編集するか、環境変数（`EXTENSIONS_SYNC_` プレフィックス）またはコマンドラインオプションで設定します。

```json
{
  "BaseUrl": "https://pleasanter.example.com",
  "ApiKey": "REPLACE_WITH_YOUR_API_KEY",
  "ParametersPath": "REPLACE_WITH_PARAMETERS_PATH"
}
```

| 設定キー        | 環境変数                         | 説明                                   |
| --------------- | -------------------------------- | -------------------------------------- |
| `BaseUrl`       | `EXTENSIONS_SYNC_BaseUrl`        | プリザンターサーバーのベース URL       |
| `ApiKey`        | `EXTENSIONS_SYNC_ApiKey`         | プリザンター API キー                  |
| `ParametersPath`| `EXTENSIONS_SYNC_ParametersPath` | ローカルの Parameters ディレクトリパス |

> **注意**: `local.config.json` は `.gitignore` に含まれているため、API キー等の機密情報をローカル設定として安全に管理できます。

### コマンド（CLI）

#### pull（DB → ファイル）

Extensions テーブルのレコードをローカルファイルとして書き出します。

```bash
dotnet run --project src/ExtensionsSyncTool -- pull \
  --base-url https://pleasanter.example.com \
  --api-key YOUR_API_KEY \
  --parameters-path /path/to/Parameters
```

#### push（ファイル → DB）

ローカルファイルを Extensions テーブルへ登録・更新します。

```bash
dotnet run --project src/ExtensionsSyncTool -- push \
  --base-url https://pleasanter.example.com \
  --api-key YOUR_API_KEY \
  --parameters-path /path/to/Parameters
```

#### ドライラン

`--dry-run`（`-n`）オプションを付けると、実際の書き込み・API 呼び出しを行わずに対象件数の確認ができます。

```bash
dotnet run --project src/ExtensionsSyncTool -- pull --dry-run ...
dotnet run --project src/ExtensionsSyncTool -- push --dry-run ...
```

### デスクトップ版（ExtensionsDiffViewer）

WinMerge 風の左右ペインでサーバー（DB）とローカル（ファイル）の差分を視覚的に比較できるデスクトップアプリケーションです。

#### 起動方法

```bash
dotnet run --project src/ExtensionsDiffViewer
```

#### 機能

- **差分比較**: サーバーとローカルの拡張機能を一覧表示し、差分ステータス（一致・変更あり・サーバーのみ・ローカルのみ）を色分け表示
- **左右ペイン**: 選択した拡張機能のサーバー側・ローカル側コンテンツを左右に並べて表示
- **個別 Pull/Push**: 差分を確認しながら、個別の拡張機能を Pull（サーバー → ローカル）または Push（ローカル → サーバー）
- **全件 Pull/Push**: すべての拡張機能を一括で同期

#### 設定

CLI ツールと同じ設定方法（`appsettings.json`、`local.config.json`、環境変数）が使用できます。GUI 上部の入力欄からも直接設定できます。

## プロジェクト構成

```text
VehicleVision.Pleasanter.ExtensionsTools/
├── Implem.Pleasanter/              # サブモジュール（Pleasanter 本体コード参照用）
├── src/
│   ├── Common/                     # 共有クラスライブラリ
│   │   ├── Configuration/          # 設定クラス
│   │   ├── Models/                 # データモデル・API モデル・差分モデル
│   │   └── Services/               # ビジネスロジック（API クライアント・ファイルサービス・同期・差分比較）
│   ├── ExtensionsSyncTool/         # 同期 CLI ツール（Program.cs のみ）
│   └── ExtensionsDiffViewer/       # デスクトップ版 差分ビューア（Avalonia UI）
│       ├── ViewModels/             # MVVM ViewModel
│       └── Views/                  # AXAML ビュー
├── tests/
│   └── ExtensionsSyncTool.Tests/   # xUnit テストプロジェクト
├── .github/                        # GitHub 設定（CI/CD、セキュリティポリシー等）
├── docs/                           # ドキュメント
├── LICENSES/                       # サードパーティライセンス
├── .editorconfig
├── .gitignore
├── .gitmodules
└── VehicleVision.Pleasanter.ExtensionsTools.slnx
```

### ファイルと Extensions テーブルのマッピング

| ExtensionType  | Parameters フォルダ       | ファイル形式              |
| -------------- | ------------------------- | ------------------------- |
| Script         | `ExtendedScripts/`        | `*.js`                    |
| Style          | `ExtendedStyles/`         | `*.css`                   |
| Html           | `ExtendedHtmls/`          | `*.html`                  |
| ServerScript   | `ExtendedServerScripts/`  | `*.json` + `*.json.js`    |
| Sql            | `ExtendedSqls/`           | `*.json` + `*.json.sql`   |
| Fields         | `ExtendedFields/`         | `*.json`                  |
| NavigationMenu | `ExtendedNavigationMenus/`| `*.json`                  |
| Plugin         | `ExtendedPlugins/`        | `*.json`                  |

## サードパーティライセンス

このプロジェクトは以下のサードパーティライブラリを使用しています：

| ライブラリ                                      | ライセンス | 著作権                      |
| ----------------------------------------------- | ---------- | --------------------------- |
| [System.CommandLine](https://github.com/dotnet/command-line-api) | MIT | .NET Foundation |
| [Microsoft.Extensions.Http](https://github.com/dotnet/runtime)   | MIT | .NET Foundation |
| [Microsoft.Extensions.Configuration](https://github.com/dotnet/runtime) | MIT | .NET Foundation |
| [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) | MIT | .NET Foundation |
| [Avalonia](https://github.com/AvaloniaUI/Avalonia)               | MIT | AvaloniaUI OÜ |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MIT | .NET Foundation |

ライセンスファイルの全文は [LICENSES](./LICENSES/) フォルダを参照してください。

## セキュリティ

セキュリティ上の脆弱性を発見された場合は、[セキュリティポリシー](.github/SECURITY.md)をご確認の上、ご報告ください。

## 謝辞

セキュリティ脆弱性の報告やプロジェクトへの貢献をしてくださった方々に感謝いたします。

<!-- 貢献者・報告者はこちらに追記 -->
