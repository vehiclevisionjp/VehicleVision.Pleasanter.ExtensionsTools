using VehicleVision.Pleasanter.ExtensionsTools.Common.Configuration;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// Extensions テーブルとローカルファイルの双方向同期サービス
/// </summary>
public class SyncService
{
    private readonly IPleasanterApiClient _apiClient;
    private readonly IExtensionsFileService _fileService;
    private readonly AppSettings _settings;

    /// <summary>
    /// SyncService を初期化します
    /// </summary>
    /// <param name="apiClient">Pleasanter API クライアント</param>
    /// <param name="fileService">ファイルサービス</param>
    /// <param name="settings">アプリケーション設定</param>
    public SyncService(
        IPleasanterApiClient apiClient,
        IExtensionsFileService fileService,
        AppSettings settings)
    {
        _apiClient = apiClient;
        _fileService = fileService;
        _settings = settings;
    }

    /// <summary>
    /// DB（Extensions テーブル）→ ローカルファイルへ同期します（pull）
    /// </summary>
    /// <param name="dryRun">true の場合、実際のファイル書き込みを行わずログのみ出力します</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>同期されたレコード数</returns>
    public async Task<int> PullAsync(bool dryRun = false, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("DB → ファイルへの同期を開始します...");

        var records = await _apiClient.GetAllExtensionsAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"  取得レコード数: {records.Count}");

        var count = 0;
        foreach (var record in records)
        {
            Console.WriteLine($"  [{record.ExtensionType}] {record.ExtensionName}");

            if (!dryRun)
            {
                _fileService.WriteEntry(_settings.ParametersPath, record);
            }

            count++;
        }

        if (dryRun)
        {
            Console.WriteLine($"（ドライラン: ファイル書き込みをスキップしました）");
        }

        Console.WriteLine($"完了: {count} 件を同期しました。");
        return count;
    }

    /// <summary>
    /// ローカルファイル → DB（Extensions テーブル）へ同期します（push）
    /// </summary>
    /// <param name="dryRun">true の場合、実際の API 呼び出しを行わずログのみ出力します</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>同期されたレコード数</returns>
    public async Task<int> PushAsync(bool dryRun = false, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("ファイル → DB への同期を開始します...");

        var fileEntries = _fileService.ReadAllEntries(_settings.ParametersPath);
        Console.WriteLine($"  ローカルファイル件数: {fileEntries.Count}");

        Dictionary<(string, string), ExtensionRecord> existingByKey = [];

        if (!dryRun)
        {
            var existingRecords = await _apiClient.GetAllExtensionsAsync(cancellationToken).ConfigureAwait(false);
            existingByKey = existingRecords.ToDictionary(
                r => (r.ExtensionType, r.ExtensionName),
                r => r);
        }

        var count = 0;
        foreach (var entry in fileEntries)
        {
            var request = _fileService.ToCreateUpdateRequest(entry);

            if (existingByKey.TryGetValue((entry.ExtensionType, entry.ExtensionName), out var existing)
                && existing.ExtensionId.HasValue)
            {
                Console.WriteLine($"  [更新] [{entry.ExtensionType}] {entry.ExtensionName} (ID={existing.ExtensionId})");

                if (!dryRun)
                {
                    await _apiClient.UpdateExtensionAsync(
                        existing.ExtensionId.Value, request, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                Console.WriteLine($"  [作成] [{entry.ExtensionType}] {entry.ExtensionName}");

                if (!dryRun)
                {
                    await _apiClient.CreateExtensionAsync(request, cancellationToken).ConfigureAwait(false);
                }
            }

            count++;
        }

        if (dryRun)
        {
            Console.WriteLine($"（ドライラン: API 呼び出しをスキップしました）");
        }

        Console.WriteLine($"完了: {count} 件を同期しました。");
        return count;
    }
}
