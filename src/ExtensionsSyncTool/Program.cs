using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ExtensionsSyncTool.Configuration;
using ExtensionsSyncTool.Services;

// 設定ファイルの読み込み
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("local.config.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "EXTENSIONS_SYNC_")
    .Build();

// --base-url / --api-key / --parameters-path オプション（コマンドラインが設定ファイルより優先）
var baseUrlOption = new Option<string?>("--base-url", "プリザンターサーバーのベース URL（例: https://pleasanter.example.com）");
baseUrlOption.AddAlias("-u");

var apiKeyOption = new Option<string?>("--api-key", "プリザンター API キー");
apiKeyOption.AddAlias("-k");

var parametersPathOption = new Option<string?>("--parameters-path", "ローカルの Parameters ディレクトリのパス");
parametersPathOption.AddAlias("-p");

var dryRunOption = new Option<bool>("--dry-run", "ドライラン（実際の書き込み・API 呼び出しを行わない）");
dryRunOption.AddAlias("-n");

// pull コマンド: DB → ファイル
var pullCommand = new Command("pull", "Extensions テーブルからローカルファイルへ同期します（DB → ファイル）");
pullCommand.AddOption(baseUrlOption);
pullCommand.AddOption(apiKeyOption);
pullCommand.AddOption(parametersPathOption);
pullCommand.AddOption(dryRunOption);

pullCommand.SetHandler(async (InvocationContext context) =>
{
    var parseResult = context.ParseResult;
    var settings = BuildSettings(parseResult, configuration, baseUrlOption, apiKeyOption, parametersPathOption);
    if (!ValidateSettings(settings))
    {
        context.ExitCode = 1;
        return;
    }

    var services = BuildServices(settings);
    var syncService = services.GetRequiredService<SyncService>();
    var dryRun = parseResult.GetValueForOption(dryRunOption);

    try
    {
        await syncService.PullAsync(dryRun, context.GetCancellationToken()).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"エラー: {ex.Message}");
        context.ExitCode = 1;
    }
});

// push コマンド: ファイル → DB
var pushCommand = new Command("push", "ローカルファイルから Extensions テーブルへ同期します（ファイル → DB）");
pushCommand.AddOption(baseUrlOption);
pushCommand.AddOption(apiKeyOption);
pushCommand.AddOption(parametersPathOption);
pushCommand.AddOption(dryRunOption);

pushCommand.SetHandler(async (InvocationContext context) =>
{
    var parseResult = context.ParseResult;
    var settings = BuildSettings(parseResult, configuration, baseUrlOption, apiKeyOption, parametersPathOption);
    if (!ValidateSettings(settings))
    {
        context.ExitCode = 1;
        return;
    }

    var services = BuildServices(settings);
    var syncService = services.GetRequiredService<SyncService>();
    var dryRun = parseResult.GetValueForOption(dryRunOption);

    try
    {
        await syncService.PushAsync(dryRun, context.GetCancellationToken()).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"エラー: {ex.Message}");
        context.ExitCode = 1;
    }
});

// ルートコマンド
var rootCommand = new RootCommand("プリザンター Extensions テーブルとローカルファイルを同期するツール");
rootCommand.AddCommand(pullCommand);
rootCommand.AddCommand(pushCommand);

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

// ---- ヘルパーメソッド ----

static AppSettings BuildSettings(
    ParseResult parseResult,
    IConfiguration configuration,
    Option<string?> baseUrlOption,
    Option<string?> apiKeyOption,
    Option<string?> parametersPathOption)
{
    return new AppSettings
    {
        BaseUrl = parseResult.GetValueForOption(baseUrlOption)
            ?? configuration["BaseUrl"]
            ?? string.Empty,
        ApiKey = parseResult.GetValueForOption(apiKeyOption)
            ?? configuration["ApiKey"]
            ?? string.Empty,
        ParametersPath = parseResult.GetValueForOption(parametersPathOption)
            ?? configuration["ParametersPath"]
            ?? string.Empty,
    };
}

static bool ValidateSettings(AppSettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        Console.Error.WriteLine("エラー: BaseUrl が指定されていません。--base-url オプションまたは appsettings.json の BaseUrl を設定してください。");
        return false;
    }

    if (string.IsNullOrWhiteSpace(settings.ApiKey))
    {
        Console.Error.WriteLine("エラー: ApiKey が指定されていません。--api-key オプションまたは appsettings.json の ApiKey を設定してください。");
        return false;
    }

    if (string.IsNullOrWhiteSpace(settings.ParametersPath))
    {
        Console.Error.WriteLine("エラー: ParametersPath が指定されていません。--parameters-path オプションまたは appsettings.json の ParametersPath を設定してください。");
        return false;
    }

    return true;
}

static ServiceProvider BuildServices(AppSettings settings)
{
    var services = new ServiceCollection();

    services.AddSingleton(settings);
    services.AddHttpClient<IPleasanterApiClient, PleasanterApiClient>();
    services.AddSingleton<IExtensionsFileService, ExtensionsFileService>();
    services.AddSingleton<SyncService>();

    return services.BuildServiceProvider();
}
