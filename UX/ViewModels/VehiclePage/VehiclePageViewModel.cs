using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.UX.ViewModels.VehiclePage;

public class VehiclePageViewModel(ILocationService locationService, IGameRegistryService gameRegistryService, IGameDataService gameDataService)
    : BaseViewModel
{
    // public GameObjectModel Model { get; } = new();
    //
    // public RelayCommand<object> ItemInvokedCommand => new(OnItemInvoked);
    //
    // private void OnItemInvoked(object? invokedItem)
    // {
    //     if (invokedItem is not GameObjectItemModel item) return;
    //     Model.SelectedNode = item;
    //     LoadSelectedDetail(item.Name);
    // }
    //
    // protected override async Task LoadedAction()
    // {
    //     LoadSchema();
    //     LoadVehicleList();
    //     await Task.CompletedTask;
    // }
    //
    // private void LoadSchema()
    // {
    //     try
    //     {
    //         var schemaPath = Path.Combine(locationService.ProjectDirectory!, "Schema", "game.schema.json");
    //         if (File.Exists(schemaPath))
    //         {
    //             gameRegistryService.LoadFromSchema(schemaPath);
    //         }
    //         else
    //         {
    //             var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
    //             gameRegistryService.Initialize(projectDataDir);
    //         }
    //
    //         Model.Modules.Clear();
    //         foreach (var def in gameRegistryService.Registry.ModuleDefinitions)
    //         {
    //             var moduleModel = new GameObjectModuleModel { Name = def.Name };
    //             foreach (var field in def.Fields) moduleModel.Fields.Add(field);
    //             foreach (var subDef in def.SubBlocks)
    //             {
    //                 var sub = new GameObjectSubBlockModel { Name = subDef.Name };
    //                 foreach (var f in subDef.Fields) sub.Fields.Add(f);
    //                 moduleModel.SubBlocks.Add(sub);
    //             }
    //
    //             Model.Modules.Add(moduleModel);
    //         }
    //     }
    //     catch
    //     {
    //         // ignore schema errors for first cut
    //     }
    // }
    //
    // private void LoadVehicleList()
    // {
    //     Model.GameObjectGroups.Clear();
    //     var sideMap = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);
    //
    //     foreach (var unit in gameDataService.Vehicles)
    //     {
    //         var side = string.IsNullOrWhiteSpace(unit.Side) ? "Unknown" : unit.Side;
    //         if (!sideMap.TryGetValue(side, out var group))
    //         {
    //             group = new SideGroupModel { Name = side };
    //             sideMap[side] = group;
    //             Model.GameObjectGroups.Add(group);
    //         }
    //
    //         if (group.Children.All(i => i.Name != unit.Name))
    //         {
    //             group.Children.Add(new GameObjectItemModel { Name = unit.Name });
    //         }
    //     }
    //
    //     // Sort groups and items
    //     var sortedGroups = Model.GameObjectGroups.OrderBy(g => g.Name).ToList();
    //     Model.GameObjectGroups.Clear();
    //     foreach (var group in sortedGroups)
    //     {
    //         var sortedItems = group.Children.OrderBy(i => i.Name).ToList();
    //         group.Children.Clear();
    //         foreach (var item in sortedItems) group.Children.Add(item);
    //         Model.GameObjectGroups.Add(group);
    //     }
    // }
    //
    // private void LoadSelectedDetail(string name)
    // {
    //     var picked = gameDataService.Vehicles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
    //     if (picked is not null)
    //     {
    //         Model.Detail = picked;
    //     }
    // }
}
