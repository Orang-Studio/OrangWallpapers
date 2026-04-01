using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace OrangWallpapers;
public partial class MainWindow : Window
{
    private static readonly HttpClient client = new();
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;
    private readonly Dictionary<string, string> wallpapers = new()
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
    public MainWindow()
    {
        InitializeComponent();
        client.DefaultRequestHeaders.Add("User-Agent", "OrangWallpapers/5.0");
        PredefinedComboBox.ItemsSource = wallpapers.Keys;
        UpdateThemeRadioButtons();
    }
    private void UpdateThemeRadioButtons()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        if (key != null)
        {
            var appsUseLight = key.GetValue("AppsUseLightTheme") as int?;
            if (appsUseLight == 1) ThemeLightRadio.IsChecked = true;
            else ThemeDarkRadio.IsChecked = true;
        }
        else ThemeDarkRadio.IsChecked = true;
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
    private async void PredefinedCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PredefinedComboBox.SelectedItem is string key && wallpapers.TryGetValue(key, out string? url))
            await ApplyWallpaper(url);
    }
    private async Task ApplyWallpaper(string pathOrUrl)
    {
        StatusText.Text = "Loading...";
        try
        {
            if (File.Exists(pathOrUrl))
            {
                SetWallpaper(pathOrUrl);
                StatusText.Text = "Success (Local)";
            }
            else
            {
                var saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InterJava-Programs", "wallpaperchng");
                Directory.CreateDirectory(saveDir);
                
                var ext = pathOrUrl.Contains('.') ? pathOrUrl.Split('.').Last() : "jpg";
                var filename = $"img{Random.Shared.Next(1000, 9999)}.{ext}";
                var savePath = Path.Combine(saveDir, filename);

                var bytes = await client.GetByteArrayAsync(pathOrUrl);
                await File.WriteAllBytesAsync(savePath, bytes);

                SetWallpaper(savePath);
                StatusText.Text = "Success (Downloaded)";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "Failed: " + ex.Message;
        }
    }
    private static void SetWallpaper(string path)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }
    private void Theme_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        bool isLight = ThemeLightRadio.IsChecked == true;
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
        if (key != null)
        {
            int val = isLight ? 1 : 0;
            key.SetValue("AppsUseLightTheme", val, Microsoft.Win32.RegistryValueKind.DWord);
            key.SetValue("SystemUsesLightTheme", val, Microsoft.Win32.RegistryValueKind.DWord);
        }
        var res = Resources;
        res["WindowBackground"] = isLight ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(30,30,30));
        res["WindowText"] = isLight ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
        res["InputBackground"] = isLight ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(50,50,50));
        res["InputText"] = isLight ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
    }
}