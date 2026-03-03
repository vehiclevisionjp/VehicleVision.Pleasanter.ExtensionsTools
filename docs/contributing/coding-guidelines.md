# コーディングガイドライン

このドキュメントでは、{{PROJECT_NAME}} プロジェクトのコーディング規約について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [基本原則](#基本原則)
    - [プロジェクト設定](#プロジェクト設定)
    - [重要な方針](#重要な方針)
- [命名規則](#命名規則)
    - [一覧](#一覧)
    - [詳細ルール](#詳細ルール)
- [フォーマット](#フォーマット)
    - [インデントと空白](#インデントと空白)
    - [中括弧](#中括弧)
- [コードスタイル](#コードスタイル)
    - [var の使用](#var-の使用)
    - [パターンマッチング](#パターンマッチング)
    - [null チェック](#null-チェック)
    - [文字列](#文字列)
- [コメントとドキュメント](#コメントとドキュメント)
    - [XMLドキュメントコメント](#xmlドキュメントコメント)
- [非同期プログラミング](#非同期プログラミング)
- [エラーハンドリング](#エラーハンドリング)
- [ファイル構成](#ファイル構成)
    - [ディレクトリ構造](#ディレクトリ構造)
    - [usingディレクティブ](#usingディレクティブ)
    - [ファイル内の順序](#ファイル内の順序)
- [ツール設定](#ツール設定)
    - [EditorConfig](#editorconfig)
    - [Directory.Build.props](#directorybuildprops)
- [参考リンク](#参考リンク)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 基本原則

### プロジェクト設定

| 項目           | 設定             |
| -------------- | ---------------- |
| ターゲット     | .NET 10          |
| 言語           | C# latest        |
| Nullable       | enable           |
| JSONライブラリ | System.Text.Json |
| 暗黙的using    | enable           |

### 重要な方針

- **Newtonsoft.Json は使用しない**（System.Text.Json を使用）
- **Nullable 参照型**を有効にし、null安全なコードを書く
- **file-scoped namespace** を使用する
- すべての公開APIに**XMLドキュメントコメント**を記述する

---

## 命名規則

### 一覧

| 対象                   | スタイル           | 例                  |
| ---------------------- | ------------------ | ------------------- |
| クラス・構造体・列挙型 | PascalCase         | `PleasanterClient`  |
| インターフェース       | I + PascalCase     | `IRecordService`    |
| メソッド               | PascalCase         | `GetRecordsAsync`   |
| 非同期メソッド         | PascalCase + Async | `CreateRecordAsync` |
| プロパティ             | PascalCase         | `BaseUrl`           |
| 公開フィールド         | PascalCase         | `DefaultTimeout`    |
| プライベートフィールド | \_camelCase        | `_httpClient`       |
| パラメータ             | camelCase          | `siteId`            |
| ローカル変数           | camelCase          | `response`          |
| 定数                   | PascalCase         | `MaxRetryCount`     |
| 型パラメータ           | T + PascalCase     | `TResponse`         |

### 詳細ルール

- **ブール型**: `Is`、`Has`、`Can`、`Should` などのプレフィックスを使用
- **コレクション**: 複数形を使用（例: `records`、`items`）
- **略語**: 2文字の略語は全大文字（`IO`）、3文字以上は PascalCase（`Http`）

---

## フォーマット

### インデントと空白

- **インデント**: スペース4つ
- **行末の空白**: 削除
- **ファイル末尾**: 改行を挿入

### 中括弧

Allmanスタイル（新しい行に配置）：

```csharp
if (condition)
{
    DoSomething();
}
else
{
    DoSomethingElse();
}
```

---

## コードスタイル

### var の使用

型が明らかな場合は `var` を使用：

```csharp
// Good
var client = new HttpClient();
var records = new List<RecordData>();

// Bad - 型が不明な場合は明示
HttpResponseMessage response = await client.SendAsync(request);
```

### パターンマッチング

```csharp
// Good
if (obj is string text)
{
    Console.WriteLine(text);
}

// Good
if (value is not null)
{
    Process(value);
}
```

### null チェック

```csharp
// Good - is null / is not null を使用
if (value is null) { }
if (value is not null) { }

// Bad - == null / != null は使用しない
if (value == null) { }
if (value != null) { }
```

### 文字列

```csharp
// Good - 文字列補間を使用
var message = $"Record {recordId} created successfully";

// Bad - string.Format は使用しない
var message = string.Format("Record {0} created successfully", recordId);
```

---

## コメントとドキュメント

### XMLドキュメントコメント

すべての公開メンバーにXMLドキュメントコメントを記述する：

```csharp
/// <summary>
/// レコードを取得します
/// </summary>
/// <param name="siteId">サイトID</param>
/// <param name="recordId">レコードID</param>
/// <returns>レコードデータ</returns>
/// <exception cref="ArgumentException">siteId が 0 以下の場合</exception>
public async Task<RecordResponse> GetRecordAsync(long siteId, long recordId)
{
    // ...
}
```

---

## 非同期プログラミング

- すべての非同期メソッドに `Async` サフィックスを付ける
- `ConfigureAwait(false)` を使用する（ライブラリコードの場合）
- 非同期メソッドの戻り値は `Task` または `Task<T>` を使用

```csharp
public async Task<ApiResponse> GetDataAsync()
{
    var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
    return await response.Content.ReadFromJsonAsync<ApiResponse>().ConfigureAwait(false);
}
```

---

## エラーハンドリング

- `ArgumentNullException.ThrowIfNull()` を使用（.NET 7+）
- `ArgumentException.ThrowIfNullOrWhiteSpace()` を使用（.NET 8+）

```csharp
public void SetApiKey(string apiKey)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
    _apiKey = apiKey;
}
```

---

## ファイル構成

### ディレクトリ構造

```text
VehicleVision.Pleasanter.ExtensionsTools/
├── src/
│   └── ExtensionsSyncTool/
│       ├── Configuration/
│       ├── Models/
│       └── Services/
└── tests/
    └── ExtensionsSyncTool.Tests/
        └── Services/
```

### usingディレクティブ

- `System` 系を先頭に配置
- グループ間に空行は入れない
- 不要な `using` は削除

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ExtensionsSyncTool.Models;
```

### ファイル内の順序

```csharp
// 1. usingディレクティブ
// 2. 名前空間
// 3. クラス定義
//    4. 定数・静的フィールド
//    5. フィールド
//    6. コンストラクタ
//    7. プロパティ
//    8. 公開メソッド
//    9. 内部メソッド
//   10. プライベートメソッド
```

---

## ツール設定

### EditorConfig

プロジェクトルートの `.editorconfig` でコードスタイルを統一している。詳細は `.editorconfig` ファイルを参照。

### Directory.Build.props

プロジェクト全体の共通設定は `Directory.Build.props` で管理：

```xml
<Project>
  <PropertyGroup>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors Condition="'$(Configuration)' == 'Release'">true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
    <WarningLevel>5</WarningLevel>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

---

## 参考リンク

- [C# コーディング規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET コードスタイル規則](https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/style-rules/)
- [EditorConfig](https://editorconfig.org/)
