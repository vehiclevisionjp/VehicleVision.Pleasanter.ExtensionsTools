namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

/// <summary>
/// サーバー（DB）とローカルファイルの比較結果を表すモデル
/// </summary>
public class DiffEntry
{
    /// <summary>拡張機能の種別</summary>
    public string ExtensionType { get; set; } = string.Empty;

    /// <summary>拡張機能名</summary>
    public string ExtensionName { get; set; } = string.Empty;

    /// <summary>比較ステータス</summary>
    public DiffStatus Status { get; set; }

    /// <summary>サーバー側の設定 JSON（サーバーにのみ存在する場合は null）</summary>
    public string? ServerSettings { get; set; }

    /// <summary>サーバー側のコンテンツ本体（サーバーにのみ存在する場合は null）</summary>
    public string? ServerBody { get; set; }

    /// <summary>ローカル側の設定 JSON（ローカルにのみ存在する場合は null）</summary>
    public string? LocalSettings { get; set; }

    /// <summary>ローカル側のコンテンツ本体（ローカルにのみ存在する場合は null）</summary>
    public string? LocalBody { get; set; }

    /// <summary>設定 JSON に差分があるかどうか</summary>
    public bool HasSettingsDiff { get; set; }

    /// <summary>コンテンツ本体に差分があるかどうか</summary>
    public bool HasBodyDiff { get; set; }
}

/// <summary>
/// サーバーとローカルの比較ステータス
/// </summary>
public enum DiffStatus
{
    /// <summary>サーバーとローカルが同一</summary>
    Unchanged,

    /// <summary>サーバーとローカルに差分がある</summary>
    Modified,

    /// <summary>サーバーにのみ存在する（ローカルにない）</summary>
    ServerOnly,

    /// <summary>ローカルにのみ存在する（サーバーにない）</summary>
    LocalOnly,
}
