using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.GraphicsService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.NavigationService;
using GeneralsZeroHourEditor.Tasks;
using GeneralsZeroHourEditor.UX.ViewModels.HomePage;
using GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;
using GeneralsZeroHourEditor.UX.ViewModels.TopBar;
using GeneralsZeroHourEditor.UX.ViewModels.VehiclePage;
using GeneralsZeroHourEditor.UX.ViewModels.WeaponPage;
using GeneralsZeroHourEditor.UX.Views.HomePage;
using GeneralsZeroHourEditor.UX.Views.InfantryPage;
using GeneralsZeroHourEditor.UX.Views.MainPage;
using GeneralsZeroHourEditor.UX.Views.VehiclePage;
using GeneralsZeroHourEditor.UX.Views.WeaponPage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using GeneralsZeroHourEditor.Services.BigArchiveService;
using GeneralsZeroHourEditor.Services.FolderPickerService;

namespace GeneralsZeroHourEditor.AppMain;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App
{
    #region Fields

    private static ServiceProvider? ServiceProvider { get; set; }

    #endregion

    public static IServiceProvider? Services => ServiceProvider;

    public App()
    {
        var services = new ServiceCollection();

        #region Register View Models

        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<TopBarViewModel>();
        services.AddSingleton<InfantryPageViewModel>();
        services.AddSingleton<VehiclePageViewModel>();
        services.AddSingleton<WeaponPageViewModel>();

        #endregion

        #region Register Views

        services.AddSingleton<MainPage>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<InfantryPage>();
        services.AddSingleton<VehiclePage>();
        services.AddSingleton<WeaponPage>();

        #endregion

        #region Register Tasks

        services.AddSingleton<InitialLoadDataTask>();
        services.AddSingleton<SaveProjectDataTask>();
        services.AddSingleton<LoadProjectDataTask>();

        #endregion

        #region Register Services

        services.AddSingleton<Func<Type, EngineTask>>(provider => taskType => (EngineTask) provider.GetRequiredService(taskType));
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ILocationService, LocationService>();
        services.AddSingleton<IGraphicsService, GraphicsService>();
        services.AddSingleton<IGameDataService, GameDataService>();
        services.AddSingleton<IGameRegistryService, GameRegistryService>();
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<IJsonService, JsonService>();
        services.AddSingleton<IBigArchiveService, BigArchiveService>();
        services.AddSingleton<IFolderPickerService>(sp => new FolderPickerService(sp.GetRequiredService<MainPage>()));

        #endregion

        ServiceProvider = services.BuildServiceProvider();

        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = Services?.GetRequiredService<MainPage>();
        _window?.Activate();
    }

    private Window? _window;
}
