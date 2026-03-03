# テストガイドライン

このドキュメントでは、{{PROJECT_NAME}} プロジェクトのテスト規約について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [基本情報](#基本情報)
    - [テストプロジェクト構成](#テストプロジェクト構成)
    - [ディレクトリ構造](#ディレクトリ構造)
- [テスト実行](#テスト実行)
    - [コマンドライン](#コマンドライン)
    - [Visual Studio / VS Code](#visual-studio--vs-code)
- [カバレッジ](#カバレッジ)
- [テストの書き方](#テストの書き方)
    - [テスト命名規則](#テスト命名規則)
    - [テスト構造（AAA パターン）](#テスト構造aaa-パターン)
- [ベストプラクティス](#ベストプラクティス)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

---

## 基本情報

### テストプロジェクト構成

| 項目                     | 内容                     |
| ------------------------ | ------------------------ |
| プロジェクト名           | `{{PROJECT_NAME}}.Tests` |
| テストフレームワーク     | xUnit                    |
| ターゲットフレームワーク | .NET 10                  |
| カバレッジツール         | coverlet.collector       |

### ディレクトリ構造

<!-- TODO: プロジェクトに合わせて更新 -->

```text
{{PROJECT_NAME}}.Tests/
├── {{PROJECT_NAME}}.Tests.csproj
└── (テストファイル)
```

---

## テスト実行

### コマンドライン

```bash
# 全テスト実行
dotnet test

# 詳細出力
dotnet test --verbosity normal

# 特定テスト実行
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Visual Studio / VS Code

- **VS Code**: Test Explorer（C# Dev Kit）でテストを実行
- **Visual Studio**: テストエクスプローラーから実行

---

## カバレッジ

```bash
# カバレッジ収集
dotnet test --collect:"XPlat Code Coverage"

# ReportGeneratorでHTMLレポート生成
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"CodeCoverage" \
  -reporttypes:"Html"
```

---

## テストの書き方

### テスト命名規則

`{メソッド名}{条件}{期待結果}` の形式：

```csharp
// 例
public void GetRecordShouldReturnRecordWhenValidId()
public void CreateRecordShouldThrowWhenSiteIdIsZero()
```

### テスト構造（AAA パターン）

すべてのテストは **Arrange-Act-Assert** パターンに従う：

```csharp
[Fact]
public void ConstructorShouldInitializeCorrectly()
{
    // Arrange（準備）
    var baseUrl = "https://example.com";
    var apiKey = "test-api-key";

    // Act（実行）
    var client = new SampleClient(baseUrl, apiKey);

    // Assert（検証）
    Assert.NotNull(client);
}
```

---

## ベストプラクティス

- テストは**独立**して実行できること（テスト間の依存を避ける）
- **マジックナンバー**を避け、意味のある変数名を使用
- 各テストで**1つのこと**だけを検証する
- テストデータは**Arrange**セクションで明確に定義する
- 外部依存はモック化する
