# {{PROJECT_NAME}}

<!-- markdownlint-disable MD013 -->

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![Pleasanter](https://img.shields.io/badge/Pleasanter-1.3.13.0%2B-00A0E9)](https://pleasanter.org/) [![License](https://img.shields.io/badge/License-LGPL--2.1-blue.svg)](LICENSE)

<!-- markdownlint-enable MD013 -->

{{PROJECT_DESCRIPTION}}

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [セットアップ](#セットアップ)
    - [前提条件](#前提条件)
    - [クローン](#クローン)
    - [ビルド](#ビルド)
- [使用方法](#使用方法)
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
git clone https://github.com/vehiclevisionjp/{{PROJECT_NAME}}.git
cd {{PROJECT_NAME}}
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

<!-- TODO: プロジェクトに合わせて記述 -->

## プロジェクト構成

```text
{{PROJECT_NAME}}/
├── .github/                    # GitHub設定（CI/CD、セキュリティポリシー等）
│   ├── copilot-instructions.md
│   ├── SECURITY.md
│   └── workflows/
│       └── sync-wiki.yml
├── .vscode/                    # VS Code設定
│   ├── extensions.json
│   ├── settings.json
│   └── tasks.json
├── docs/                       # ドキュメント
│   ├── contributing/           # 開発者向けガイドライン
│   ├── script/                 # ドキュメント用スクリプト
│   └── wiki/                   # Wikiドキュメント
├── LICENSES/                   # サードパーティライセンス
├── .editorconfig
├── .gitignore
├── .markdownlint-cli2.jsonc
├── .prettierignore
├── .prettierrc
├── AUTHORS
├── CONTRIBUTING.md
├── LICENSE
├── README.md
└── package.json
```

## サードパーティライセンス

このプロジェクトは以下のサードパーティライブラリを使用しています：

<!-- TODO: 使用するライブラリに合わせて更新 -->

| ライブラリ | ライセンス | 著作権 |
| ---------- | ---------- | ------ |
|            |            |        |

ライセンスファイルの全文は [LICENSES](./LICENSES/) フォルダを参照してください。

## セキュリティ

セキュリティ上の脆弱性を発見された場合は、[セキュリティポリシー](.github/SECURITY.md)をご確認の上、ご報告ください。

## 謝辞

セキュリティ脆弱性の報告やプロジェクトへの貢献をしてくださった方々に感謝いたします。

<!-- 貢献者・報告者はこちらに追記 -->
