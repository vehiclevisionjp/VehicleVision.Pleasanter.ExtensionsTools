using System.Net.Http.Json;
using System.Text.Json;
using ExtensionsSyncTool.Configuration;
using ExtensionsSyncTool.Models;

namespace ExtensionsSyncTool.Services;

/// <summary>
/// プリザンター Extensions API クライアントの実装
/// </summary>
public class PleasanterApiClient : IPleasanterApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
    };

    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;

    /// <summary>
    /// PleasanterApiClient を初期化します
    /// </summary>
    /// <param name="httpClient">HTTP クライアント</param>
    /// <param name="settings">アプリケーション設定</param>
    public PleasanterApiClient(HttpClient httpClient, AppSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    /// <inheritdoc/>
    public async Task<List<ExtensionRecord>> GetAllExtensionsAsync(CancellationToken cancellationToken = default)
    {
        var allRecords = new List<ExtensionRecord>();
        var offset = 0;
        const int pageSize = 200;

        while (true)
        {
            var request = new ExtensionsGetRequest
            {
                ApiKey = _settings.ApiKey,
                Offset = offset,
                PageSize = pageSize,
            };

            var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/extensions/get";
            var response = await _httpClient
                .PostAsJsonAsync(url, request, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ExtensionsGetResponse>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (result?.StatusCode != 200)
            {
                throw new InvalidOperationException(
                    $"Extensions API 取得失敗: StatusCode={result?.StatusCode}, Message={result?.Message}");
            }

            var data = result.Response?.Data ?? [];
            allRecords.AddRange(data);

            if (data.Count < pageSize)
            {
                break;
            }

            offset += pageSize;
        }

        return allRecords;
    }

    /// <inheritdoc/>
    public async Task<ExtensionRecord?> CreateExtensionAsync(
        ExtensionCreateUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        request.ApiKey = _settings.ApiKey;

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/extensions/create";
        var response = await _httpClient
            .PostAsJsonAsync(url, request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ExtensionSingleResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (result?.StatusCode != 200)
        {
            throw new InvalidOperationException(
                $"Extensions 作成失敗: StatusCode={result?.StatusCode}, Message={result?.Message}");
        }

        return result.Response;
    }

    /// <inheritdoc/>
    public async Task<ExtensionRecord?> UpdateExtensionAsync(
        int extensionId,
        ExtensionCreateUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        request.ApiKey = _settings.ApiKey;

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/extensions/{extensionId}/update";
        var response = await _httpClient
            .PostAsJsonAsync(url, request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ExtensionSingleResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (result?.StatusCode != 200)
        {
            throw new InvalidOperationException(
                $"Extensions 更新失敗 (ID={extensionId}): StatusCode={result?.StatusCode}, Message={result?.Message}");
        }

        return result.Response;
    }

    /// <inheritdoc/>
    public async Task DeleteExtensionAsync(int extensionId, CancellationToken cancellationToken = default)
    {
        var request = new ExtensionsGetRequest
        {
            ApiKey = _settings.ApiKey,
        };

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/extensions/{extensionId}/delete";
        var response = await _httpClient
            .PostAsJsonAsync(url, request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ExtensionSingleResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (result?.StatusCode != 200)
        {
            throw new InvalidOperationException(
                $"Extensions 削除失敗 (ID={extensionId}): StatusCode={result?.StatusCode}, Message={result?.Message}");
        }
    }
}
