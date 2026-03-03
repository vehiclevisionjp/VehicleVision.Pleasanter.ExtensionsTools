using System.Text.Json;
using System.Text.Json.Nodes;
using ExtensionsSyncTool.Models;

namespace ExtensionsSyncTool.Services;

/// <summary>
/// ローカルの Parameters フォルダに対するファイル操作の実装
/// </summary>
/// <remarks>
/// Parameters フォルダの拡張機能ファイル構成と Extensions テーブルのカラムの対応:
///
/// | ExtensionType  | フォルダ                  | ファイル形式                          |
/// |----------------|---------------------------|---------------------------------------|
/// | Script         | ExtendedScripts/          | *.js（コンテンツのみ）                |
/// | Style          | ExtendedStyles/           | *.css（コンテンツのみ）               |
/// | Html           | ExtendedHtmls/            | *.html（コンテンツのみ）              |
/// | ServerScript   | ExtendedServerScripts/    | *.json + *.json.js（任意）            |
/// | Sql            | ExtendedSqls/             | *.json + *.json.sql（任意）           |
/// | Fields         | ExtendedFields/           | *.json（設定のみ）                    |
/// | NavigationMenu | ExtendedNavigationMenus/  | *.json（設定のみ）                    |
/// | Plugin         | ExtendedPlugins/          | *.json（設定のみ）                    |
/// </remarks>
public class ExtensionsFileService : IExtensionsFileService
{
    private static readonly JsonSerializerOptions WriteJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    /// <summary>ExtensionType と Parameters フォルダ名のマッピング</summary>
    private static readonly Dictionary<string, ExtensionFolderDefinition> FolderDefinitions =
        new Dictionary<string, ExtensionFolderDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["Script"] = new("ExtendedScripts", ContentFileType.JsOnly, "js"),
            ["Style"] = new("ExtendedStyles", ContentFileType.CssOnly, "css"),
            ["Html"] = new("ExtendedHtmls", ContentFileType.HtmlOnly, "html"),
            ["ServerScript"] = new("ExtendedServerScripts", ContentFileType.JsonPlusJs, "js"),
            ["Sql"] = new("ExtendedSqls", ContentFileType.JsonPlusSql, "sql"),
            ["Fields"] = new("ExtendedFields", ContentFileType.JsonOnly, null),
            ["NavigationMenu"] = new("ExtendedNavigationMenus", ContentFileType.JsonOnly, null),
            ["Plugin"] = new("ExtendedPlugins", ContentFileType.JsonOnly, null),
        };

    /// <inheritdoc/>
    public IReadOnlyList<ExtensionFileEntry> ReadAllEntries(string parametersPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parametersPath);

        var entries = new List<ExtensionFileEntry>();

        foreach (var (extensionType, definition) in FolderDefinitions)
        {
            var folderPath = Path.Combine(parametersPath, definition.FolderName);
            if (!Directory.Exists(folderPath))
            {
                continue;
            }

            var folderEntries = definition.ContentType switch
            {
                ContentFileType.JsOnly => ReadContentOnlyEntries(folderPath, extensionType, "*.js"),
                ContentFileType.CssOnly => ReadContentOnlyEntries(folderPath, extensionType, "*.css"),
                ContentFileType.HtmlOnly => ReadContentOnlyEntries(folderPath, extensionType, "*.html"),
                ContentFileType.JsonPlusJs => ReadJsonPlusContentEntries(folderPath, extensionType, ".js"),
                ContentFileType.JsonPlusSql => ReadJsonPlusContentEntries(folderPath, extensionType, ".sql"),
                ContentFileType.JsonOnly => ReadJsonOnlyEntries(folderPath, extensionType),
                _ => [],
            };

            entries.AddRange(folderEntries);
        }

        return entries;
    }

    /// <inheritdoc/>
    public void WriteEntry(string parametersPath, ExtensionRecord record)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parametersPath);
        ArgumentNullException.ThrowIfNull(record);

        if (!FolderDefinitions.TryGetValue(record.ExtensionType, out var definition))
        {
            throw new NotSupportedException($"未対応の ExtensionType: {record.ExtensionType}");
        }

        var folderPath = Path.Combine(parametersPath, definition.FolderName);
        Directory.CreateDirectory(folderPath);

        var name = SanitizeFileName(record.ExtensionName);

        switch (definition.ContentType)
        {
            case ContentFileType.JsOnly:
                File.WriteAllText(Path.Combine(folderPath, $"{name}.js"), record.Body ?? string.Empty);
                break;

            case ContentFileType.CssOnly:
                File.WriteAllText(Path.Combine(folderPath, $"{name}.css"), record.Body ?? string.Empty);
                break;

            case ContentFileType.HtmlOnly:
                File.WriteAllText(Path.Combine(folderPath, $"{name}.html"), record.Body ?? string.Empty);
                break;

            case ContentFileType.JsonPlusJs:
                WriteJsonPlusContent(folderPath, name, record, ".json.js");
                break;

            case ContentFileType.JsonPlusSql:
                WriteJsonPlusContent(folderPath, name, record, ".json.sql");
                break;

            case ContentFileType.JsonOnly:
                WriteJsonOnly(folderPath, name, record);
                break;
        }
    }

    /// <inheritdoc/>
    public ExtensionCreateUpdateRequest ToCreateUpdateRequest(ExtensionFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new ExtensionCreateUpdateRequest
        {
            ExtensionType = entry.ExtensionType,
            ExtensionName = entry.ExtensionName,
            ExtensionSettings = entry.SettingsJson,
            Body = entry.Content,
        };
    }

    private static IEnumerable<ExtensionFileEntry> ReadContentOnlyEntries(
        string folderPath, string extensionType, string searchPattern)
    {
        foreach (var file in Directory.GetFiles(folderPath, searchPattern).OrderBy(f => f))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return new ExtensionFileEntry
            {
                ExtensionType = extensionType,
                ExtensionName = name,
                ContentFilePath = file,
                Content = File.ReadAllText(file),
            };
        }
    }

    private static IEnumerable<ExtensionFileEntry> ReadJsonPlusContentEntries(
        string folderPath, string extensionType, string contentExtension)
    {
        foreach (var jsonFile in Directory.GetFiles(folderPath, "*.json").OrderBy(f => f))
        {
            var rawJson = File.ReadAllText(jsonFile);
            var node = JsonNode.Parse(rawJson);
            var settingsJson = rawJson;
            string? content = null;

            var contentFile = jsonFile + contentExtension;
            if (File.Exists(contentFile))
            {
                content = File.ReadAllText(contentFile);
                settingsJson = RemoveBodyFromJson(node, extensionType);
            }
            else
            {
                content = node?["Body"]?.GetValue<string>() ?? node?["CommandText"]?.GetValue<string>();
                settingsJson = RemoveBodyFromJson(node, extensionType);
            }

            var name = Path.GetFileNameWithoutExtension(jsonFile);
            yield return new ExtensionFileEntry
            {
                ExtensionType = extensionType,
                ExtensionName = name,
                SettingsFilePath = jsonFile,
                ContentFilePath = File.Exists(contentFile) ? contentFile : null,
                SettingsJson = settingsJson,
                Content = content,
            };
        }
    }

    private static IEnumerable<ExtensionFileEntry> ReadJsonOnlyEntries(
        string folderPath, string extensionType)
    {
        foreach (var jsonFile in Directory.GetFiles(folderPath, "*.json").OrderBy(f => f))
        {
            var settingsJson = File.ReadAllText(jsonFile);
            var name = Path.GetFileNameWithoutExtension(jsonFile);
            yield return new ExtensionFileEntry
            {
                ExtensionType = extensionType,
                ExtensionName = name,
                SettingsFilePath = jsonFile,
                SettingsJson = settingsJson,
            };
        }
    }

    private static void WriteJsonPlusContent(
        string folderPath, string name, ExtensionRecord record, string contentExtension)
    {
        var jsonPath = Path.Combine(folderPath, $"{name}.json");
        var contentPath = Path.Combine(folderPath, $"{name}{contentExtension}");

        var settingsJson = record.ExtensionSettings ?? "{}";
        File.WriteAllText(jsonPath, settingsJson);

        if (!string.IsNullOrEmpty(record.Body))
        {
            File.WriteAllText(contentPath, record.Body);
        }
    }

    private static void WriteJsonOnly(string folderPath, string name, ExtensionRecord record)
    {
        var jsonPath = Path.Combine(folderPath, $"{name}.json");
        var settingsJson = record.ExtensionSettings ?? "{}";
        File.WriteAllText(jsonPath, settingsJson);
    }

    private static string RemoveBodyFromJson(JsonNode? node, string extensionType)
    {
        if (node is not JsonObject obj)
        {
            return "{}";
        }

        var clone = JsonNode.Parse(obj.ToJsonString())?.AsObject();
        if (clone is null)
        {
            return "{}";
        }

        clone.Remove("Body");
        clone.Remove("CommandText");

        return clone.ToJsonString(WriteJsonOptions);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private sealed record ExtensionFolderDefinition(
        string FolderName,
        ContentFileType ContentType,
        string? ContentExtension);

    private enum ContentFileType
    {
        JsOnly,
        CssOnly,
        HtmlOnly,
        JsonPlusJs,
        JsonPlusSql,
        JsonOnly,
    }
}
