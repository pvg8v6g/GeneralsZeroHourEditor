using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.Enumerations;
using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Services.GameDataService;
using MercuryLibrary.WinUI3Components;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.Models;

public class PrerequisiteSetModel : PropertyChangedUpdater
{
    public PrerequisiteType PrerequisiteType
    {
        get;
        set
        {
            SetField(ref field, value);

            switch (value)
            {
                case PrerequisiteType.Object:
                {
                    var infantry = GameDataService.Infantry
                        .Select(x => x.Name)
                        .ToArray();
                    var vehicles = GameDataService.Vehicles
                        .Select(x => x.Name)
                        .ToArray();
                    var structures = GameDataService.Structures
                        .Select(x => x.Name)
                        .ToArray();
                    string[] masterList = [..infantry, ..vehicles, ..structures];
                    masterList = masterList
                        .OrderBy(x => x)
                        .ToArray();
                    PrerequisiteChoices.SetRange(masterList);
                    break;
                }
                case PrerequisiteType.Science:
                {
                    PrerequisiteChoices.SetRange(GameDataService.GameSciences.OrderBy(x => x).ToArray());
                    break;
                }
            }
        }
    } = PrerequisiteType.Object;

    public string Prerequisite
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public ObservableCollection<string> PrerequisiteChoices { get; } = [];

    private IGameDataService GameDataService { get; init; } = App.Services!.GetRequiredService<IGameDataService>();
}
