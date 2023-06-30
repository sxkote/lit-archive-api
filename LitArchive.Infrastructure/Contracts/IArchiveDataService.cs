using LitArchive.Infrastructure.Models;

namespace LitArchive.Infrastructure.Contracts
{
    public interface IArchiveDataService
    {
        string GetAccessKey(string? path = null);
        bool ValidateAccessKey(string key, string? path = null);

        ArchiveData GetArchiveData(string path);
        List<ArchiveItem> GetArchiveItems(string path);        
        string GetVideoPoster(string posterFolder, string videoFilePath);
    }
}
