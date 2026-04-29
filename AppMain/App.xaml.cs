using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GraphicsService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.NavigationService;
using GeneralsZeroHourEditor.Tasks;
using GeneralsZeroHourEditor.UX.ViewModels.HomePage;
using GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;
using GeneralsZeroHourEditor.UX.ViewModels.TopBar;
using GeneralsZeroHourEditor.UX.Views.HomePage;
using GeneralsZeroHourEditor.UX.Views.InfantryPage;
using GeneralsZeroHourEditor.UX.Views.MainPage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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
        // services.AddSingleton<TopBarViewModel>();
        // services.AddSingleton<HomeViewViewModel>();
        // services.AddSingleton<ActorsPageViewModel>();
        // services.AddSingleton<AttributesPageViewModel>();
        // services.AddSingleton<ElementsPageViewModel>();
        // services.AddSingleton<GrowthsPageViewModel>();
        // services.AddSingleton<DisciplinesPageViewModel>();
        // services.AddSingleton<EquipmentPageViewModel>();
        // services.AddSingleton<StatesPageViewModel>();
        // services.AddSingleton<AnimationsPageViewModel>();

        #endregion

        #region Register Views

        services.AddSingleton<MainPage>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<InfantryPage>();
        // services.AddSingleton<MainWindow>();
        // services.AddSingleton<HomeView>();
        // services.AddSingleton<TopBar>();
        // services.AddSingleton<ActorsPage>();
        // services.AddSingleton<AttributesPage>();
        // services.AddSingleton<ElementsPage>();
        // services.AddSingleton<GrowthsPage>();
        // services.AddSingleton<DisciplinesPage>();
        // services.AddSingleton<EquipmentPage>();
        // services.AddSingleton<StatesPage>();
        // services.AddSingleton<AnimationsPage>();

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
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<IJsonService, JsonService>();

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
