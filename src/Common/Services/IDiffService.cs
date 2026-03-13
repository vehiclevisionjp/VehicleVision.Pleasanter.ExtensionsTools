using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// サーバー（DB）とローカルファイルの差分比較サービスのインターフェース
/// </summary>
public interface IDiffService
{
    /// <summary>
    /// サーバーのレコードとローカルのファイルエントリを比較し、差分一覧を返します
    /// </summary>
    /// <param name="serverRecords">サーバーから取得した Extensions レコード</param>
    /// <param name="localEntries">ローカルファイルから読み込んだエントリ</param>
    /// <returns>差分エントリの一覧</returns>
    IReadOnlyList<DiffEntry> ComputeDiff(
        IReadOnlyList<ExtensionRecord> serverRecords,
        IReadOnlyList<ExtensionFileEntry> localEntries);
}
