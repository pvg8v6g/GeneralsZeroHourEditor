using System.Threading.Tasks;

namespace GeneralsZeroHourEditor.Services.FolderPickerService;

public interface IFolderPickerService
{
    Task<string?> PickGeneralsFolderAsync();
    Task<string?> PickZeroHourFolderAsync();
}
