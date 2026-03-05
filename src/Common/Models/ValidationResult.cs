namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

/// <summary>
/// コンテンツバリデーションの結果を表すモデル
/// </summary>
public class ValidationResult
{
    /// <summary>バリデーション対象の拡張機能種別</summary>
    public string ExtensionType { get; set; } = string.Empty;

    /// <summary>バリデーション対象の拡張機能名</summary>
    public string ExtensionName { get; set; } = string.Empty;

    /// <summary>対象ファイルのパス</summary>
    public string? FilePath { get; set; }

    /// <summary>バリデーションが成功したかどうか</summary>
    public bool IsValid { get; set; }

    /// <summary>エラーメッセージの一覧</summary>
    public IReadOnlyList<string> Errors { get; set; } = [];
}
