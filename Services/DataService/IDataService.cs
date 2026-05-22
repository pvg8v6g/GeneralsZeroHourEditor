namespace GeneralsZeroHourEditor.Services.DataService;

public interface IDataService
{
    string ParseIniToJson(string filePath);
    string ParseIniContentToJson(string content);
    string[] CollectTopLevelNames(string projectDataDir, string topLevelType);
}
