using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// サーバー（DB）とローカルファイルの差分比較サービスの実装
/// </summary>
public class DiffService : IDiffService
{
    /// <inheritdoc/>
    public IReadOnlyList<DiffEntry> ComputeDiff(
        IReadOnlyList<ExtensionRecord> serverRecords,
        IReadOnlyList<ExtensionFileEntry> localEntries)
    {
        ArgumentNullException.ThrowIfNull(serverRecords);
        ArgumentNullException.ThrowIfNull(localEntries);

        var results = new List<DiffEntry>();

        var serverByKey = serverRecords.ToDictionary(
            r => (r.ExtensionType, r.ExtensionName),
            r => r);

        var localByKey = localEntries.ToDictionary(
            e => (e.ExtensionType, e.ExtensionName),
            e => e);

        // サーバー側のレコードを処理
        foreach (var record in serverRecords)
        {
            var key = (record.ExtensionType, record.ExtensionName);

            if (localByKey.TryGetValue(key, out var localEntry))
            {
                // 両方に存在する → 比較
                var settingsDiff = !NormalizeAndCompare(
                    record.ExtensionSettings, localEntry.SettingsJson);
                var bodyDiff = !NormalizeAndCompare(
                    record.Body, localEntry.Content);

                results.Add(new DiffEntry
                {
                    ExtensionType = record.ExtensionType,
                    ExtensionName = record.ExtensionName,
                    Status = settingsDiff || bodyDiff ? DiffStatus.Modified : DiffStatus.Unchanged,
                    ServerSettings = record.ExtensionSettings,
                    ServerBody = record.Body,
                    LocalSettings = localEntry.SettingsJson,
                    LocalBody = localEntry.Content,
                    HasSettingsDiff = settingsDiff,
                    HasBodyDiff = bodyDiff,
                });
            }
            else
            {
                // サーバーにのみ存在
                results.Add(new DiffEntry
                {
                    ExtensionType = record.ExtensionType,
                    ExtensionName = record.ExtensionName,
                    Status = DiffStatus.ServerOnly,
                    ServerSettings = record.ExtensionSettings,
                    ServerBody = record.Body,
                    HasSettingsDiff = !string.IsNullOrEmpty(record.ExtensionSettings),
                    HasBodyDiff = !string.IsNullOrEmpty(record.Body),
                });
            }
        }

        // ローカルにのみ存在するエントリ
        foreach (var entry in localEntries)
        {
            var key = (entry.ExtensionType, entry.ExtensionName);
            if (!serverByKey.ContainsKey(key))
            {
                results.Add(new DiffEntry
                {
                    ExtensionType = entry.ExtensionType,
                    ExtensionName = entry.ExtensionName,
                    Status = DiffStatus.LocalOnly,
                    LocalSettings = entry.SettingsJson,
                    LocalBody = entry.Content,
                    HasSettingsDiff = !string.IsNullOrEmpty(entry.SettingsJson),
                    HasBodyDiff = !string.IsNullOrEmpty(entry.Content),
                });
            }
        }

        return results
            .OrderBy(d => d.ExtensionType)
            .ThenBy(d => d.ExtensionName)
            .ToList();
    }

    /// <summary>
    /// 2 つの文字列を正規化して比較します（null・空文字の統一、末尾改行の除去）
    /// </summary>
    private static bool NormalizeAndCompare(string? a, string? b)
    {
        var normA = Normalize(a);
        var normB = Normalize(b);
        return string.Equals(normA, normB, StringComparison.Ordinal);
    }

    /// <summary>
    /// 文字列を正規化します（null → 空文字、末尾空白の除去）
    /// </summary>
    private static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.TrimEnd();
    }
}
