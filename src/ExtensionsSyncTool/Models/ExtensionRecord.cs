using System.Text.Json.Serialization;

namespace ExtensionsSyncTool.Models;

/// <summary>
/// プリザンター Extensions テーブルのレコードを表すモデル
/// </summary>
public class ExtensionRecord
{
    /// <summary>ExtensionId（DB の主キー）</summary>
    [JsonPropertyName("ExtensionId")]
    public int? ExtensionId { get; set; }

    /// <summary>テナントID</summary>
    [JsonPropertyName("TenantId")]
    public int? TenantId { get; set; }

    /// <summary>バージョン</summary>
    [JsonPropertyName("Ver")]
    public int? Ver { get; set; }

    /// <summary>
    /// 拡張機能の種別。
    /// 有効値: Script, Style, Html, ServerScript, Sql, Fields, NavigationMenu, Plugin
    /// </summary>
    [JsonPropertyName("ExtensionType")]
    public string ExtensionType { get; set; } = string.Empty;

    /// <summary>拡張機能名（ファイル名のベース部分に対応）</summary>
    [JsonPropertyName("ExtensionName")]
    public string ExtensionName { get; set; } = string.Empty;

    /// <summary>拡張設定（JSON 文字列）</summary>
    [JsonPropertyName("ExtensionSettings")]
    public string? ExtensionSettings { get; set; }

    /// <summary>コンテンツ本体（スクリプト・SQL・HTML 等）</summary>
    [JsonPropertyName("Body")]
    public string? Body { get; set; }

    /// <summary>説明</summary>
    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    /// <summary>無効フラグ</summary>
    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    /// <summary>作成日時</summary>
    [JsonPropertyName("CreatedTime")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>更新日時</summary>
    [JsonPropertyName("UpdatedTime")]
    public DateTime? UpdatedTime { get; set; }
}
