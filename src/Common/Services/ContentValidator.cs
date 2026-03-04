using System.Text.Json;
using System.Text.RegularExpressions;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// 拡張機能ファイルのコンテンツバリデーションの実装
/// </summary>
/// <remarks>
/// 各 ExtensionType に応じて以下のチェックを行います:
///
/// | ExtensionType           | 設定 JSON | コンテンツ            |
/// |-------------------------|-----------|-----------------------|
/// | Script                  | —         | JavaScript 簡易構文   |
/// | Style                   | —         | CSS 簡易構文          |
/// | Html                    | —         | HTML 簡易構文（断片） |
/// | ServerScript            | JSON      | JavaScript 簡易構文   |
/// | Sql                     | JSON      | —                     |
/// | Fields / NavigationMenu / Plugin | JSON | —              |
/// </remarks>
public partial class ContentValidator : IContentValidator
{
    /// <inheritdoc/>
    public IReadOnlyList<ValidationResult> ValidateAll(IReadOnlyList<ExtensionFileEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var results = new List<ValidationResult>();
        foreach (var entry in entries)
        {
            results.AddRange(Validate(entry));
        }

        return results;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ValidationResult> Validate(ExtensionFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var results = new List<ValidationResult>();

        if (entry.SettingsJson is not null)
        {
            results.Add(ValidateJson(entry, entry.SettingsJson, entry.SettingsFilePath));
        }

        if (entry.Content is not null)
        {
            var contentResult = entry.ExtensionType switch
            {
                "Script" or "ServerScript" => ValidateJavaScript(entry, entry.Content, entry.ContentFilePath),
                "Style" => ValidateCss(entry, entry.Content, entry.ContentFilePath),
                "Html" => ValidateHtml(entry, entry.Content, entry.ContentFilePath),
                _ => null,
            };

            if (contentResult is not null)
            {
                results.Add(contentResult);
            }
        }

        return results;
    }

    /// <summary>
    /// JSON のバリデーションを実行します
    /// </summary>
    internal static ValidationResult ValidateJson(ExtensionFileEntry entry, string json, string? filePath)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("JSON が空です。");
            return CreateResult(entry, filePath, errors);
        }

        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
            });
        }
        catch (JsonException ex)
        {
            var message = $"JSON 構文エラー: {ex.Message}";
            if (ex.LineNumber.HasValue)
            {
                message = $"JSON 構文エラー (行 {ex.LineNumber + 1}, 位置 {ex.BytePositionInLine}): {ex.Message}";
            }

            errors.Add(message);
        }

        return CreateResult(entry, filePath, errors);
    }

    /// <summary>
    /// JavaScript の簡易構文チェックを実行します
    /// </summary>
    internal static ValidationResult ValidateJavaScript(ExtensionFileEntry entry, string content, string? filePath)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return CreateResult(entry, filePath, errors);
        }

        CheckBracketBalance(content, errors, removeJsCommentsAndStrings: true);

        return CreateResult(entry, filePath, errors);
    }

    /// <summary>
    /// CSS の簡易構文チェックを実行します
    /// </summary>
    internal static ValidationResult ValidateCss(ExtensionFileEntry entry, string content, string? filePath)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return CreateResult(entry, filePath, errors);
        }

        var cleaned = RemoveCssCommentsAndStrings(content);
        CheckBracketBalance(cleaned, errors, removeJsCommentsAndStrings: false);

        return CreateResult(entry, filePath, errors);
    }

    /// <summary>
    /// HTML の簡易構文チェックを実行します（断片 HTML 対応）
    /// </summary>
    /// <remarks>
    /// Pleasanter の拡張 HTML は既存ページに埋め込まれる断片であり、
    /// &lt;html&gt; や &lt;!DOCTYPE&gt; で始まる必要はありません。
    /// </remarks>
    internal static ValidationResult ValidateHtml(ExtensionFileEntry entry, string content, string? filePath)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return CreateResult(entry, filePath, errors);
        }

        CheckHtmlTagBalance(content, errors);

        return CreateResult(entry, filePath, errors);
    }

    private static void CheckBracketBalance(string content, List<string> errors, bool removeJsCommentsAndStrings)
    {
        var cleaned = removeJsCommentsAndStrings
            ? RemoveJsCommentsAndStrings(content)
            : content;

        var braceCount = 0;
        var parenCount = 0;
        var bracketCount = 0;

        foreach (var ch in cleaned)
        {
            switch (ch)
            {
                case '{': braceCount++; break;
                case '}': braceCount--; break;
                case '(': parenCount++; break;
                case ')': parenCount--; break;
                case '[': bracketCount++; break;
                case ']': bracketCount--; break;
            }

            if (braceCount < 0)
            {
                errors.Add("閉じ波括弧 '}' が対応する開き波括弧 '{' より多くあります。");
                return;
            }

            if (parenCount < 0)
            {
                errors.Add("閉じ丸括弧 ')' が対応する開き丸括弧 '(' より多くあります。");
                return;
            }

            if (bracketCount < 0)
            {
                errors.Add("閉じ角括弧 ']' が対応する開き角括弧 '[' より多くあります。");
                return;
            }
        }

        if (braceCount != 0)
        {
            errors.Add($"波括弧 '{{' '}}' の対応が一致しません（差分: {braceCount}）。");
        }

        if (parenCount != 0)
        {
            errors.Add($"丸括弧 '(' ')' の対応が一致しません（差分: {parenCount}）。");
        }

        if (bracketCount != 0)
        {
            errors.Add($"角括弧 '[' ']' の対応が一致しません（差分: {bracketCount}）。");
        }
    }

    /// <summary>
    /// JavaScript のコメントと文字列リテラルを除去して括弧チェック用のテキストを返します
    /// </summary>
    private static string RemoveJsCommentsAndStrings(string content)
    {
        return JsCommentAndStringPattern().Replace(content, match =>
        {
            if (match.Value.StartsWith("//", StringComparison.Ordinal)
                || match.Value.StartsWith("/*", StringComparison.Ordinal))
            {
                return " ";
            }

            return new string(' ', match.Value.Length);
        });
    }

    /// <summary>
    /// CSS のコメントと文字列リテラルを除去して括弧チェック用のテキストを返します
    /// </summary>
    private static string RemoveCssCommentsAndStrings(string content)
    {
        return CssCommentAndStringPattern().Replace(content, match =>
        {
            return new string(' ', match.Value.Length);
        });
    }

    /// <summary>
    /// HTML タグの開閉バランスをチェックします（断片 HTML 対応）
    /// </summary>
    private static void CheckHtmlTagBalance(string content, List<string> errors)
    {
        var tagStack = new Stack<string>();
        var matches = HtmlTagPattern().Matches(content);

        foreach (Match match in matches)
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value.ToLowerInvariant();
            var isSelfClosing = match.Groups[3].Value == "/";

            if (IsVoidElement(tagName) || isSelfClosing)
            {
                continue;
            }

            if (isClosing)
            {
                if (tagStack.Count == 0)
                {
                    errors.Add($"閉じタグ '</{tagName}>' に対応する開きタグがありません。");
                    return;
                }

                var expected = tagStack.Pop();
                if (expected != tagName)
                {
                    errors.Add($"タグの対応が一致しません: '<{expected}>' に対して '</{tagName}>' が見つかりました。");
                    return;
                }
            }
            else
            {
                tagStack.Push(tagName);
            }
        }

        if (tagStack.Count > 0)
        {
            var unclosed = string.Join(", ", tagStack.Select(t => $"<{t}>"));
            errors.Add($"閉じられていないタグがあります: {unclosed}");
        }
    }

    private static bool IsVoidElement(string tagName)
    {
        return tagName is "area" or "base" or "br" or "col" or "embed"
            or "hr" or "img" or "input" or "link" or "meta"
            or "param" or "source" or "track" or "wbr";
    }

    private static ValidationResult CreateResult(
        ExtensionFileEntry entry, string? filePath, List<string> errors)
    {
        return new ValidationResult
        {
            ExtensionType = entry.ExtensionType,
            ExtensionName = entry.ExtensionName,
            FilePath = filePath,
            IsValid = errors.Count == 0,
            Errors = errors,
        };
    }

    /// <summary>
    /// JavaScript のコメント（// と /* */）および文字列リテラル（' " `）にマッチする正規表現
    /// </summary>
    /// <remarks>
    /// パターン構成:
    /// <list type="bullet">
    ///   <item><c>//.*?$</c> — 行コメント</item>
    ///   <item><c>/\*[\s\S]*?\*/</c> — ブロックコメント</item>
    ///   <item><c>"(?:[^"\\]|\\.)*"</c> — ダブルクォート文字列</item>
    ///   <item><c>'(?:[^'\\]|\\.)*'</c> — シングルクォート文字列</item>
    ///   <item><c>`(?:[^`\\]|\\.)*`</c> — テンプレートリテラル</item>
    /// </list>
    /// </remarks>
    [GeneratedRegex("""//.*?$|/\*[\s\S]*?\*/|"(?:[^"\\]|\\.)*"|'(?:[^'\\]|\\.)*'|`(?:[^`\\]|\\.)*`""", RegexOptions.Multiline)]
    private static partial Regex JsCommentAndStringPattern();

    /// <summary>
    /// CSS のコメント（/* */）および文字列リテラル（' "）にマッチする正規表現
    /// </summary>
    /// <remarks>
    /// パターン構成:
    /// <list type="bullet">
    ///   <item><c>/\*[\s\S]*?\*/</c> — ブロックコメント</item>
    ///   <item><c>"(?:[^"\\]|\\.)*"</c> — ダブルクォート文字列</item>
    ///   <item><c>'(?:[^'\\]|\\.)*'</c> — シングルクォート文字列</item>
    /// </list>
    /// </remarks>
    [GeneratedRegex("""/\*[\s\S]*?\*/|"(?:[^"\\]|\\.)*"|'(?:[^'\\]|\\.)*'""", RegexOptions.Multiline)]
    private static partial Regex CssCommentAndStringPattern();

    /// <summary>
    /// HTML タグにマッチする正規表現（開きタグ・閉じタグ・自己閉じタグ）
    /// </summary>
    /// <remarks>
    /// キャプチャグループ:
    /// <list type="bullet">
    ///   <item>グループ 1 — 閉じタグのスラッシュ（<c>/</c>）。閉じタグの場合に値を持つ。</item>
    ///   <item>グループ 2 — タグ名（<c>div</c>, <c>span</c> 等）</item>
    ///   <item>グループ 3 — 自己閉じのスラッシュ（<c>/</c>）。自己閉じタグの場合に値を持つ。</item>
    /// </list>
    /// </remarks>
    [GeneratedRegex("""<(/)?([a-zA-Z][a-zA-Z0-9]*)\b[^>]*?(/?)>""")]
    private static partial Regex HtmlTagPattern();
}
