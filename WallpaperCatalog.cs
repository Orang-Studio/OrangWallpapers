using System.IO;
using System.Net.Http;
using System.Text.Json;
namespace OrangWallpapers;
public sealed record WallpaperEntry(string Category, string Name, string Url)
{
    public override string ToString() => Name;
}
public sealed class WallpaperCollection
{
    public required string Name { get; init; }
    public string? Repo { get; init; }
    public string Branch { get; init; } = "main";
    public string[] ExcludedFolders { get; init; } = [];
    public IReadOnlyList<WallpaperEntry>? BuiltIn { get; init; }
    public override string ToString() => Name;
}
public static class WallpaperCatalog
{
    private static readonly string[] imageExtensions = [".jpg", ".jpeg", ".png"];
    private static readonly TimeSpan cacheLifetime = TimeSpan.FromDays(7);
    private sealed record CatalogCache(DateTime FetchedUtc, List<string> Paths);
    public static async Task<IReadOnlyList<WallpaperEntry>> LoadAsync(WallpaperCollection collection, HttpClient client)
    {
        if (collection.BuiltIn != null || collection.Repo == null)
            return collection.BuiltIn ?? [];
        var cachePath = Path.Join(AppPaths.Cache, collection.Repo.Replace('/', '_') + ".json");
        var cache = ReadCache(cachePath);
        if (cache == null || DateTime.UtcNow - cache.FetchedUtc > cacheLifetime)
        {
            try
            {
                cache = new CatalogCache(DateTime.UtcNow, await FetchImagePaths(collection, client));
                Directory.CreateDirectory(AppPaths.Cache);
                await File.WriteAllTextAsync(cachePath, JsonSerializer.Serialize(cache));
            }
            catch (Exception ex) when (cache != null && IsExpectedFailure(ex))
            {
                // offline or rate limited by GitHub
            }
        }
        return cache.Paths.Select(path => ToEntry(collection, path)).ToList();
    }
    public static bool IsExpectedFailure(Exception ex) =>
        ex is HttpRequestException or IOException or JsonException or KeyNotFoundException or InvalidOperationException or TaskCanceledException or UnauthorizedAccessException;
    private static async Task<List<string>> FetchImagePaths(WallpaperCollection collection, HttpClient client)
    {
        var json = await client.GetStringAsync($"https://api.github.com/repos/{collection.Repo}/git/trees/{collection.Branch}?recursive=1");
        using var doc = JsonDocument.Parse(json);
        var paths = new List<string>();
        foreach (var item in doc.RootElement.GetProperty("tree").EnumerateArray())
        {
            var path = item.GetProperty("path").GetString();
            if (path == null || item.GetProperty("type").GetString() != "blob")
                continue;
            if (!imageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                continue;
            if (collection.ExcludedFolders.Any(folder => path.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase)))
                continue;
            paths.Add(path);
        }
        return paths;
    }
    private static CatalogCache? ReadCache(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<CatalogCache>(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }
    private static WallpaperEntry ToEntry(WallpaperCollection collection, string path)
    {
        var slash = path.LastIndexOf('/');
        var category = slash > 0 ? path[..slash] : "Other";
        var name = Path.GetFileNameWithoutExtension(path).Replace('_', ' ');
        var escapedPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
        return new WallpaperEntry(category, name, $"https://raw.githubusercontent.com/{collection.Repo}/{collection.Branch}/{escapedPath}");
    }
}