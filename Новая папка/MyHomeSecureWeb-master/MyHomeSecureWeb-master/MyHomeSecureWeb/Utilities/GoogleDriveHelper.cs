using MyHomeSecureWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHomeSecureWeb.Utilities
{
    public class GoogleDriveHelper : IGoogleDriveHelper
    {
        private const string _createFolderAddress = "https://www.googleapis.com/drive/v2/files";
        private const string _searchAddressTemplate = "https://www.googleapis.com/drive/v2/files?spaces=drive&q={0}"; // corpus=DOMAIN&
        private const string _fileAPIAddressTemplate = _createFolderAddress + "/{0}";

        private const string _fileGetContentAddressTemplate = _createFolderAddress + "/{0}?alt=media";

        private const string _rootFolderName = "HomeSecureStream";

        public async Task<string> GetFolderId(string accessToken, string folderPath, string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                parentId = await GetFolderIdInternal(accessToken, _rootFolderName);
            }

            var pathParts = folderPath.Split('/');
            string folderId = parentId;
            foreach(var folderName in pathParts)
            {
                folderId = await GetFolderIdInternal(accessToken, folderName, folderId);
            }

            return folderId;
        }

        private async Task<string> GetFolderIdInternal(string accessToken, string folderName, string parentId = null)
        { 
            var query = string.Format("title = '{0}'", folderName);
            if (!string.IsNullOrEmpty(parentId))
            {
                query += string.Format(" and '{0}' in parents", parentId);
            }

            var searchResult = await Search(accessToken, query);
            if (searchResult.Items.Length > 0)
            {
                return searchResult.Items[0].Id;
            }
            else
            {
                return await CreateFolder(accessToken, folderName, parentId);
            }
        }

        public async Task<GoogleSearchResult> GetChildrenIDs(string accessToken, string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                parentId = await GetFolderIdInternal(accessToken, _rootFolderName);
            }

            var query = string.Format("'{0}' in parents", parentId);

            return await Search(accessToken, query);
        }

        public async Task<GoogleSearchResult> Search(string accessToken, string query)
        {
            var search = string.Format(_searchAddressTemplate, HttpUtility.UrlEncode(query));

            var request = WebRequest.Create(search);
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }

            return JsonConvert.DeserializeObject<GoogleSearchResult>(responseContent);
        }

        public async Task Delete(string accessToken, string itemId)
        {
            var deleteURL = string.Format(_fileAPIAddressTemplate, itemId);

            var request = WebRequest.Create(deleteURL);
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);
            request.Method = "DELETE";

            await request.GetResponseAsync();
        }

        public async Task<byte[]> GetFileContent(string accessToken, string itemId)
        {
            var fileURL = string.Format(_fileGetContentAddressTemplate, itemId);

            var request = WebRequest.Create(fileURL);
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);

            var response = await request.GetResponseAsync();

            using (var stream = response.GetResponseStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);

                    return memoryStream.GetBuffer();
                }
            }
        }

        private async Task<string> CreateFolder(string accessToken, string folderName, string parentId = null)
        {
            var metadata = new GoogleFileMetadata
            {
                Title = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            if (!string.IsNullOrEmpty(parentId))
            {
                metadata.Parents = new GoogleFileParent[] {
                    new GoogleFileParent { Id = parentId }
                };
            }
            var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata));

            // Get the response
            var request = WebRequest.Create(_createFolderAddress);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);
            request.Method = "POST";

            using (var writeStream = await request.GetRequestStreamAsync())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }

            var parent = JsonConvert.DeserializeObject<GoogleFileParent>(responseContent);
            return parent.Id;
        }
    }
}
