using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Configuration;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

namespace VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.ViewModels;

/// <summary>
/// メインウィンドウの ViewModel
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IPleasanterApiClient _apiClient;
    private readonly IExtensionsFileService _fileService;
    private readonly IDiffService _diffService;
    private readonly SyncService _syncService;
    private readonly AppSettings _settings;

    [ObservableProperty]
    private string _baseUrl = string.Empty;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _parametersPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DiffItemViewModel> _diffItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServerContentText))]
    [NotifyPropertyChangedFor(nameof(LocalContentText))]
    [NotifyPropertyChangedFor(nameof(HasSelectedItem))]
    [NotifyPropertyChangedFor(nameof(CanPullSelected))]
    [NotifyPropertyChangedFor(nameof(CanPushSelected))]
    private DiffItemViewModel? _selectedItem;

    [ObservableProperty]
    private string _statusMessage = "設定を入力して「比較」ボタンを押してください。";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _unchangedCount;

    [ObservableProperty]
    private int _modifiedCount;

    [ObservableProperty]
    private int _serverOnlyCount;

    [ObservableProperty]
    private int _localOnlyCount;

    /// <summary>
    /// 選択アイテムのサーバー側テキスト（設定 + 本体）
    /// </summary>
    public string ServerContentText => SelectedItem is null
        ? string.Empty
        : FormatContent(SelectedItem.ServerSettings, SelectedItem.ServerBody);

    /// <summary>
    /// 選択アイテムのローカル側テキスト（設定 + 本体）
    /// </summary>
    public string LocalContentText => SelectedItem is null
        ? string.Empty
        : FormatContent(SelectedItem.LocalSettings, SelectedItem.LocalBody);

    /// <summary>
    /// アイテムが選択されているかどうか
    /// </summary>
    public bool HasSelectedItem => SelectedItem is not null;

    /// <summary>
    /// 選択アイテムを pull できるかどうか（サーバーにデータがある場合）
    /// </summary>
    public bool CanPullSelected => SelectedItem is not null
        && SelectedItem.Status is not DiffStatus.LocalOnly;

    /// <summary>
    /// 選択アイテムを push できるかどうか（ローカルにデータがある場合）
    /// </summary>
    public bool CanPushSelected => SelectedItem is not null
        && SelectedItem.Status is not DiffStatus.ServerOnly;

    public MainWindowViewModel(
        IPleasanterApiClient apiClient,
        IExtensionsFileService fileService,
        IDiffService diffService,
        SyncService syncService,
        AppSettings settings)
    {
        _apiClient = apiClient;
        _fileService = fileService;
        _diffService = diffService;
        _syncService = syncService;
        _settings = settings;

        BaseUrl = settings.BaseUrl;
        ApiKey = settings.ApiKey;
        ParametersPath = settings.ParametersPath;
    }

    /// <summary>
    /// サーバーとローカルの比較を実行します
    /// </summary>
    [RelayCommand]
    private async Task CompareAsync()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl)
            || string.IsNullOrWhiteSpace(ApiKey)
            || string.IsNullOrWhiteSpace(ParametersPath))
        {
            StatusMessage = "エラー: BaseUrl、ApiKey、ParametersPath をすべて入力してください。";
            return;
        }

        IsLoading = true;
        StatusMessage = "比較中...";

        try
        {
            ApplySettings();

            var serverRecords = await _apiClient.GetAllExtensionsAsync().ConfigureAwait(false);
            var localEntries = _fileService.ReadAllEntries(ParametersPath);
            var diffs = _diffService.ComputeDiff(serverRecords, localEntries);

            DiffItems = new ObservableCollection<DiffItemViewModel>(
                diffs.Select(DiffItemViewModel.FromDiffEntry));

            UnchangedCount = diffs.Count(d => d.Status == DiffStatus.Unchanged);
            ModifiedCount = diffs.Count(d => d.Status == DiffStatus.Modified);
            ServerOnlyCount = diffs.Count(d => d.Status == DiffStatus.ServerOnly);
            LocalOnlyCount = diffs.Count(d => d.Status == DiffStatus.LocalOnly);

            StatusMessage = $"比較完了: 合計 {diffs.Count} 件（一致: {UnchangedCount}, 変更: {ModifiedCount}, サーバーのみ: {ServerOnlyCount}, ローカルのみ: {LocalOnlyCount}）";
        }
        catch (Exception ex)
        {
            StatusMessage = $"エラー: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 選択アイテムのサーバーデータをローカルに pull します
    /// </summary>
    [RelayCommand]
    private async Task PullSelectedAsync()
    {
        if (SelectedItem is null || !CanPullSelected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Pull 中: [{SelectedItem.ExtensionType}] {SelectedItem.ExtensionName}...";

        try
        {
            ApplySettings();

            var record = new ExtensionRecord
            {
                ExtensionType = SelectedItem.ExtensionType,
                ExtensionName = SelectedItem.ExtensionName,
                ExtensionSettings = SelectedItem.ServerSettings,
                Body = SelectedItem.ServerBody,
            };

            _fileService.WriteEntry(ParametersPath, record);

            StatusMessage = $"Pull 完了: [{SelectedItem.ExtensionType}] {SelectedItem.ExtensionName}";

            await CompareAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Pull エラー: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 選択アイテムのローカルデータをサーバーに push します
    /// </summary>
    [RelayCommand]
    private async Task PushSelectedAsync()
    {
        if (SelectedItem is null || !CanPushSelected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Push 中: [{SelectedItem.ExtensionType}] {SelectedItem.ExtensionName}...";

        try
        {
            ApplySettings();

            var entry = new ExtensionFileEntry
            {
                ExtensionType = SelectedItem.ExtensionType,
                ExtensionName = SelectedItem.ExtensionName,
                SettingsJson = SelectedItem.LocalSettings,
                Content = SelectedItem.LocalBody,
            };

            var request = _fileService.ToCreateUpdateRequest(entry);

            // 既存レコードの検索
            var existingRecords = await _apiClient.GetAllExtensionsAsync().ConfigureAwait(false);
            var existing = existingRecords.FirstOrDefault(
                r => r.ExtensionType == entry.ExtensionType
                    && r.ExtensionName == entry.ExtensionName);

            if (existing?.ExtensionId is not null)
            {
                await _apiClient.UpdateExtensionAsync(existing.ExtensionId.Value, request)
                    .ConfigureAwait(false);
            }
            else
            {
                await _apiClient.CreateExtensionAsync(request).ConfigureAwait(false);
            }

            StatusMessage = $"Push 完了: [{SelectedItem.ExtensionType}] {SelectedItem.ExtensionName}";

            await CompareAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Push エラー: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// すべてのサーバーデータをローカルに pull します
    /// </summary>
    [RelayCommand]
    private async Task PullAllAsync()
    {
        IsLoading = true;
        StatusMessage = "全件 Pull 中...";

        try
        {
            ApplySettings();
            var count = await _syncService.PullAsync(false).ConfigureAwait(false);
            StatusMessage = $"全件 Pull 完了: {count} 件を同期しました。";
            await CompareAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Pull エラー: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// すべてのローカルデータをサーバーに push します
    /// </summary>
    [RelayCommand]
    private async Task PushAllAsync()
    {
        IsLoading = true;
        StatusMessage = "全件 Push 中...";

        try
        {
            ApplySettings();
            var count = await _syncService.PushAsync(false).ConfigureAwait(false);
            StatusMessage = $"全件 Push 完了: {count} 件を同期しました。";
            await CompareAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Push エラー: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplySettings()
    {
        _settings.BaseUrl = BaseUrl;
        _settings.ApiKey = ApiKey;
        _settings.ParametersPath = ParametersPath;
    }

    private static string FormatContent(string settings, string body)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(settings))
        {
            parts.Add($"=== Settings (JSON) ===\n{settings}");
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            parts.Add($"=== Body ===\n{body}");
        }

        return parts.Count > 0 ? string.Join("\n\n", parts) : "(なし)";
    }
}
