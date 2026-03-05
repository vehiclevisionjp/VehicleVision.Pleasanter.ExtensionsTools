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

    // ---- SQL バリデーション（RDBMS 未指定時） ----

    [Fact]
    public void ValidateShouldOnlyValidateJsonForSqlTypeWhenNoRdbms()
    {
        // Arrange — RDBMS 未指定のため SQL コンテンツはチェックしない
        var entry = CreateEntry(
            "Sql",
            settingsJson: """{"Name": "my-query"}""",
            content: "SELECT * FROM invalid syntax ((((");

        // Act
        var results = _sut.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
        Assert.Equal("Sql", result.ExtensionType);
    }

    // ---- SQL バリデーション（全 RDBMS 共通） ----

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForValidSelectStatement(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Id = 1;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForSqlWithSubquery(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Admins);");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldFailForUnbalancedParensInSql(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Id IN (1, 2, 3;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("丸括弧"));
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldFailForExtraClosingParenInSql(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT COUNT(*) FROM Users);");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("閉じ丸括弧"));
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForSqlWithLineComment(RdbmsType rdbmsType)
    {
        // Arrange — 行コメント内の括弧は無視される
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: """
            -- This comment has ( unbalanced paren
            SELECT * FROM Users;
            """);

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForSqlWithBlockComment(RdbmsType rdbmsType)
    {
        // Arrange — ブロックコメント内の括弧は無視される
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: """
            /* This comment has (( unbalanced parens */
            SELECT COUNT(*) FROM Users;
            """);

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldFailForUnclosedBlockComment(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "/* unclosed comment SELECT * FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ブロックコメント"));
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForSqlWithStringLiteral(RdbmsType rdbmsType)
    {
        // Arrange — 文字列内の括弧は無視される
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Name = 'hello ( world';");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForSqlWithEscapedQuote(RdbmsType rdbmsType)
    {
        // Arrange — '' でエスケープされたシングルクォートを正しく処理
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Name = 'it''s a test';");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldFailForUnclosedStringLiteral(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users WHERE Name = 'unclosed");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("文字列リテラル"));
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldPassForEmptySqlContent(RdbmsType rdbmsType)
    {
        // Arrange
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry("Sql", content: "   ");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(RdbmsType.SqlServer)]
    [InlineData(RdbmsType.MySql)]
    [InlineData(RdbmsType.PostgreSql)]
    public void ValidateShouldValidateBothJsonAndSqlForSqlType(RdbmsType rdbmsType)
    {
        // Arrange — JSON 設定 + SQL コンテンツ両方をバリデーション
        var validator = new ContentValidator(rdbmsType);
        var entry = CreateEntry(
            "Sql",
            settingsJson: """{"Name": "my-query"}""",
            content: "SELECT COUNT(*) FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsValid));
    }

    // ---- SQL Server 固有 ----

    [Fact]
    public void ValidateShouldPassForSqlServerBracketIdentifier()
    {
        // Arrange — [identifier] を正しくスキップ
        var validator = new ContentValidator(RdbmsType.SqlServer);
        var entry = CreateEntry("Sql", content: "SELECT [User Name] FROM [dbo].[Users];");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnclosedSqlServerBracketIdentifier()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.SqlServer);
        var entry = CreateEntry("Sql", content: "SELECT [User Name FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("角括弧識別子"));
    }

    [Fact]
    public void ValidateShouldPassForSqlServerTopQuery()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.SqlServer);
        var entry = CreateEntry("Sql", content: "SELECT TOP(10) * FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    // ---- MySQL 固有 ----

    [Fact]
    public void ValidateShouldPassForMySqlBacktickIdentifier()
    {
        // Arrange — `identifier` を正しくスキップ
        var validator = new ContentValidator(RdbmsType.MySql);
        var entry = CreateEntry("Sql", content: "SELECT `User Name` FROM `Users`;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnclosedMySqlBacktickIdentifier()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.MySql);
        var entry = CreateEntry("Sql", content: "SELECT `User Name FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("バッククォート識別子"));
    }

    [Fact]
    public void ValidateShouldPassForMySqlHashComment()
    {
        // Arrange — MySQL の # コメント内の括弧は無視
        var validator = new ContentValidator(RdbmsType.MySql);
        var entry = CreateEntry("Sql", content: """
            # This comment has ( unbalanced paren
            SELECT * FROM Users;
            """);

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForMySqlLimitQuery()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.MySql);
        var entry = CreateEntry("Sql", content: "SELECT * FROM Users LIMIT 10;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    // ---- PostgreSQL 固有 ----

    [Fact]
    public void ValidateShouldPassForPostgreSqlDoubleQuoteIdentifier()
    {
        // Arrange — "identifier" を正しくスキップ
        var validator = new ContentValidator(RdbmsType.PostgreSql);
        var entry = CreateEntry("Sql", content: """SELECT "User Name" FROM "Users";""");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldFailForUnclosedPostgreSqlDoubleQuoteIdentifier()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.PostgreSql);
        var entry = CreateEntry("Sql", content: """SELECT "User Name FROM Users;""");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ダブルクォート識別子"));
    }

    [Fact]
    public void ValidateShouldPassForPostgreSqlTypeCast()
    {
        // Arrange
        var validator = new ContentValidator(RdbmsType.PostgreSql);
        var entry = CreateEntry("Sql", content: "SELECT '123'::integer FROM Users;");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateShouldPassForPostgreSqlEscapedDoubleQuoteIdentifier()
    {
        // Arrange — PostgreSQL では "" で識別子内のダブルクォートをエスケープ
        var validator = new ContentValidator(RdbmsType.PostgreSql);
        var entry = CreateEntry("Sql", content: """SELECT "User""Name" FROM "Users";""");

        // Act
        var results = validator.Validate(entry);

        // Assert
        var result = Assert.Single(results);
        Assert.True(result.IsValid);
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
