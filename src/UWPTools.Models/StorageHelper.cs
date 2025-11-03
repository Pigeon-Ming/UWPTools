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
        /// 递归获取指定文件夹中的所有文件。
        /// </summary>
        /// <param name="folder">要搜索的存储文件夹</param>
        /// <returns>此方法完成时，它将返回包含文件夹中所有文件的列表。</returns>
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

        /// <summary>
        /// 在应用程序的LocalState文件夹下创建文件。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>此方法完成时，它将返回创建的文件。</returns>
        public static async Task<StorageFile> CreateFileInAppLocalFolderAsync(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            return await CreateFileAsync(folder, fileName);
        }

        /// <summary>
        /// 在应用程序的LocalState文件夹下创建文件夹。
        /// </summary>
        /// <param name="folderName">文件夹名称</param>
        /// <returns>此方法完成时，它将返回创建的文件夹，若文件夹已存在，则返回存在的文件夹。</returns>
        public static async Task<StorageFolder> CreateFolderInAppLocalFolderAsync(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem storageItem = await folder.GetItemAsync(folderName);
            if (storageItem == null || storageItem is StorageFile)
                return await folder.CreateFolderAsync(folderName);
            else
                return await folder.GetFolderAsync(folderName);
        }

        /// <summary>
        /// 在“下载”文件夹中创建文件。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>此方法完成时，它将返回创建的文件。</returns>
        public static async Task<StorageFile> CreateFileInDownloadsFolderAsync(string fileName)
        {
            //StorageFolder folder = ApplicationData.Current.LocalFolder;
            return await DownloadsFolder.CreateFileAsync(fileName);
        }

        /// <summary>
        /// 在应用程序的LocalState目录下Temp文件夹创建文件。
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>此方法完成时，它将返回创建的文件。</returns>
        public static async Task<StorageFile> CreateTempFile(string fileName)
        {
            StorageFolder folder = await CreateFolderInAppLocalFolderAsync("Temp");
            return await CreateFileAsync(folder, fileName);
        }

        /// <summary>
        /// 在指定目录下创建创建文件。
        /// </summary>
        /// <param name="storageFolder">父文件夹</param>
        /// <param name="fileName">新文件名称</param>
        /// <returns>此方法完成时，它将返回创建的文件，若已存在重名文件，则指定文件名称基础上自动添加“{<副本号>}”。</returns>
        public static async Task<StorageFile> CreateFileAsync(StorageFolder storageFolder, string fileName)
        {

            if (!await IsFileExistAsync(storageFolder, fileName))
            {
                return await storageFolder.CreateFileAsync(fileName);
            }
            else
            {
                string fileType = fileName.Substring(fileName.Length - fileName.LastIndexOf("."));
                for (int i = 1; i < 999; i++)
                {
                    if (!await IsFileExistAsync(storageFolder, fileType + $"({i})" + fileType))
                    {
                        return await storageFolder.CreateFileAsync(fileType + $"({i})" + fileType);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 在指定目录下创建文件夹。
        /// </summary>
        /// <param name="storageFolder">父文件夹</param>
        /// <param name="folderName">新文件夹名称</param>
        /// <returns>此方法完成时，它将返回创建的文件夹，若存在同名文件夹，则返回已存在的文件夹。</returns>
        public static async Task<StorageFolder> CreateFolderAsync(StorageFolder storageFolder, string folderName)
        {

            if (!await IsFolderExistAsync(storageFolder, folderName))
            {
                return await storageFolder.CreateFolderAsync(folderName);
            }
            else
            {
                return await storageFolder.GetFolderAsync(folderName);
            }
        }

        /// <summary>
        /// 以二进制方式读取文件。
        /// </summary>
        /// <param name="storageFile">要读取的文件</param>
        /// <returns>此方法完成时，它将返回读取到的二进制数据</returns>
        public static async Task<byte[]> ReadFileAsBytesAsync(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(storageFile);
                byte[] bytes = buffer.ToArray();
                return bytes;
            }
            return null;
        }

        /// <summary>
        /// 以字符串方式读取文件。
        /// </summary>
        /// <param name="storageFile">要读取的文件</param>
        /// <returns>此方法完成时，它将返回读取到的字符串</returns>
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

        /// <summary>
        /// 将二进制数据写入文件。
        /// </summary>
        /// <param name="storageFile">要写入数据的文件</param>
        /// <param name="data">要写入文件的二进制数据</param>
        /// <returns></returns>
        public static async Task WriteBytesToFileAsync(StorageFile storageFile, byte[] data)
        {
            if (storageFile != null && data != null)
            {
                await FileIO.WriteBytesAsync(storageFile, data);
            }
        }

        /// <summary>
        /// 将字符串数据写入文件
        /// </summary>
        /// <param name="storageFile">要写入数据的文件</param>
        /// <param name="content">要写入文件的内容</param>
        /// <returns>此方法完成时，它将返回表示字符串是否被成功写入文件的值。</returns>
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

        /// <summary>
        /// 判断文件是否存在。
        /// </summary>
        /// <param name="parentFolder">父文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>此方法完成时，它将返回表示文件是否存在的值。</returns>
        public static async Task<bool> IsFileExistAsync(StorageFolder parentFolder, string fileName)
        {

            IStorageItem storageItem = await parentFolder.TryGetItemAsync(fileName);
            if (storageItem is StorageFile == false || storageItem == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 判断文件夹是否存在。
        /// </summary>
        /// <param name="parentFolder">父文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>此方法完成时，它将返回表示文件夹是否存在的值。</returns>
        public static async Task<bool> IsFolderExistAsync(StorageFolder parentFolder, string itemName)
        {

            IStorageItem storageItem = await parentFolder.TryGetItemAsync(itemName);
            if (storageItem is StorageFolder == false || storageItem == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 获取文件的FutureAccessList令牌字符串。
        /// </summary>
        /// <param name="storageFile">文件</param>
        /// <returns>文件的FutureAccessList令牌字符串。</returns>
        public static string GetFutureAccessListToken(StorageFile storageFile)
        {
            return Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
        }

        /// <summary>
        /// 移除FutureAccessList令牌字符串指定的文件的访问权限。
        /// </summary>
        /// <param name="token">文件的令牌字符串</param>
        public static void RemoveFutureAccessListToken(string token)
        {
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(token);
        }

        /// <summary>
        /// 获取文件夹的FutureAccessList令牌字符串。
        /// </summary>
        /// <param name="storageFolder">文件</param>
        /// <returns>文件夹的FutureAccessList令牌字符串。</returns>
        public static string GetFutureAccessListToken(StorageFolder storageFolder)
        {
            return Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFolder);
        }

        /// <summary>
        /// 通过令牌字符串在FutureAccessList中获取文件夹。
        /// </summary>
        /// <param name="token"></param>
        /// <returns>通过令牌获取的文件夹。</returns>
        public static async Task<StorageFolder> GetStorageFolderFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        /// <summary>
        /// 通过令牌字符串在FutureAccessList中获取文件。
        /// </summary>
        /// <param name="token"></param>
        /// <returns>通过令牌获取的文件。</returns>
        public static async Task<StorageFile> GetStorageFileFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        /// <summary>
        /// 获取LocalFolder下的指定文件夹。
        /// </summary>
        /// <param name="folderName">文件夹名称</param>
        /// <returns>指定的文件夹。</returns>
        public static async Task<StorageFolder> GetApplicationDataFolderAsync(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await IsFolderExistAsync(folder, folderName))
                return await folder.GetFolderAsync(folderName);
            return await folder.CreateFolderAsync(folderName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<StorageFile> GetStorageFileFromStorageFolderAsync(StorageFolder storageFolder, string fileName)
        {
            if (await IsFileExistAsync(storageFolder, fileName))
            {
                return await storageFolder.GetFileAsync(fileName);
            }
            else
            {
                return await CreateFileAsync(storageFolder, fileName);
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="storageFolder"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static async Task<StorageFolder> GetStorageFolderFromStorageFolderAsync(StorageFolder storageFolder, string folderName)
        {
            if (await IsFolderExistAsync(storageFolder, folderName))
            {
                return await storageFolder.GetFolderAsync(folderName);
            }
            else
            {
                return await CreateFolderAsync(storageFolder, folderName);
            }
        }

        /// <summary>
        /// 在文件资源管理器中打开ApplicationData.Current.LocalFolder文件夹。
        /// </summary>
        public static async void LaunchAppLocalFolder()
        {
            var t = new FolderLauncherOptions();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            await Launcher.LaunchFolderAsync(folder, t);
        }

        /// <summary>
        /// 获取所有已连接的可移动设备的根文件夹。
        /// </summary>
        /// <returns>当方法完成时，它将返回所有可移动设备的根文件夹组成的列表。</returns>
        public static async Task<List<StorageFolder>> GetRemovableDevicesStorageFolderAsync()
        {
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            return (await externalDevices.GetFoldersAsync()).ToList();
        }

    }
}
