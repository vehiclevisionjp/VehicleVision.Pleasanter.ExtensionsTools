using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;

namespace VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

/// <summary>
/// 拡張機能ファイルのコンテンツバリデーションを行うインターフェース
/// </summary>
public interface IContentValidator
{
    /// <summary>
    /// 拡張機能エントリの一覧に対してバリデーションを実行します
    /// </summary>
    /// <param name="entries">バリデーション対象のエントリ一覧</param>
    /// <returns>バリデーション結果の一覧</returns>
    IReadOnlyList<ValidationResult> ValidateAll(IReadOnlyList<ExtensionFileEntry> entries);

    /// <summary>
    /// 単一の拡張機能エントリに対してバリデーションを実行します
    /// </summary>
    /// <param name="entry">バリデーション対象のエントリ</param>
    /// <returns>バリデーション結果の一覧（設定JSONとコンテンツそれぞれに対する結果）</returns>
    IReadOnlyList<ValidationResult> Validate(ExtensionFileEntry entry);
}
