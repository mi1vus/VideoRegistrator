using System.Threading.Tasks;
using MyHomeSecureWeb.Models;

namespace MyHomeSecureWeb.Utilities
{
    public interface IGoogleDriveHelper
    {
        Task<string> GetFolderId(string accessToken, string folderPath, string parentId = null);
        Task<GoogleSearchResult> GetChildrenIDs(string accessToken, string parentId = null);
        Task<GoogleSearchResult> Search(string accessToken, string query);
        Task Delete(string accessToken, string itemId);
        Task<byte[]> GetFileContent(string accessToken, string itemId);
    }
}