# ExtensionsSyncTool 使用ガイド

プリザンターの Extensions テーブルとローカルの Parameters フォルダを双方向に同期する CLI ツールです。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [概要](#概要)
- [前提条件](#前提条件)
- [設定方法](#設定方法)
    - [appsettings.json](#appsettingsjson)
    - [環境変数](#環境変数)
    - [コマンドラインオプション](#コマンドラインオプション)
    - [優先順位](#優先順位)
- [コマンドリファレンス](#コマンドリファレンス)
    - [pull（DB → ファイル）](#pulldb--ファイル)
    - [push（ファイル → DB）](#pushファイル--db)
    - [validate（ローカルファイル検証）](#validateローカルファイル検証)
- [ファイルと DB のマッピング仕様](#ファイルと-db-のマッピング仕様)
    - [Script（ExtendedScripts）](#scriptextendedscripts)
    - [Style（ExtendedStyles）](#styleextendedstyles)
    - [Html（ExtendedHtmls）](#htmlextendedhtmls)
    - [ServerScript（ExtendedServerScripts）](#serverscriptextendedserverscripts)
    - [Sql（ExtendedSqls）](#sqlextendedsqls)
    - [Fields・NavigationMenu・Plugin（JSON のみ）](#fieldsnavigationmenupluginjson-のみ)
- [設計上の注意事項](#設計上の注意事項)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 概要

`ExtensionsSyncTool` は以下のコマンドをサポートします:

| コマンド   | 方向          | 説明                                              |
| ---------- | ------------- | ------------------------------------------------- |
| `pull`     | DB → ファイル | Extensions テーブルのレコードをファイルに書き出す |
| `push`     | ファイル → DB | Parameters フォルダのファイルを DB に登録・更新   |
| `validate` | ローカルのみ  | 拡張機能ファイルのバリデーションチェック          |

---

## 前提条件

- .NET SDK 10.0 以上
- プリザンター 1.3.13.0 以上（Extensions API が有効であること）
- プリザンターの API キー

---

## 設定方法

### appsettings.json

`src/ExtensionsSyncTool/appsettings.json` に設定します:

```json
{
    "BaseUrl": "https://pleasanter.example.com",
    "ApiKey": "REPLACE_WITH_YOUR_API_KEY",
    "ParametersPath": "REPLACE_WITH_PARAMETERS_PATH"
}
```

> **ヒント**: `local.config.json`（.gitignore 対象）に機密情報を記述することで、API キーをリポジトリにコミットせずに管理できます。

### 環境変数

`EXTENSIONS_SYNC_` プレフィックスを付けた環境変数でも設定できます:

```bash
export EXTENSIONS_SYNC_BaseUrl="https://pleasanter.example.com"
export EXTENSIONS_SYNC_ApiKey="your-api-key-here"
export EXTENSIONS_SYNC_ParametersPath="/path/to/Parameters"
```

### コマンドラインオプション

```bash
--base-url / -u    プリザンターサーバーのベース URL
--api-key / -k     プリザンター API キー
--parameters-path / -p  ローカルの Parameters ディレクトリのパス
--dry-run / -n     ドライラン（書き込みを行わない）
--rdbms / -r       SQL バリデーション対象の RDBMS（validate コマンド専用）
```

### 優先順位

コマンドラインオプション > 環境変数 > `local.config.json` > `appsettings.json`

---

## コマンドリファレンス

### pull（DB → ファイル）

Extensions テーブルの全レコードをローカルファイルとして書き出します。

```bash
dotnet run --project src/ExtensionsSyncTool -- pull \
  --base-url https://pleasanter.example.com \
  --api-key YOUR_API_KEY \
  --parameters-path /path/to/Parameters
```

ドライラン（対象件数の確認のみ）:

```bash
dotnet run --project src/ExtensionsSyncTool -- pull --dry-run \
  --base-url https://pleasanter.example.com \
  --api-key YOUR_API_KEY \
  --parameters-path /path/to/Parameters
```

### push（ファイル → DB）

Parameters フォルダの拡張機能ファイルを Extensions テーブルへ登録または更新します。

- 同名レコード（ExtensionType + ExtensionName が一致）が存在する場合は **更新**
- 存在しない場合は **新規作成**

```bash
dotnet run --project src/ExtensionsSyncTool -- push \
  --base-url https://pleasanter.example.com \
  --api-key YOUR_API_KEY \
  --parameters-path /path/to/Parameters
```

### validate（ローカルファイル検証）

ローカルの拡張機能ファイルに対してバリデーションチェックを実行します。対象フォーマットは JSON / JavaScript / HTML / CSS / SQL です。

```bash
dotnet run --project src/ExtensionsSyncTool -- validate \
  --parameters-path /path/to/Parameters
```

`--rdbms`（`-r`）オプションで SQL バリデーション対象の RDBMS を指定できます:

```bash
dotnet run --project src/ExtensionsSyncTool -- validate \
  --parameters-path /path/to/Parameters \
  --rdbms sqlserver
```

| オプション   | 説明                          |
| ------------ | ----------------------------- |
| `sqlserver`  | SQL Server 向けバリデーション |
| `mysql`      | MySQL 向けバリデーション      |
| `postgresql` | PostgreSQL 向けバリデーション |

---

## ファイルと DB のマッピング仕様

### Script（ExtendedScripts）

| 項目              | 値                            |
| ----------------- | ----------------------------- |
| フォルダ          | `Parameters/ExtendedScripts/` |
| ファイル形式      | `{name}.js`                   |
| ExtensionType     | `Script`                      |
| ExtensionName     | ファイル名（拡張子除く）      |
| Body              | ファイル内容（JavaScript）    |
| ExtensionSettings | なし                          |

### Style（ExtendedStyles）

| 項目              | 値                           |
| ----------------- | ---------------------------- |
| フォルダ          | `Parameters/ExtendedStyles/` |
| ファイル形式      | `{name}.css`                 |
| ExtensionType     | `Style`                      |
| ExtensionName     | ファイル名（拡張子除く）     |
| Body              | ファイル内容（CSS）          |
| ExtensionSettings | なし                         |

### Html（ExtendedHtmls）

| 項目              | 値                          |
| ----------------- | --------------------------- |
| フォルダ          | `Parameters/ExtendedHtmls/` |
| ファイル形式      | `{name}.html`               |
| ExtensionType     | `Html`                      |
| ExtensionName     | ファイル名（拡張子除く）    |
| Body              | ファイル内容（HTML）        |
| ExtensionSettings | なし                        |

### ServerScript（ExtendedServerScripts）

| 項目              | 値                                                              |
| ----------------- | --------------------------------------------------------------- |
| フォルダ          | `Parameters/ExtendedServerScripts/`                             |
| ファイル形式      | `{name}.json`（設定）+ `{name}.json.js`（スクリプト本体、任意） |
| ExtensionType     | `ServerScript`                                                  |
| ExtensionName     | JSON ファイル名（拡張子除く）                                   |
| Body              | `.json.js` ファイルの内容、または JSON 内 `Body` フィールドの値 |
| ExtensionSettings | JSON ファイルの内容（`Body` フィールドを除く）                  |

### Sql（ExtendedSqls）

| 項目              | 値                                                                      |
| ----------------- | ----------------------------------------------------------------------- |
| フォルダ          | `Parameters/ExtendedSqls/`                                              |
| ファイル形式      | `{name}.json`（設定）+ `{name}.json.sql`（SQL 本体、任意）              |
| ExtensionType     | `Sql`                                                                   |
| ExtensionName     | JSON ファイル名（拡張子除く）                                           |
| Body              | `.json.sql` ファイルの内容、または JSON 内 `CommandText` フィールドの値 |
| ExtensionSettings | JSON ファイルの内容（`Body`・`CommandText` フィールドを除く）           |

### Fields・NavigationMenu・Plugin（JSON のみ）

| ExtensionType  | フォルダ                   | ファイル形式  |
| -------------- | -------------------------- | ------------- |
| Fields         | `ExtendedFields/`          | `{name}.json` |
| NavigationMenu | `ExtendedNavigationMenus/` | `{name}.json` |
| Plugin         | `ExtendedPlugins/`         | `{name}.json` |

JSON ファイルの全内容が `ExtensionSettings` に格納されます。

---

## 設計上の注意事項

- `push` コマンドは既存レコードを ExtensionType + ExtensionName で検索して照合します。同名レコードが複数存在する場合、最初に見つかったものが更新されます。
- `pull` コマンドは既存ファイルを上書きします。
- `Disabled` フラグはファイルから push するとき `false`（デフォルト値）として登録されます。DB から pull した場合は DB の値が使われます。
