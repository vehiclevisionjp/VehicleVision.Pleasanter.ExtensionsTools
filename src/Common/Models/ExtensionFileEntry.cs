namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

/// <summary>
/// ローカルファイルシステム上の拡張機能エントリを表すモデル
/// </summary>
public class ExtensionFileEntry
{
    /// <summary>拡張機能の種別</summary>
    public string ExtensionType { get; set; } = string.Empty;

    /// <summary>拡張機能名（ファイル名のベース部分）</summary>
    public string ExtensionName { get; set; } = string.Empty;

    /// <summary>設定ファイル（JSON）の絶対パス（存在する場合）</summary>
    public string? SettingsFilePath { get; set; }

    /// <summary>コンテンツファイルの絶対パス（存在する場合）</summary>
    public string? ContentFilePath { get; set; }

    /// <summary>設定 JSON の内容</summary>
    public string? SettingsJson { get; set; }

    /// <summary>コンテンツ（スクリプト・SQL・HTML 等）の内容</summary>
    public string? Content { get; set; }
}
