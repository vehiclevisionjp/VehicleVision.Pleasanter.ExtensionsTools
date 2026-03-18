#!/usr/bin/env node

/**
 * docs/ 配下の設計書を GitHub Wiki に同期するスクリプト
 *
 * このスクリプトはGitHub Actions環境でのみ実行可能です。
 * ローカル環境からの実行は許可されていません。
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// 環境変数の取得
const GITHUB_TOKEN = process.env.GITHUB_TOKEN;
const GITHUB_REPO = process.env.GITHUB_REPO || process.env.GITHUB_REPOSITORY;
const GITHUB_ACTIONS = process.env.GITHUB_ACTIONS === 'true';

// GitHub Actions環境でのみ実行を許可
if (!GITHUB_ACTIONS) {
  console.error('❌ このスクリプトはGitHub Actions環境でのみ実行できます');
  console.error('   ローカル環境からの実行は許可されていません');
  console.error('   Wiki同期はGitHub Actionsから自動的に実行されます');
  process.exit(1);
}

if (!GITHUB_TOKEN) {
  console.error('❌ GITHUB_TOKEN 環境変数が設定されていません');
  process.exit(1);
}

if (!GITHUB_REPO) {
  console.error('❌ GITHUB_REPO 環境変数が設定されていません');
  console.error('   例: GITHUB_REPO=owner/repo');
  process.exit(1);
}

const [owner, repo] = GITHUB_REPO.split('/');
if (!owner || !repo) {
  console.error('❌ GITHUB_REPO の形式が正しくありません');
  console.error('   例: GITHUB_REPO=owner/repo');
  process.exit(1);
}

// ここは実際の構成に合わせて変更してください
const DOCS_DIR = path.join(process.cwd(), 'docs', 'wiki');

// Wiki にのみ残すべきページ（削除対象外）
// [private] で始まるページは自動的に保護されます
const PROTECTED_WIKI_PAGES = [];

/**
 * docs/wiki/ 配下の Markdown ファイルを再帰的に取得
 */
function getMarkdownFiles(dir, basePath = '') {
  const files = [];
  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    const relativePath = path.join(basePath, entry.name);

    if (entry.isDirectory()) {
      files.push(...getMarkdownFiles(fullPath, relativePath));
    } else if (entry.isFile() && entry.name.endsWith('.md')) {
      files.push({
        filePath: fullPath,
        relativePath: relativePath,
        wikiTitle: getWikiTitle(relativePath),
      });
    }
  }

  return files;
}

/**
 * ファイルパスから Wiki ページタイトルを生成
 */
function getWikiTitle(relativePath) {
  const nameWithoutExt = relativePath.replace(/\.md$/, '');
  return nameWithoutExt.replace(/\//g, '-');
}

/**
 * Wiki 内部リンクの .md 拡張子を除去する
 *
 * docs/wiki/ のソースファイルでは相対リンクに .md 拡張子を付けて
 * リポジトリ上でもリンクが機能するようにしている。
 * GitHub Wiki ではファイル名（拡張子なし）でページを参照するため、
 * 同期時に .md 拡張子を除去する。
 *
 * 変換例:
 *   [テキスト](extensions-sync-tool.md)       → [テキスト](extensions-sync-tool)
 *   [テキスト](extensions-sync-tool.md#概要)  → [テキスト](extensions-sync-tool#概要)
 *   [テキスト](https://example.com/file.md)   → 変換しない（外部URL）
 *   [テキスト](../contributing/file.md)        → 変換しない（パス区切りあり）
 */
function transformWikiLinks(content) {
  return content.replace(
    /\]\(([^/)#:][^/)#:]*?)\.md(#[^)]*?)?\)/g,
    (_, filename, anchor) => `](${filename}${anchor || ''})`
  );
}

/**
 * Wiki ページを作成または更新（Gitリポジトリ経由）
 */
function createOrUpdateWikiPage(title, content, wikiDir) {
  const fileName = `${title}.md`;
  const filePath = path.join(wikiDir, fileName);

  const exists = fs.existsSync(filePath);

  fs.writeFileSync(filePath, content, 'utf-8');

  execSync(`cd "${wikiDir}" && git add "${fileName}"`, { stdio: 'pipe' });

  if (exists) {
    console.log(`✅ 更新: ${title}`);
  } else {
    console.log(`✨ 作成: ${title}`);
  }
}

/**
 * Wiki リポジトリからすべてのページを取得（Git操作のみ）
 */
function getAllWikiPages(wikiDir) {
  const pages = [];

  if (!fs.existsSync(wikiDir)) {
    return [];
  }

  const files = fs.readdirSync(wikiDir);
  for (const file of files) {
    if (file.endsWith('.md')) {
      const title = file.replace(/\.md$/, '');
      if (title !== '_Sidebar' && title !== 'Home') {
        pages.push({
          title: title,
          fileName: file,
        });
      }
    }
  }

  return pages;
}

/**
 * ページタイトルが保護対象かどうかを判定
 */
function isProtectedPage(title) {
  if (title.startsWith('[private]')) {
    return true;
  }

  return PROTECTED_WIKI_PAGES.some(protectedPage => {
    return title === protectedPage ||
      title === protectedPage.replace(/\//g, '-') ||
      title === protectedPage.replace(/-/g, '/');
  });
}

/**
 * Wiki ページを削除（Git操作のみ）
 */
function deleteWikiPages(wikiDir, pagesToDelete) {
  if (pagesToDelete.length === 0) {
    return 0;
  }

  let deletedCount = 0;
  for (const pageTitle of pagesToDelete) {
    const fileName = `${pageTitle}.md`;
    const filePath = path.join(wikiDir, fileName);

    if (fs.existsSync(filePath)) {
      execSync(`cd "${wikiDir}" && git rm "${fileName}"`, { stdio: 'pipe' });
      console.log(`🗑️  削除: ${pageTitle}`);
      deletedCount++;
    }
  }

  return deletedCount;
}

/**
 * メイン処理
 */
function main() {
  console.log(`📚 ${GITHUB_REPO} の Wiki に同期を開始します...\n`);

  if (!fs.existsSync(DOCS_DIR)) {
    console.error(`❌ docs/wiki/ ディレクトリが見つかりません: ${DOCS_DIR}`);
    process.exit(1);
  }

  const files = getMarkdownFiles(DOCS_DIR);
  console.log(`📄 ${files.length} 個の Markdown ファイルが見つかりました\n`);

  const wikiDir = path.join(process.cwd(), '.wiki-temp');
  const encodedToken = encodeURIComponent(GITHUB_TOKEN);
  const wikiRepoUrl = `https://x-access-token:${encodedToken}@github.com/${owner}/${repo}.wiki.git`;
  const gitUserName = process.env.GITHUB_ACTOR || 'github-actions[bot]';
  const gitUserEmail = process.env.GIT_COMMIT_EMAIL || `${gitUserName}@users.noreply.github.com`;

  try {
    const gitEnv = {
      ...process.env,
      GIT_TERMINAL_PROMPT: '0',
      GIT_ASKPASS: 'echo',
    };

    if (!fs.existsSync(wikiDir)) {
      console.log('📥 Wiki リポジトリをクローン中...\n');
      execSync(`git clone "${wikiRepoUrl}" "${wikiDir}"`, {
        stdio: 'inherit',
        env: gitEnv
      });
    } else {
      execSync(`cd "${wikiDir}" && git pull origin master`, {
        stdio: 'pipe',
        env: gitEnv
      });
    }

    execSync(`cd "${wikiDir}" && git remote set-url origin "${wikiRepoUrl}"`, {
      stdio: 'pipe',
      env: gitEnv,
    });

    execSync(`cd "${wikiDir}" && git config user.name "${gitUserName}"`, {
      stdio: 'pipe',
      env: gitEnv,
    });
    execSync(`cd "${wikiDir}" && git config user.email "${gitUserEmail}"`, {
      stdio: 'pipe',
      env: gitEnv,
    });

    const wikiPages = getAllWikiPages(wikiDir);
    const docsWikiTitles = new Set(files.map(f => f.wikiTitle));

    const protectedPages = wikiPages
      .filter(page => isProtectedPage(page.title))
      .map(page => ({
        title: page.title,
        wikiTitle: page.title,
        fileName: page.fileName,
      }));

    for (const file of files) {
      const rawContent = fs.readFileSync(file.filePath, 'utf-8');
      const content = transformWikiLinks(rawContent);
      createOrUpdateWikiPage(file.wikiTitle, content, wikiDir);
    }

    const sidebarContent = `# 目次

${files
        .filter(f => !f.relativePath?.includes('wiki-backup'))
        .map(f => `- [[${f.wikiTitle}|${f.wikiTitle}]]`)
        .join('\n')}

${protectedPages.length > 0 ? `## Wiki 専用ページ\n\n${protectedPages.map(p => `- [[${p.wikiTitle}|${p.wikiTitle}]]`).join('\n')}\n` : ''}
`;
    createOrUpdateWikiPage('_Sidebar', sidebarContent, wikiDir);

    const pagesToDelete = wikiPages
      .filter(page => {
        const title = page.title;
        if (isProtectedPage(title)) {
          return false;
        }
        return !docsWikiTitles.has(title);
      })
      .map(page => page.title);

    const allProtectedPages = wikiPages.filter(page => isProtectedPage(page.title));
    if (allProtectedPages.length > 0) {
      console.log(`\n🔒 保護された Wiki ページ（削除対象外）: ${allProtectedPages.length} 個`);
      allProtectedPages.forEach(page => console.log(`   - ${page.title}`));
    }

    if (pagesToDelete.length > 0) {
      console.log(`\n🗑️  削除対象の Wiki ページ: ${pagesToDelete.length} 個`);
      pagesToDelete.forEach(title => console.log(`   - ${title}`));
      deleteWikiPages(wikiDir, pagesToDelete);
    } else {
      console.log('\n✅ 削除対象のページはありませんでした');
    }

    try {
      execSync(`cd "${wikiDir}" && git diff --cached --quiet`, { stdio: 'pipe' });
      console.log('\n✅ 変更はありませんでした');
    } catch {
      console.log('\n💾 変更をコミット中...');
      execSync(`cd "${wikiDir}" && git commit -m "Sync docs/wiki to GitHub Wiki"`, { stdio: 'inherit' });

      console.log('📤 変更をプッシュ中...');
      execSync(`cd "${wikiDir}" && git push origin master`, {
        stdio: 'inherit',
        env: gitEnv
      });

      console.log('\n✅ Wikiへの同期が完了しました！');
    }
  } finally {
    if (fs.existsSync(wikiDir)) {
      execSync(`rm -rf "${wikiDir}"`, { stdio: 'pipe' });
    }
  }
}

try {
  main();
} catch (error) {
  console.error('\n❌ エラーが発生しました:', error);
  process.exit(1);
}
