using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.JsonService;

namespace GeneralsZeroHourEditor.Tasks;

public class SaveProjectDataTask(IJsonService jsonService, IDataService dataService) : EngineTask
{
    public override async Task Call()
    {
        MaxWork = 3;
        await DoWork();
        Work = MaxWork;
    }

    private async Task DoWork()
    {
        // Task[] tasks =
        // [
        //     CreateWorkTask(jsonService.SaveAsJson("ObjectModels.json", dataService.ObjectModels)),
        //     CreateWorkTask(jsonService.SaveAsJson("ArmorModels.json", dataService.ArmorModels)),
        //     CreateWorkTask(jsonService.SaveAsJson("GameStrings.json", dataService.GameStrings)),
        //     CreateWorkTask(jsonService.SaveAsJson("LocomotorModels.json", dataService.LocomotorModels)),
        //     CreateWorkTask(jsonService.SaveAsJson("LocomotorValues.json", dataService.LocomotorValues)),
        // ];
        //
        // await Task.WhenAll(tasks);
    }
}
