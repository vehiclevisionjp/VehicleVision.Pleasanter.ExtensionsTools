# Copilot Instructions

このリポジトリは **{{PROJECT_NAME}}** です。

## プロジェクト情報

- **ターゲット**: .NET 10
- **言語バージョン**: C# latest
- **Nullable**: 有効
- **JSONライブラリ**: System.Text.Json

## コントリビューションガイドライン

コードやドキュメントを変更する際は、以下のガイドラインを必ず参照すること：

| ガイドライン | パス                                                                            | 内容                                     |
| ------------ | ------------------------------------------------------------------------------- | ---------------------------------------- |
| コーディング | [coding-guidelines.md](../docs/contributing/coding-guidelines.md)               | 命名規則、フォーマット、コードスタイル   |
| テスト       | [testing-guidelines.md](../docs/contributing/testing-guidelines.md)             | テストの書き方、実行方法、カバレッジ     |
| ドキュメント | [documentation-guidelines.md](../docs/contributing/documentation-guidelines.md) | Markdown記法、ファイル構成、同期ルール   |
| ブランチ戦略 | [branch-strategy.md](../docs/contributing/branch-strategy.md)                   | ブランチ命名、マージ方針                 |
| CI/CD        | [ci-workflow.md](../docs/contributing/ci-workflow.md)                           | 自動テスト、リリースプロセス             |
| 開発環境     | [development-environment.md](../docs/contributing/development-environment.md)   | Node.js、VS Code、.NET SDKのセットアップ |

> **重要**: `docs/contributing/` に新しいガイドラインを追加した場合は、上記テーブルにも追記すること。

## 変更時のルール

- ガイドラインの変更が必要な変更を行う場合は、関連するガイドライン（`docs/contributing/` 配下）も併せて変更すること
- コードを変更する際には、`docs/wiki/` 配下の関連ドキュメントも併せて変更すること

## 出力ルール

- 優先順位や処理の都合上、指示されたタスクの一部を実行しない・できない場合は、その旨を明示的にPromptで出力すること
- 省略した内容と理由を簡潔に説明し、必要に応じて後続の対応を提案すること
