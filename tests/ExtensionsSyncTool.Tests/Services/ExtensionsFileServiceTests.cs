using System.Text.Json;
using ExtensionsSyncTool.Models;
using ExtensionsSyncTool.Services;

namespace ExtensionsSyncTool.Tests.Services;

/// <summary>
/// ExtensionsFileService のテスト
/// </summary>
public class ExtensionsFileServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ExtensionsFileService _sut;

    public ExtensionsFileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ExtSyncTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _sut = new ExtensionsFileService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    // ---- ReadAllEntries ----

    [Fact]
    public void ReadAllEntriesShouldReturnEmptyWhenParametersFolderIsEmpty()
    {
        // Arrange（空の Parameters フォルダ）

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        Assert.Empty(entries);
    }

    [Fact]
    public void ReadAllEntriesShouldReadJsScript()
    {
        // Arrange
        var scriptsDir = Path.Combine(_tempDir, "ExtendedScripts");
        Directory.CreateDirectory(scriptsDir);
        File.WriteAllText(Path.Combine(scriptsDir, "my-script.js"), "console.log('hello');");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("Script", entry.ExtensionType);
        Assert.Equal("my-script", entry.ExtensionName);
        Assert.Equal("console.log('hello');", entry.Content);
        Assert.Null(entry.SettingsJson);
    }

    [Fact]
    public void ReadAllEntriesShouldReadCssStyle()
    {
        // Arrange
        var stylesDir = Path.Combine(_tempDir, "ExtendedStyles");
        Directory.CreateDirectory(stylesDir);
        File.WriteAllText(Path.Combine(stylesDir, "custom.css"), "body { color: red; }");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("Style", entry.ExtensionType);
        Assert.Equal("custom", entry.ExtensionName);
        Assert.Equal("body { color: red; }", entry.Content);
    }

    [Fact]
    public void ReadAllEntriesShouldReadHtml()
    {
        // Arrange
        var htmlsDir = Path.Combine(_tempDir, "ExtendedHtmls");
        Directory.CreateDirectory(htmlsDir);
        File.WriteAllText(Path.Combine(htmlsDir, "sidebar.html"), "<div>sidebar</div>");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("Html", entry.ExtensionType);
        Assert.Equal("sidebar", entry.ExtensionName);
        Assert.Equal("<div>sidebar</div>", entry.Content);
    }

    [Fact]
    public void ReadAllEntriesShouldReadServerScriptJsonOnly()
    {
        // Arrange
        var dir = Path.Combine(_tempDir, "ExtendedServerScripts");
        Directory.CreateDirectory(dir);
        var jsonContent = """
            {
                "Name": "my-ss",
                "Description": "test",
                "BeforeCreate": true,
                "Body": "context.Log('hi');"
            }
            """;
        File.WriteAllText(Path.Combine(dir, "my-ss.json"), jsonContent);

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("ServerScript", entry.ExtensionType);
        Assert.Equal("my-ss", entry.ExtensionName);
        Assert.Equal("context.Log('hi');", entry.Content);
        Assert.NotNull(entry.SettingsJson);

        var settings = JsonDocument.Parse(entry.SettingsJson!).RootElement;
        Assert.False(settings.TryGetProperty("Body", out _), "Body should be removed from settings JSON");
        Assert.True(settings.TryGetProperty("BeforeCreate", out var beforeCreate));
        Assert.True(beforeCreate.GetBoolean());
    }

    [Fact]
    public void ReadAllEntriesShouldReadServerScriptWithSeparateBodyFile()
    {
        // Arrange
        var dir = Path.Combine(_tempDir, "ExtendedServerScripts");
        Directory.CreateDirectory(dir);
        var jsonContent = """{"Name": "my-ss", "BeforeCreate": true}""";
        File.WriteAllText(Path.Combine(dir, "my-ss.json"), jsonContent);
        File.WriteAllText(Path.Combine(dir, "my-ss.json.js"), "// separate body");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("// separate body", entry.Content);
        Assert.NotNull(entry.ContentFilePath);
    }

    [Fact]
    public void ReadAllEntriesShouldReadSqlJsonWithSeparateSqlFile()
    {
        // Arrange
        var dir = Path.Combine(_tempDir, "ExtendedSqls");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "my-query.json"), """{"Name": "my-query"}""");
        File.WriteAllText(Path.Combine(dir, "my-query.json.sql"), "SELECT 1;");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("Sql", entry.ExtensionType);
        Assert.Equal("my-query", entry.ExtensionName);
        Assert.Equal("SELECT 1;", entry.Content);
    }

    [Fact]
    public void ReadAllEntriesShouldReadFieldsJson()
    {
        // Arrange
        var dir = Path.Combine(_tempDir, "ExtendedFields");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "my-field.json"), """{"Name": "my-field", "FieldType": "String"}""");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal("Fields", entry.ExtensionType);
        Assert.Equal("my-field", entry.ExtensionName);
        Assert.NotNull(entry.SettingsJson);
        Assert.Null(entry.Content);
    }

    [Fact]
    public void ReadAllEntriesShouldReadMultipleTypes()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_tempDir, "ExtendedScripts"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "ExtendedStyles"));
        File.WriteAllText(Path.Combine(_tempDir, "ExtendedScripts", "a.js"), "// js");
        File.WriteAllText(Path.Combine(_tempDir, "ExtendedStyles", "b.css"), "/* css */");

        // Act
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.ExtensionType == "Script");
        Assert.Contains(entries, e => e.ExtensionType == "Style");
    }

    // ---- WriteEntry ----

    [Fact]
    public void WriteEntryShouldWriteJsFile()
    {
        // Arrange
        var record = new ExtensionRecord
        {
            ExtensionType = "Script",
            ExtensionName = "my-script",
            Body = "console.log('test');",
        };

        // Act
        _sut.WriteEntry(_tempDir, record);

        // Assert
        var expectedPath = Path.Combine(_tempDir, "ExtendedScripts", "my-script.js");
        Assert.True(File.Exists(expectedPath));
        Assert.Equal("console.log('test');", File.ReadAllText(expectedPath));
    }

    [Fact]
    public void WriteEntryShouldWriteCssFile()
    {
        // Arrange
        var record = new ExtensionRecord
        {
            ExtensionType = "Style",
            ExtensionName = "custom",
            Body = "body { margin: 0; }",
        };

        // Act
        _sut.WriteEntry(_tempDir, record);

        // Assert
        var expectedPath = Path.Combine(_tempDir, "ExtendedStyles", "custom.css");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void WriteEntryShouldWriteServerScriptJsonAndBodyFiles()
    {
        // Arrange
        var settingsJson = """{"Name": "my-ss", "BeforeCreate": true}""";
        var record = new ExtensionRecord
        {
            ExtensionType = "ServerScript",
            ExtensionName = "my-ss",
            ExtensionSettings = settingsJson,
            Body = "// server script body",
        };

        // Act
        _sut.WriteEntry(_tempDir, record);

        // Assert
        var jsonPath = Path.Combine(_tempDir, "ExtendedServerScripts", "my-ss.json");
        var bodyPath = Path.Combine(_tempDir, "ExtendedServerScripts", "my-ss.json.js");
        Assert.True(File.Exists(jsonPath));
        Assert.True(File.Exists(bodyPath));
        Assert.Equal(settingsJson, File.ReadAllText(jsonPath));
        Assert.Equal("// server script body", File.ReadAllText(bodyPath));
    }

    [Fact]
    public void WriteEntryShouldWriteFieldsJsonFile()
    {
        // Arrange
        var settingsJson = """{"Name": "my-field", "FieldType": "String"}""";
        var record = new ExtensionRecord
        {
            ExtensionType = "Fields",
            ExtensionName = "my-field",
            ExtensionSettings = settingsJson,
        };

        // Act
        _sut.WriteEntry(_tempDir, record);

        // Assert
        var expectedPath = Path.Combine(_tempDir, "ExtendedFields", "my-field.json");
        Assert.True(File.Exists(expectedPath));
        Assert.Equal(settingsJson, File.ReadAllText(expectedPath));
    }

    [Fact]
    public void WriteEntryShouldThrowForUnsupportedExtensionType()
    {
        // Arrange
        var record = new ExtensionRecord
        {
            ExtensionType = "Unknown",
            ExtensionName = "test",
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _sut.WriteEntry(_tempDir, record));
    }

    // ---- ToCreateUpdateRequest ----

    [Fact]
    public void ToCreateUpdateRequestShouldMapAllFields()
    {
        // Arrange
        var entry = new ExtensionFileEntry
        {
            ExtensionType = "Script",
            ExtensionName = "my-script",
            Content = "console.log('hello');",
            SettingsJson = null,
        };

        // Act
        var request = _sut.ToCreateUpdateRequest(entry);

        // Assert
        Assert.Equal("Script", request.ExtensionType);
        Assert.Equal("my-script", request.ExtensionName);
        Assert.Equal("console.log('hello');", request.Body);
        Assert.Null(request.ExtensionSettings);
    }

    // ---- Round-trip ----

    [Fact]
    public void WriteAndReadShouldRoundTripScriptEntry()
    {
        // Arrange
        var original = new ExtensionRecord
        {
            ExtensionType = "Script",
            ExtensionName = "round-trip-script",
            Body = "/* round trip */",
        };

        // Act
        _sut.WriteEntry(_tempDir, original);
        var entries = _sut.ReadAllEntries(_tempDir);

        // Assert
        var entry = Assert.Single(entries);
        Assert.Equal(original.ExtensionType, entry.ExtensionType);
        Assert.Equal(original.ExtensionName, entry.ExtensionName);
        Assert.Equal(original.Body, entry.Content);
    }
}
