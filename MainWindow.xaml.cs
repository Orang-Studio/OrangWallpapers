using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace OrangWallpapers;
public partial class MainWindow : Window
{
    private static readonly HttpClient client = new();
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    private const int DWMWA_NCRENDERING_POLICY = 2;
    private const int DWMNCRP_DISABLED = 2;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_DONOTROUND = 1;
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(IntPtr hwnd, IntPtr region, bool redraw);
    private const int SM_CXFIXEDFRAME = 7;
    private const int SM_CXPADDEDBORDER = 92;
    [DllImport("user32.dll")]
    private static extern int GetSysColor(int index);
    [DllImport("user32.dll")]
    private static extern bool SetSysColors(int count, int[] elements, int[] colors);
    // COLOR_ACTIVECAPTION, COLOR_GRADIENTACTIVECAPTION, COLOR_INACTIVECAPTION, COLOR_GRADIENTINACTIVECAPTION, COLOR_ACTIVEBORDER, COLOR_INACTIVEBORDER
    private static readonly int[] captionColorIds = [2, 27, 3, 28, 10, 11];
    private static readonly int[] grayCaptionColors = [0x404040, 0xA0A0A0, 0x808080, 0xC0C0C0, 0x808080, 0x808080];
    private int[] originalCaptionColors = [];
    private const uint SWP_FRAMECHANGED_ONLY = 0x0001 | 0x0002 | 0x0004 | 0x0020;
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint SMTO_ABORTIFHUNG = 0x0002;
    private static readonly IntPtr HWND_BROADCAST = new(0xffff);
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private static readonly Dictionary<string, string> wallpapers = new()
    {
        { "Windows Default Wallpaper.", @"C:\Windows\Web\Wallpaper\Windows\img0.jpg" },
        { "Orange OS Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img0.png" },
        { "Windows 11 365 White Ver.", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img9989.jpg" },
        { "Windows Server 2025 White Ver.", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img1993.jpg" },
        { "Windows 11 365 Black Ver.", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img9990.jpg" },
        { "Windows Server 2025 Black Ver.", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img4783.jpg" },
        { "Dragon Ball Z Son Goku Island", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img6141.jpg" },
        { "CSGO AK 47 Gun Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img1945.jpg" },
        { "Blank - Best For Performance", "https://image-0.uhdpaper.com/wallpaper/windows-11-365-abstract-dark-background-digital-art-2k-wallpaper-uhdpaper.com-549@0@i.jpg" },
        { "Forest White Blue", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img2856.jpg" },
        { "Forest Purple Yellow", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img8348.jpg" },
        { "Windows XP Bliss", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img1119.jpg" },
        { "Simple Car Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img4598.jpg" },
        { "Windows 10 Classic Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img3106.jpg" },
        { "Huskey Dog Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img9577.jpg" },
        { "Windows SE Wallpaper", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img9991.jpg" },
        { "Windows 10 Beach But Minecrafted", "https://raw.githubusercontent.com/Orang-Studio/OrangWallpapers/main/wallpapers/img9992.jpg" }
    };
    private static readonly List<WallpaperCollection> collections =
    [
        new() { Name = "OrangWallpapers", BuiltIn = wallpapers.Select(w => new WallpaperEntry("All", w.Key, w.Value)).ToList() },
        new() { Name = "vyrx-dev Wallpapers", Repo = "vyrx-dev/Wallpapers", Branch = "master" },
        new() { Name = "dharmx Walls", Repo = "dharmx/walls" },
        new() { Name = "D3Ext Aesthetic Wallpapers", Repo = "D3Ext/aesthetic-wallpapers", ExcludedFolders = ["assets"] },
        new() { Name = "Catppuccin Wallpapers", Repo = "zhichaoh/catppuccin-wallpapers" }
    ];
    private static readonly Dictionary<string, (string Style, string Tile)> fitModes = new()
    {
        { "Fill", ("10", "0") },
        { "Fit", ("6", "0") },
        { "Stretch", ("2", "0") },
        { "Tile", ("0", "1") },
        { "Center", ("0", "0") },
        { "Span", ("22", "0") }
    };
    // (0, 0) keeps original size (-1, -1) uses primary screens resolution
    private static readonly Dictionary<string, (int Width, int Height)> resolutions = new()
    {
        { "Original", (0, 0) },
        { "Match Screen", (-1, -1) },
        { "3840 x 2160", (3840, 2160) },
        { "2560 x 1440", (2560, 1440) },
        { "1920 x 1080", (1920, 1080) },
        { "1600 x 900", (1600, 900) },
        { "1366 x 768", (1366, 768) },
        { "1280 x 720", (1280, 720) }
    };
    private readonly AppSettings settings = AppSettings.Load();
    private IReadOnlyList<WallpaperEntry> currentEntries = [];
    private int collectionLoad;
    public MainWindow()
    {
        InitializeComponent();
        client.DefaultRequestHeaders.Add("User-Agent", $"OrangWallpapers/{AppInfo.VERSION.TrimStart('v')}");
        FitComboBox.ItemsSource = fitModes.Keys;
        FitComboBox.SelectedItem = fitModes.ContainsKey(settings.FitMode) ? settings.FitMode : "Fill";
        ResolutionComboBox.ItemsSource = resolutions.Keys;
        ResolutionComboBox.SelectedItem = resolutions.ContainsKey(settings.Resolution) ? settings.Resolution : "Original";
        UpdateThemeRadioButtons();
        CollectionComboBox.ItemsSource = collections;
        CollectionComboBox.SelectedItem = collections.FirstOrDefault(c => c.Name == settings.Collection) ?? collections[0];
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // classic
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        int policy = DWMNCRP_DISABLED;
        int corners = DWMWCP_DONOTROUND;
        SetWindowTheme(hwnd, "", "");
        DwmSetWindowAttribute(hwnd, DWMWA_NCRENDERING_POLICY, ref policy, sizeof(int));
        DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corners, sizeof(int));
        originalCaptionColors = captionColorIds.Select(GetSysColor).ToArray();
        SetSysColors(captionColorIds.Length, captionColorIds, grayCaptionColors);
        Closed += (_, _) => SetSysColors(captionColorIds.Length, captionColorIds, originalCaptionColors);
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED_ONLY);
        ClipFrame();
        SizeChanged += (_, _) => ClipFrame();
    }
    // windslop still paints the full classic frame, and window region just cuts it down to its innermost 1px ig
    // I hate this shit cuz not fully documented, but love it 
    private void ClipFrame()
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        GetWindowRect(hwnd, out var rect);
        int cut = GetSystemMetrics(SM_CXFIXEDFRAME) + GetSystemMetrics(SM_CXPADDEDBORDER) - 1;
        var region = CreateRectRgn(cut, cut, rect.Right - rect.Left - cut, rect.Bottom - rect.Top - cut);
        SetWindowRgn(hwnd, region, true);
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }
    private void UpdateThemeRadioButtons()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(PersonalizeKey);
        bool isLight = (key?.GetValue("AppsUseLightTheme") as int?) == 1;
        ThemeLightRadio.IsChecked = isLight;
        ThemeDarkRadio.IsChecked = !isLight;
        TransparencyCheckBox.IsChecked = (key?.GetValue("EnableTransparency") as int? ?? 1) == 1;
    }
    private async void ChangeFromUrl_Click(object sender, RoutedEventArgs e)
    {
        if (Uri.TryCreate(UrlTextBox.Text.Trim(), UriKind.Absolute, out _))
            await ApplyWallpaper(UrlTextBox.Text.Trim());
        else
            StatusText.Text = "Invalid URL";
    }
    private async void SelectLocal_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*" };
        if (dlg.ShowDialog() == true)
            await ApplyWallpaper(dlg.FileName);
    }
    private async void Collection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CollectionComboBox.SelectedItem is not WallpaperCollection collection) return;
        int load = ++collectionLoad;
        // startup load Made by OrangStudio
        bool quiet = !IsLoaded;
        CategoryComboBox.ItemsSource = null;
        WallpaperComboBox.ItemsSource = null;
        if (!quiet)
            StatusText.Text = "Loading collection...";
        try
        {
            var entries = await WallpaperCatalog.LoadAsync(collection, client);
            if (load != collectionLoad) return;
            currentEntries = entries;
            settings.Collection = collection.Name;
            var categories = entries.Select(w => w.Category).Distinct().Order(StringComparer.OrdinalIgnoreCase).ToList();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.IsEnabled = categories.Count > 1;
            CategoryComboBox.SelectedItem = categories.FirstOrDefault(c => c == settings.Category) ?? categories.FirstOrDefault();
            if (!quiet)
                StatusText.Text = $"{entries.Count} wallpapers available";
        }
        catch (Exception ex) when (WallpaperCatalog.IsExpectedFailure(ex))
        {
            if (load == collectionLoad)
                StatusText.Text = "Failed to load collection: " + ex.Message;
        }
    }
    private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryComboBox.SelectedItem is not string category) return;
        settings.Category = category;
        settings.Save();
        WallpaperComboBox.ItemsSource = currentEntries.Where(w => w.Category == category).ToList();
    }
    private async void Wallpaper_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WallpaperComboBox.SelectedItem is WallpaperEntry entry)
            await ApplyWallpaper(entry.Url);
    }
    private async void Scaling_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        settings.FitMode = (string)FitComboBox.SelectedItem;
        settings.Resolution = (string)ResolutionComboBox.SelectedItem;
        settings.Save();
        if (settings.LastWallpaper != null)
            await ApplyWallpaper(settings.LastWallpaper);
    }
    private async Task ApplyWallpaper(string pathOrUrl)
    {
        StatusText.Text = "Loading...";
        try
        {
            bool isLocal = File.Exists(pathOrUrl);
            var path = isLocal ? pathOrUrl : await Download(pathOrUrl);
            var fit = (string)FitComboBox.SelectedItem;
            if (GetTargetResolution() is { } target)
            {
                StatusText.Text = "Scaling...";
                path = await Task.Run(() => ScaleImage(path, target.Width, target.Height, fit));
            }
            SetWallpaper(path, fit);
            settings.LastWallpaper = pathOrUrl;
            settings.Save();
            StatusText.Text = isLocal ? "Success (Local)" : "Success (Downloaded)";
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException or TaskCanceledException or UnauthorizedAccessException or NotSupportedException or FormatException or ExternalException)
        {
            StatusText.Text = "Failed: " + ex.Message;
        }
    }
    private static async Task<string> Download(string url)
    {
        Directory.CreateDirectory(AppPaths.Downloads);
        var ext = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".bmp"))
            ext = ".jpg";
        // same URL maps to same file
        var savePath = Path.Join(AppPaths.Downloads, Hash(url) + ext);
        if (!File.Exists(savePath))
        {
            var bytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(savePath + ".part", bytes);
            File.Move(savePath + ".part", savePath, true);
        }
        return savePath;
    }
    private (int Width, int Height)? GetTargetResolution()
    {
        var (width, height) = resolutions[(string)ResolutionComboBox.SelectedItem];
        if (width == 0) return null;
        if (width < 0) return (GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
        return (width, height);
    }
    private static string ScaleImage(string path, int width, int height, string fit)
    {
        int sourceWidth, sourceHeight;
        using (var stream = File.OpenRead(path))
        {
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None).Frames[0];
            sourceWidth = frame.PixelWidth;
            sourceHeight = frame.PixelHeight;
        }
        int scaledWidth = width, scaledHeight = height;
        if (fit != "Stretch")
        {
            double scaleX = (double)width / sourceWidth, scaleY = (double)height / sourceHeight;
            double scale = fit == "Fit" ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
            scaledWidth = Math.Max(1, (int)Math.Round(sourceWidth * scale));
            scaledHeight = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        }
        if (scaledWidth == sourceWidth && scaledHeight == sourceHeight)
            return path;
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(path);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = scaledWidth;
        image.DecodePixelHeight = scaledHeight;
        image.EndInit();
        bool isPng = Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase);
        BitmapEncoder encoder = isPng ? new PngBitmapEncoder() : new JpegBitmapEncoder { QualityLevel = 95 };
        encoder.Frames.Add(BitmapFrame.Create(image));
        // only the current scaled wallpaper is kept around
        Directory.CreateDirectory(AppPaths.Scaled);
        foreach (var old in Directory.GetFiles(AppPaths.Scaled))
        {
            try { File.Delete(old); } catch (IOException) { }
        }
        var outPath = Path.Join(AppPaths.Scaled, $"{Hash(path + File.GetLastWriteTimeUtc(path).Ticks)}_{scaledWidth}x{scaledHeight}{(isPng ? ".png" : ".jpg")}");
        using (var output = File.Create(outPath))
            encoder.Save(output);
        return outPath;
    }
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..16].ToLowerInvariant();
    private static void SetWallpaper(string path, string fit)
    {
        var (style, tile) = fitModes[fit];
        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
        {
            key?.SetValue("WallpaperStyle", style);
            key?.SetValue("TileWallpaper", tile);
        }
        if (SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE) == 0)
            throw new IOException("Windows could not use this image as a wallpaper");
    }
    private static void BroadcastThemeChange()
    {
        // tells explorer and other apps to reload theme
        Task.Run(() => SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "ImmersiveColorSet", SMTO_ABORTIFHUNG, 1000, out _));
    }
    private void Transparency_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        bool enabled = TransparencyCheckBox.IsChecked ?? false;
        using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(PersonalizeKey))
            key.SetValue("EnableTransparency", enabled ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
        BroadcastThemeChange();
        StatusText.Text = enabled ? "Transparency Enabled" : "Transparency Disabled";
    }
    private void Theme_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        bool isLight = ThemeLightRadio.IsChecked ?? false;
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(PersonalizeKey, true);
        if (key != null)
        {
            int val = isLight ? 1 : 0;
            key.SetValue("AppsUseLightTheme", val, Microsoft.Win32.RegistryValueKind.DWord);
            key.SetValue("SystemUsesLightTheme", val, Microsoft.Win32.RegistryValueKind.DWord);
            BroadcastThemeChange();
        }
        var res = Resources;
        res["WindowBackground"] = isLight ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(30,30,30));
        res["WindowText"] = isLight ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
        res["InputBackground"] = isLight ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(50,50,50));
        res["InputText"] = isLight ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
    }
}