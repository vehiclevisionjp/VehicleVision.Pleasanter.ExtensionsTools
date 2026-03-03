# VehicleVision.Pleasanter.ExtensionsTools

<!-- markdownlint-disable MD013 -->

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![Pleasanter](https://img.shields.io/badge/Pleasanter-1.3.13.0%2B-00A0E9)](https://pleasanter.org/) [![License](https://img.shields.io/badge/License-LGPL--2.1-blue.svg)](LICENSE)

<!-- markdownlint-enable MD013 -->

プリザンターの **Extensions テーブル**とローカルの **Parameters フォルダ**を双方向に同期するクロスプラットフォーム対応 CLI ツールです。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [セットアップ](#セットアップ)
    - [前提条件](#前提条件)
    - [クローン](#クローン)
    - [ビルド](#ビルド)
- [使用方法](#使用方法)
    - [設定](#設定)
    - [コマンド](#コマンド)
        - [pull（DB → ファイル）](#pulldb--ファイル)
        - [push（ファイル → DB）](#pushファイル--db)
        - [ドライラン](#ドライラン)
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

### コマンド

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

## プロジェクト構成

```text
VehicleVision.Pleasanter.ExtensionsTools/
├── Implem.Pleasanter/              # サブモジュール（Pleasanter 本体コード参照用）
├── src/
│   ├── Common/                     # 共有クラスライブラリ
│   │   ├── Configuration/          # 設定クラス
│   │   ├── Models/                 # データモデル・API モデル
│   │   └── Services/               # ビジネスロジック（API クライアント・ファイルサービス・同期）
│   └── ExtensionsSyncTool/         # 同期 CLI ツール（Program.cs のみ）
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

ライセンスファイルの全文は [LICENSES](./LICENSES/) フォルダを参照してください。

## セキュリティ

セキュリティ上の脆弱性を発見された場合は、[セキュリティポリシー](.github/SECURITY.md)をご確認の上、ご報告ください。

## 謝辞

セキュリティ脆弱性の報告やプロジェクトへの貢献をしてくださった方々に感謝いたします。

<!-- 貢献者・報告者はこちらに追記 -->
