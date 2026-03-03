using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// プリザンター Extensions API クライアントのインターフェース
/// </summary>
public interface IPleasanterApiClient
{
    /// <summary>
    /// Extensions テーブルの全レコードを取得します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>Extensions レコードの一覧</returns>
    Task<List<ExtensionRecord>> GetAllExtensionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 拡張機能レコードを新規作成します
    /// </summary>
    /// <param name="request">作成リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>作成されたレコード</returns>
    Task<ExtensionRecord?> CreateExtensionAsync(ExtensionCreateUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 既存の拡張機能レコードを更新します
    /// </summary>
    /// <param name="extensionId">更新対象の ExtensionId</param>
    /// <param name="request">更新リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたレコード</returns>
    Task<ExtensionRecord?> UpdateExtensionAsync(int extensionId, ExtensionCreateUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 拡張機能レコードを削除します
    /// </summary>
    /// <param name="extensionId">削除対象の ExtensionId</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task DeleteExtensionAsync(int extensionId, CancellationToken cancellationToken = default);
}
