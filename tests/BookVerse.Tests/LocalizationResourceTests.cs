using System.Text.Json;

namespace BookVerse.Tests;

public sealed class LocalizationResourceTests
{
    [Fact]
    public void VietnameseAndEnglishResourcesHaveSameStringKeys()
    {
        var root = FindRepositoryRoot();
        var vi = ReadKeys(Path.Combine(root, "BookVerse", "Resources", "vi.json"));
        var en = ReadKeys(Path.Combine(root, "BookVerse", "Resources", "en.json"));
        Assert.Equal(vi.OrderBy(x => x), en.OrderBy(x => x));
    }

    private static HashSet<string> ReadKeys(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("strings").EnumerateObject().Select(x => x.Name).ToHashSet();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Final_Projects.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
