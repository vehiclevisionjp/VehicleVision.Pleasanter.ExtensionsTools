using System.Text.Json.Serialization;

namespace ExtensionsSyncTool.Models;

/// <summary>
/// プリザンター API への一覧取得リクエスト
/// </summary>
public class ExtensionsGetRequest
{
    /// <summary>API バージョン</summary>
    [JsonPropertyName("ApiVersion")]
    public decimal ApiVersion { get; set; } = 1.1m;

    /// <summary>API キー</summary>
    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>オフセット（ページング用）</summary>
    [JsonPropertyName("Offset")]
    public int Offset { get; set; }

    /// <summary>1回の取得件数</summary>
    [JsonPropertyName("PageSize")]
    public int PageSize { get; set; } = 200;
}

/// <summary>
/// プリザンター API への作成・更新リクエスト
/// </summary>
public class ExtensionCreateUpdateRequest
{
    /// <summary>API バージョン</summary>
    [JsonPropertyName("ApiVersion")]
    public decimal ApiVersion { get; set; } = 1.1m;

    /// <summary>API キー</summary>
    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>拡張機能の種別</summary>
    [JsonPropertyName("ExtensionType")]
    public string ExtensionType { get; set; } = string.Empty;

    /// <summary>拡張機能名</summary>
    [JsonPropertyName("ExtensionName")]
    public string ExtensionName { get; set; } = string.Empty;

    /// <summary>拡張設定（JSON 文字列）</summary>
    [JsonPropertyName("ExtensionSettings")]
    public string? ExtensionSettings { get; set; }

    /// <summary>コンテンツ本体</summary>
    [JsonPropertyName("Body")]
    public string? Body { get; set; }

    /// <summary>説明</summary>
    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    /// <summary>無効フラグ</summary>
    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }
}

/// <summary>
/// プリザンター API の一覧取得レスポンス
/// </summary>
public class ExtensionsGetResponse
{
    /// <summary>ステータスコード</summary>
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    /// <summary>エラーメッセージ</summary>
    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    /// <summary>レスポンスデータ</summary>
    [JsonPropertyName("Response")]
    public ExtensionsResponseData? Response { get; set; }
}

/// <summary>
/// 一覧取得レスポンスの内部データ
/// </summary>
public class ExtensionsResponseData
{
    /// <summary>オフセット</summary>
    [JsonPropertyName("Offset")]
    public int Offset { get; set; }

    /// <summary>ページサイズ</summary>
    [JsonPropertyName("PageSize")]
    public int PageSize { get; set; }

    /// <summary>総件数</summary>
    [JsonPropertyName("TotalCount")]
    public int TotalCount { get; set; }

    /// <summary>レコードの一覧</summary>
    [JsonPropertyName("Data")]
    public List<ExtensionRecord> Data { get; set; } = [];
}

/// <summary>
/// プリザンター API の単一レコード操作レスポンス
/// </summary>
public class ExtensionSingleResponse
{
    /// <summary>ステータスコード</summary>
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    /// <summary>エラーメッセージ</summary>
    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    /// <summary>作成・更新されたレコード</summary>
    [JsonPropertyName("Response")]
    public ExtensionRecord? Response { get; set; }
}
