using System.IO;
using System.Text.Json;
namespace OrangWallpapers;
public static class AppPaths
{
    // everything app writes lives under %APPDATA%\OrangStudio\OrangWallpapers because it's global path for all apps we write btw. In linux it's just in .local
    public static readonly string Root = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OrangStudio", "OrangWallpapers");
    public static readonly string Downloads = Path.Join(Root, "wallpapers");
    public static readonly string Scaled = Path.Join(Root, "scaled");
    public static readonly string Cache = Path.Join(Root, "cache");
    public static readonly string Settings = Path.Join(Root, "settings.json");
}
public sealed class AppSettings
{
    private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
    public string? Collection { get; set; }
    public string? Category { get; set; }
    public string FitMode { get; set; } = "Fill";
    public string Resolution { get; set; } = "Original";
    public string? LastWallpaper { get; set; }
    public static AppSettings Load()
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.Settings)) ?? new();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new();
        }
    }
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppPaths.Root);
            File.WriteAllText(AppPaths.Settings, JsonSerializer.Serialize(this, jsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}