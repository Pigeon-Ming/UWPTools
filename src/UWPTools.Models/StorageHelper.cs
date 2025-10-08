using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace UWPTools.Models
{
    public class StorageHelper
    {
        /// <summary>
        /// 递归获取指定文件夹中的所有文件
        /// </summary>
        /// <param name="folder">要搜索的存储文件夹</param>
        /// <returns>包含文件夹中所有文件的列表</returns>
        public static async Task<List<StorageFile>> GetFilesInFolder(StorageFolder folder)
        {
            // 获取文件夹中的所有项目（文件和子文件夹）
            var items = await folder.GetItemsAsync();
            List<StorageFile> files = new List<StorageFile>();
            
            // 遍历所有项目，区分文件和文件夹
            foreach (var item in items)
            {
                if (item is StorageFile)
                    files.Add((StorageFile)item);
                else if (item is StorageFolder)
                    // 递归处理子文件夹，将其中的文件添加到结果列表
                    files.AddRange(await GetFilesInFolder((StorageFolder)item));
            }
            return files;
        }
        public static async Task<StorageFile> CreateFileInAppLocalFolderAsync(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            return await CreateFileAsync(folder, fileName);
        }

        public static async Task<StorageFolder> CreateFolderInAppLocalFolderAsync(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem storageItem = await folder.GetItemAsync(folderName);
            if (storageItem == null || storageItem is StorageFile)
                return await folder.CreateFolderAsync(folderName);
            else
                return await folder.GetFolderAsync(folderName);
        }

        public static async Task<StorageFile> CreateFileInDownloadsFolderAsync(string fileName)
        {
            //StorageFolder folder = ApplicationData.Current.LocalFolder;
            return await DownloadsFolder.CreateFileAsync(fileName);
        }

        public static async Task<StorageFile> CreateTempFile(string fileName)
        {
            StorageFolder folder = await CreateFolderInAppLocalFolderAsync("Temp");
            return await CreateFileAsync(folder, fileName);
        }

        public static async Task<StorageFile> CreateFileAsync(StorageFolder storageFolder, string fileName)
        {

            if (!await IsItemExistAsync(storageFolder, fileName))
            {
                return await storageFolder.CreateFileAsync(fileName);
            }
            else
            {
                string fileType = fileName.Substring(fileName.Length - fileName.LastIndexOf("."));
                for (int i = 1; i < 999; i++)
                {
                    if (!await IsItemExistAsync(storageFolder, fileType + $"({i})" + fileType))
                    {
                        return await storageFolder.CreateFileAsync(fileType + $"({i})" + fileType);
                    }
                }
            }
            return null;
        }

        public static async Task<byte[]> ReadFileAsBytesAsync(StorageFile file)
        {
            if (file != null)
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(file);
                byte[] bytes = buffer.ToArray();
                return bytes;
            }
            return null;
        }

        public static async Task<string> ReadFileAsStringAsync(StorageFile storageFile)
        {
            if (storageFile == null)
                return null;
            try
            {
                return await Windows.Storage.FileIO.ReadTextAsync(storageFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch
            {
                return null;
            }
        }

        public static async Task WriteBytesToFileAsync(StorageFile file, byte[] data)
        {
            if (file != null && data != null)
            {
                await FileIO.WriteBytesAsync(file, data);
            }
        }

        public static async Task<bool> WriteStringToFileAsync(StorageFile storageFile, string content)
        {
            if (storageFile == null)
                return false;
            try
            {
                await Windows.Storage.FileIO.WriteTextAsync(storageFile, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> IsItemExistAsync(StorageFolder parentFolder, string itemName)
        {

            IStorageItem storageItem = await parentFolder.TryGetItemAsync(itemName);
            if (storageItem == null)
                return false;
            else
                return true;
        }

        public static string GetFutureAccessListToken(StorageFile storageFile)
        {
            return Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
        }

        public static void RemoveFutureAccessListToken(string token)
        {
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(token);
        }

        public static string GetFutureAccessListToken(StorageFolder storageFolder)
        {
            return Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFolder);
        }

        public static async Task<StorageFolder> GetStorageFolderFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        public static async Task<StorageFile> GetStorageFileFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        public static async Task<StorageFolder> GetApplicationDataFolderAsync(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await IsItemExistAsync(folder, folderName))
                return await folder.GetFolderAsync(folderName);
            return await folder.CreateFolderAsync(folderName);
        }

        public static async Task<StorageFile> GetStorageFileFromStorageFolderAsync(StorageFolder storageFolder, string fileName)
        {
            if (await IsItemExistAsync(storageFolder, fileName))
            {
                return await storageFolder.GetFileAsync(fileName);
            }
            else
            {
                return await CreateFileAsync(storageFolder, fileName);
            }
        }

        public static async void LaunchAppLocalFolder()
        {
            var t = new FolderLauncherOptions();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            await Launcher.LaunchFolderAsync(folder, t);
        }

        public static async Task<List<StorageFolder>> GetRemovableDevicesStorageFolderAsync()
        {
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            return (await externalDevices.GetFoldersAsync()).ToList();
        }

    }
}
