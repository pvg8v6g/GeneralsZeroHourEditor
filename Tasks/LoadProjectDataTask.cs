using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.Tasks;

public class LoadProjectDataTask(
    IDataService dataService,
    IJsonService jsonService,
    ILocationService locationService,
    IGameRegistryService gameRegistryService,
    IGameDataService gameDataService) : EngineTask
{
    public override async Task Call()
    {
        MaxWork = GetWorkLoad();
        await DoWork();
        Work = MaxWork;
    }

    private int GetWorkLoad()
    {
        return 11;
    }

    private async Task DoWork()
    {
        // 1) Load schema and catalogs
        var projectDir = locationService.ProjectDirectory;
        if (projectDir is null) return;
        var schemaPath = Path.Combine(projectDir, "Schema", "game.schema.json");
        var dataDir = Path.Combine(projectDir, "Data");
        Directory.CreateDirectory(dataDir);

        if (File.Exists(schemaPath))
        {
            gameRegistryService.LoadFromSchema(schemaPath);
            Work++;
            // Ensure changeable catalogs are populated too (they live in GameDataService)
            gameDataService.GameWeapons.SetRange(dataService.CollectTopLevelNames(dataDir, "Weapon"));
            Work++;
            gameDataService.GameArmors.SetRange(dataService.CollectTopLevelNames(dataDir, "Armor"));
            Work++;
            gameDataService.GameLocomotors.SetRange(dataService.CollectTopLevelNames(dataDir, "Locomotor"));
            Work++;
            // Load Sciences catalog
            gameDataService.GameSciences.SetRange(dataService.CollectTopLevelNames(dataDir, "Science"));
            Work++;
            gameDataService.FXLists.SetRange(dataService.CollectTopLevelNames(dataDir, "FXList"));
            Work++;
        }
        else
        {
            gameRegistryService.Initialize(dataDir);
            Work++;
            // Optional: persist mined schema for faster subsequent loads
            Directory.CreateDirectory(Path.GetDirectoryName(schemaPath)!);
            gameRegistryService.SaveSchema(schemaPath);
            Work++;
        }

        // 2) Preload Infantry objects fully into memory (delegated to JsonService)
        var infantry = (await jsonService.LoadInfantryAsync(dataDir)).OrderBy(u => u.Name).ToList();
        gameDataService.Infantry.SetRange(infantry);
        Work++;

        // 3) Preload Vehicles
        var vehicles = (await jsonService.LoadVehiclesAsync(dataDir)).OrderBy(u => u.Name).ToList();
        gameDataService.Vehicles.SetRange(vehicles);
        Work++;

        // 4) Preload Structures
        var structures = (await jsonService.LoadStructuresAsync(dataDir)).OrderBy(u => u.Name).ToList();
        gameDataService.Structures.SetRange(structures);
        Work++;
    }
}
