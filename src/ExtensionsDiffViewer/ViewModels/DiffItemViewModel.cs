using CommunityToolkit.Mvvm.ComponentModel;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.ViewModels;

/// <summary>
/// 差分一覧の各アイテムを表す ViewModel
/// </summary>
public partial class DiffItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _extensionType = string.Empty;

    [ObservableProperty]
    private string _extensionName = string.Empty;

    [ObservableProperty]
    private DiffStatus _status;

    [ObservableProperty]
    private string _serverSettings = string.Empty;

    [ObservableProperty]
    private string _serverBody = string.Empty;

    [ObservableProperty]
    private string _localSettings = string.Empty;

    [ObservableProperty]
    private string _localBody = string.Empty;

    [ObservableProperty]
    private bool _hasSettingsDiff;

    [ObservableProperty]
    private bool _hasBodyDiff;

    /// <summary>
    /// 表示用のステータスラベル
    /// </summary>
    public string StatusLabel => Status switch
    {
        DiffStatus.Unchanged => "一致",
        DiffStatus.Modified => "変更あり",
        DiffStatus.ServerOnly => "サーバーのみ",
        DiffStatus.LocalOnly => "ローカルのみ",
        _ => "不明",
    };

    /// <summary>
    /// ステータスに応じた色名
    /// </summary>
    public string StatusColor => Status switch
    {
        DiffStatus.Unchanged => "#4CAF50",
        DiffStatus.Modified => "#FF9800",
        DiffStatus.ServerOnly => "#2196F3",
        DiffStatus.LocalOnly => "#9C27B0",
        _ => "#757575",
    };

    /// <summary>
    /// 表示用のタイトル
    /// </summary>
    public string DisplayTitle => $"[{ExtensionType}] {ExtensionName}";

    /// <summary>
    /// DiffEntry モデルから ViewModel を生成します
    /// </summary>
    public static DiffItemViewModel FromDiffEntry(DiffEntry entry)
    {
        return new DiffItemViewModel
        {
            ExtensionType = entry.ExtensionType,
            ExtensionName = entry.ExtensionName,
            Status = entry.Status,
            ServerSettings = entry.ServerSettings ?? string.Empty,
            ServerBody = entry.ServerBody ?? string.Empty,
            LocalSettings = entry.LocalSettings ?? string.Empty,
            LocalBody = entry.LocalBody ?? string.Empty,
            HasSettingsDiff = entry.HasSettingsDiff,
            HasBodyDiff = entry.HasBodyDiff,
        };
    }
}
