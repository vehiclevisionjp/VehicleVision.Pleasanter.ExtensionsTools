using ExtensionsSyncTool.Models;

namespace ExtensionsSyncTool.Services;

/// <summary>
/// ローカルの Parameters フォルダに対するファイル操作のインターフェース
/// </summary>
public interface IExtensionsFileService
{
    /// <summary>
    /// Parameters フォルダ配下の拡張機能ファイルをすべて読み込みます
    /// </summary>
    /// <param name="parametersPath">Parameters ディレクトリのパス</param>
    /// <returns>拡張機能エントリの一覧</returns>
    IReadOnlyList<ExtensionFileEntry> ReadAllEntries(string parametersPath);

    /// <summary>
    /// 拡張機能レコードを Parameters フォルダへ書き込みます
    /// </summary>
    /// <param name="parametersPath">Parameters ディレクトリのパス</param>
    /// <param name="record">書き込むレコード</param>
    void WriteEntry(string parametersPath, ExtensionRecord record);

    /// <summary>
    /// 拡張機能エントリを DB レコードへ変換します
    /// </summary>
    /// <param name="entry">変換元エントリ</param>
    /// <returns>変換されたレコード</returns>
    ExtensionCreateUpdateRequest ToCreateUpdateRequest(ExtensionFileEntry entry);
}
