using System.Runtime.InteropServices;
using GeneralsZeroHourEditor.Services.GameDataService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace GeneralsZeroHourEditor.UX.Views.MainPage;

public partial class MainPage
{
    #region Properties

    public static MainPage Window { get; private set; } = null!;

    #endregion

    #region Fields

    private const uint WmSeticon = 0x0080;
    private const IntPtr IconSmall = 0;
    private const IntPtr IconBig = 1;
    private const uint ImageIcon = 1;
    private const uint LrLoadfromfile = 0x0010;

    #endregion

    public MainPage()
    {
        Window = this;
        InitializeComponent();

        var hWnd = WindowNative.GetWindowHandle(this);

        // Set Small Icon (Title Bar - 16x16 or 24x24)
        var smallIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Graphics/ico/24x24.ico");
        var hSmallIcon = LoadImage(IntPtr.Zero, smallIconPath, ImageIcon, 16, 16, LrLoadfromfile);
        SendMessage(hWnd, WmSeticon, IconSmall, hSmallIcon);

        // Set Big Icon (Taskbar - 32x32 or 48x48)
        var bigIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Graphics/ico/48x48.ico");
        var hBigIcon = LoadImage(IntPtr.Zero, bigIconPath, ImageIcon, 32, 32, LrLoadfromfile);
        SendMessage(hWnd, WmSeticon, IconBig, hBigIcon);
        SizeChanged += MainWindow_SizeChanged;
        Activated += MainWindow_Activated;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    #region Listeners

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated;
        UpdateSize(Bounds.Width, Bounds.Height);
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        UpdateSize(args.Size.Width, args.Size.Height);
    }

    private void UpdateSize(double width, double height)
    {
        var gameDataService = AppMain.App.Services!.GetRequiredService<IGameDataService>();
        gameDataService.WindowWidth = width;
        gameDataService.WindowHeight = height;
    }

    #endregion
}
