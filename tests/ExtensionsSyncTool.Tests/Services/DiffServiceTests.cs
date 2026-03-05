using VehicleVision.Pleasanter.ExtensionsTools.Common.Models;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Services;

namespace VehicleVision.Pleasanter.ExtensionsTools.Tests.Services;

public class DiffServiceTests
{
    private readonly DiffService _sut = new();

    [Fact]
    public void ComputeDiffShouldReturnEmptyWhenBothEmpty()
    {
        var result = _sut.ComputeDiff([], []);
        Assert.Empty(result);
    }

    [Fact]
    public void ComputeDiffShouldReturnUnchangedWhenSameContent()
    {
        var server = new List<ExtensionRecord>
        {
            new()
            {
                ExtensionType = "Script",
                ExtensionName = "test",
                Body = "console.log('hello');",
            },
        };

        var local = new List<ExtensionFileEntry>
        {
            new()
            {
                ExtensionType = "Script",
                ExtensionName = "test",
                Content = "console.log('hello');",
            },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.Unchanged, result[0].Status);
        Assert.False(result[0].HasBodyDiff);
        Assert.False(result[0].HasSettingsDiff);
    }

    [Fact]
    public void ComputeDiffShouldReturnModifiedWhenBodyDiffers()
    {
        var server = new List<ExtensionRecord>
        {
            new()
            {
                ExtensionType = "Script",
                ExtensionName = "test",
                Body = "console.log('old');",
            },
        };

        var local = new List<ExtensionFileEntry>
        {
            new()
            {
                ExtensionType = "Script",
                ExtensionName = "test",
                Content = "console.log('new');",
            },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.Modified, result[0].Status);
        Assert.True(result[0].HasBodyDiff);
    }

    [Fact]
    public void ComputeDiffShouldReturnModifiedWhenSettingsDiffers()
    {
        var server = new List<ExtensionRecord>
        {
            new()
            {
                ExtensionType = "ServerScript",
                ExtensionName = "test",
                ExtensionSettings = """{"SiteId": 100}""",
                Body = "function(){}",
            },
        };

        var local = new List<ExtensionFileEntry>
        {
            new()
            {
                ExtensionType = "ServerScript",
                ExtensionName = "test",
                SettingsJson = """{"SiteId": 200}""",
                Content = "function(){}",
            },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.Modified, result[0].Status);
        Assert.True(result[0].HasSettingsDiff);
        Assert.False(result[0].HasBodyDiff);
    }

    [Fact]
    public void ComputeDiffShouldReturnServerOnlyForRecordNotInLocal()
    {
        var server = new List<ExtensionRecord>
        {
            new()
            {
                ExtensionType = "Script",
                ExtensionName = "serverOnly",
                Body = "alert('hi');",
            },
        };

        var result = _sut.ComputeDiff(server, []);

        Assert.Single(result);
        Assert.Equal(DiffStatus.ServerOnly, result[0].Status);
        Assert.Equal("alert('hi');", result[0].ServerBody);
        Assert.Null(result[0].LocalBody);
    }

    [Fact]
    public void ComputeDiffShouldReturnLocalOnlyForEntryNotInServer()
    {
        var local = new List<ExtensionFileEntry>
        {
            new()
            {
                ExtensionType = "Style",
                ExtensionName = "localOnly",
                Content = "body { color: red; }",
            },
        };

        var result = _sut.ComputeDiff([], local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.LocalOnly, result[0].Status);
        Assert.Null(result[0].ServerBody);
        Assert.Equal("body { color: red; }", result[0].LocalBody);
    }

    [Fact]
    public void ComputeDiffShouldHandleMixedStatuses()
    {
        var server = new List<ExtensionRecord>
        {
            new() { ExtensionType = "Script", ExtensionName = "both", Body = "same" },
            new() { ExtensionType = "Script", ExtensionName = "serverOnly", Body = "x" },
            new() { ExtensionType = "Style", ExtensionName = "modified", Body = "old" },
        };

        var local = new List<ExtensionFileEntry>
        {
            new() { ExtensionType = "Script", ExtensionName = "both", Content = "same" },
            new() { ExtensionType = "Html", ExtensionName = "localOnly", Content = "<p>hi</p>" },
            new() { ExtensionType = "Style", ExtensionName = "modified", Content = "new" },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Equal(4, result.Count);

        var unchanged = result.First(r => r.ExtensionName == "both");
        Assert.Equal(DiffStatus.Unchanged, unchanged.Status);

        var serverOnly = result.First(r => r.ExtensionName == "serverOnly");
        Assert.Equal(DiffStatus.ServerOnly, serverOnly.Status);

        var localOnly = result.First(r => r.ExtensionName == "localOnly");
        Assert.Equal(DiffStatus.LocalOnly, localOnly.Status);

        var modified = result.First(r => r.ExtensionName == "modified");
        Assert.Equal(DiffStatus.Modified, modified.Status);
    }

    [Fact]
    public void ComputeDiffShouldTreatNullAndEmptyAsEqual()
    {
        var server = new List<ExtensionRecord>
        {
            new() { ExtensionType = "Script", ExtensionName = "test", Body = null },
        };

        var local = new List<ExtensionFileEntry>
        {
            new() { ExtensionType = "Script", ExtensionName = "test", Content = "" },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.Unchanged, result[0].Status);
    }

    [Fact]
    public void ComputeDiffShouldIgnoreTrailingWhitespace()
    {
        var server = new List<ExtensionRecord>
        {
            new() { ExtensionType = "Script", ExtensionName = "test", Body = "code" },
        };

        var local = new List<ExtensionFileEntry>
        {
            new() { ExtensionType = "Script", ExtensionName = "test", Content = "code\n" },
        };

        var result = _sut.ComputeDiff(server, local);

        Assert.Single(result);
        Assert.Equal(DiffStatus.Unchanged, result[0].Status);
    }

    [Fact]
    public void ComputeDiffShouldReturnResultsSortedByTypeAndName()
    {
        var server = new List<ExtensionRecord>
        {
            new() { ExtensionType = "Style", ExtensionName = "zzz", Body = "a" },
            new() { ExtensionType = "Script", ExtensionName = "bbb", Body = "b" },
            new() { ExtensionType = "Script", ExtensionName = "aaa", Body = "c" },
        };

        var result = _sut.ComputeDiff(server, []);

        Assert.Equal(3, result.Count);
        Assert.Equal("Script", result[0].ExtensionType);
        Assert.Equal("aaa", result[0].ExtensionName);
        Assert.Equal("Script", result[1].ExtensionType);
        Assert.Equal("bbb", result[1].ExtensionName);
        Assert.Equal("Style", result[2].ExtensionType);
        Assert.Equal("zzz", result[2].ExtensionName);
    }
}
