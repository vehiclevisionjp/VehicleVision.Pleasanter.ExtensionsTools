using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

namespace VehicleVision.Pleasanter.ExtensionsTools.Tests.Services;

/// <summary>
/// ContentValidator のテスト
/// </summary>
public class ContentValidatorTests
{
    private readonly ContentValidator _sut = new();

    // ---- JSON バリデーション ----

    [Fact]
    public void ValidateShouldPassForValidJson()
    {
        // Arrange
        var entry = CreateEntry("Fields", settingsJson: """{"Name": "test", "Value": 123}""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateShouldFailForInvalidJson()
    {
        // Arrange
        var entry = CreateEntry("Fields", settingsJson: """{"Name": "test",}""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JSON 構文エラー"));
    }

    [Fact]
    public void ValidateShouldFailForEmptyJson()
    {
        // Arrange
        var entry = CreateEntry("Fields", settingsJson: "   ");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JSON が空です"));
    }

    [Fact]
    public void ValidateShouldFailForJsonWithTrailingComma()
    {
        // Arrange
        var entry = CreateEntry("NavigationMenu", settingsJson: """[1, 2, 3,]""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForJsonWithComments()
    {
        // Arrange
        var entry = CreateEntry("Plugin", settingsJson: """
            {
                // This is a comment
                "Name": "test"
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForJsonArray()
    {
        // Arrange
        var entry = CreateEntry("Fields", settingsJson: """[{"Name": "a"}, {"Name": "b"}]""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldReportLineNumberForJsonError()
    {
        // Arrange
        var entry = CreateEntry("Fields", settingsJson: """
            {
                "Name": "test"
                "Value": 123
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains('行'));
    }

    // ---- JavaScript バリデーション ----

    [Fact]
    public void ValidateShouldPassForValidJavaScript()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            function hello() {
                console.log('hello');
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnbalancedBracesInJs()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            function hello() {
                console.log('hello');
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("波括弧"));
    }

    [Fact]
    public void ValidateShouldFailForUnbalancedParensInJs()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """console.log('hello'""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("丸括弧"));
    }

    [Fact]
    public void ValidateShouldPassForJsWithBracketsInStrings()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            var x = "hello { world }";
            var y = 'test ( value )';
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForJsWithBracketsInComments()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            // This is a comment with {
            /* Another comment with ( */
            function test() {
                return 1;
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForEmptyJsContent()
    {
        // Arrange
        var entry = CreateEntry("Script", content: "   ");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForServerScriptWithJsonAndJs()
    {
        // Arrange
        var entry = CreateEntry(
            "ServerScript",
            settingsJson: """{"Name": "my-ss", "BeforeCreate": true}""",
            content: """
                context.Log('hello');
                if (true) {
                    context.Log('world');
                }
                """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsValid));
    }

    [Fact]
    public void ValidateShouldFailForExtraClosingBraceInJs()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            function hello() {
                return 1;
            }}
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("閉じ波括弧"));
    }

    [Fact]
    public void ValidateShouldPassForJsWithTemplateLiterals()
    {
        // Arrange
        var entry = CreateEntry("Script", content: """
            var msg = `Hello ${name}`;
            function test() {
                return msg;
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    // ---- CSS バリデーション ----

    [Fact]
    public void ValidateShouldPassForValidCss()
    {
        // Arrange
        var entry = CreateEntry("Style", content: """
            body {
                color: red;
                margin: 0;
            }
            .header {
                font-size: 14px;
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnbalancedBracesInCss()
    {
        // Arrange
        var entry = CreateEntry("Style", content: """
            body {
                color: red;

            .header {
                font-size: 14px;
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("波括弧"));
    }

    [Fact]
    public void ValidateShouldPassForCssWithComments()
    {
        // Arrange
        var entry = CreateEntry("Style", content: """
            /* This comment has { unbalanced braces */
            body {
                color: red;
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForEmptyCssContent()
    {
        // Arrange
        var entry = CreateEntry("Style", content: "   ");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForCssWithMediaQuery()
    {
        // Arrange
        var entry = CreateEntry("Style", content: """
            @media (max-width: 768px) {
                body {
                    font-size: 12px;
                }
            }
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    // ---- HTML バリデーション ----

    [Fact]
    public void ValidateShouldPassForValidHtmlFragment()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """<div class="sidebar"><p>Hello</p></div>""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForHtmlFragmentWithoutHtmlTag()
    {
        // Arrange — 拡張 HTML は <html> で始まらない断片
        var entry = CreateEntry("Html", content: """
            <div id="custom-area">
                <h2>見出し</h2>
                <p>本文テキスト</p>
                <img src="image.png" />
            </div>
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnclosedHtmlTag()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """<div><p>Hello</div>""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("タグの対応が一致しません"));
    }

    [Fact]
    public void ValidateShouldFailForExtraClosingTag()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """<div>Hello</div></p>""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("対応する開きタグがありません"));
    }

    [Fact]
    public void ValidateShouldPassForHtmlWithVoidElements()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """
            <div>
                <br>
                <hr>
                <img src="test.png">
                <input type="text">
            </div>
            """);

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForHtmlWithSelfClosingTags()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """<div><br /><img src="x" /></div>""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForMissingClosingTag()
    {
        // Arrange
        var entry = CreateEntry("Html", content: """<div><span>Hello</div>""");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForEmptyHtmlContent()
    {
        // Arrange
        var entry = CreateEntry("Html", content: "   ");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    // ---- ValidateAll ----

    [Fact]
    public void ValidateAllShouldReturnResultsForAllEntries()
    {
        // Arrange
        var entries = new List<ExtensionFileEntry>
        {
            CreateEntry("Script", content: "console.log('hello');"),
            CreateEntry("Style", content: "body { color: red; }"),
            CreateEntry("Fields", settingsJson: """{"Name": "test"}"""),
        };

        // Act
        var results = _sut.ValidateAll(entries);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.IsValid));
    }

    [Fact]
    public void ValidateAllShouldIncludeErrorResults()
    {
        // Arrange
        var entries = new List<ExtensionFileEntry>
        {
            CreateEntry("Script", content: "function() {"),
            CreateEntry("Fields", settingsJson: """{"Name": }"""),
        };

        // Act
        var results = _sut.ValidateAll(entries);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.False(r.IsValid));
    }

    // ---- SQL は構文チェックなし ----

    [Fact]
    public void ValidateShouldOnlyValidateJsonForSqlType()
    {
        // Arrange
        var entry = CreateEntry(
            "Sql",
            settingsJson: """{"Name": "my-query"}""",
            content: "SELECT * FROM invalid syntax {{{{");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
        Assert.Equal("Sql", result.ExtensionType);
    }

    // ---- ヘルパー ----

    private static ExtensionFileEntry CreateEntry(
        string extensionType,
        string? settingsJson = null,
        string? content = null)
    {
        return new ExtensionFileEntry
        {
            ExtensionType = extensionType,
            ExtensionName = $"test-{extensionType.ToLowerInvariant()}",
            SettingsJson = settingsJson,
            Content = content,
        };
    }
}
