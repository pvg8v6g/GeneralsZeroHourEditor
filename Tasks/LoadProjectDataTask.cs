using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.JsonService;

namespace GeneralsZeroHourEditor.Tasks;

public class LoadProjectDataTask(IDataService dataService, IJsonService jsonService) : EngineTask
{
    public override async Task Call()
    {
        MaxWork = 2;
        await DoWork();
        Work = MaxWork;
    }

    private async Task DoWork()
    {
        // dataService.ObjectModels = await jsonService.LoadFromJson<ObjectModel>("ObjectModels.json");
        // Work++;
        // dataService.ArmorModels = await jsonService.LoadFromJson<Armor>("ArmorModels.json");
        // Work++;
        // dataService.LocomotorModels = await jsonService.LoadFromJson<Locomotor>("LocomotorModels.json");
        // Work++;
        // dataService.LocomotorValues = await jsonService.LoadFromJson<LocomotorValues>("LocomotorValues.json");
        // Work++;
    }
}
