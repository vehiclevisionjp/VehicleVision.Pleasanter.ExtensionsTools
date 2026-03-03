namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Configuration;

/// <summary>
/// アプリケーション設定
/// </summary>
public class AppSettings
{
    /// <summary>プリザンターサーバーのベース URL（例: https://pleasanter.example.com）</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>プリザンター API キー</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>ローカルの Parameters ディレクトリのパス</summary>
    public string ParametersPath { get; set; } = string.Empty;
}
