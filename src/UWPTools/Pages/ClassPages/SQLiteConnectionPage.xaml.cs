using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using UWPTools.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Net.WebRequestMethods;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace UWPTools.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SQLiteConnectionPage : Page
    {
        String dbPath { get; set; }
        public StorageFile dbFile {  get; set; }

        public SQLiteConnectionPage()
        {
            this.InitializeComponent();
            //_ = InitAsync();
        }

        async Task InitAsync()
        {
            if (!await StorageHelper.IsFileExistAsync(ApplicationData.Current.LocalFolder,"MyDataBase.db"))
            {
                await ApplicationData.Current.LocalFolder.CreateFileAsync("MyDataBase.db");
            }

        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            _ = OpenFileAsync();
        }

        async Task OpenFileAsync()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".db");
            fileOpenPicker.FileTypeFilter.Add(".db3");
            fileOpenPicker.FileTypeFilter.Add(".sqlite");
            dbFile = await fileOpenPicker.PickSingleFileAsync();
            
            if(dbFile == null)
            {
                dbPath = null;
                FilePathTextBlock.Text = "请选择一个数据库文件……\n(注意，因UWP沙箱机制的限制UWP内置的SQLite无法直接操作打开的文件，所以UWPTools会为打开的文件在内部临时文件夹创建一个副本，后续所有操作都将在创建的副本中进行)";
                ExecuteNonQueryButton.IsEnabled = false;
            }
            else
            {
                if(await StorageHelper.IsFileExistAsync(ApplicationData.Current.TemporaryFolder, dbFile.Name))
                {
                    StorageFile file = await ApplicationData.Current.TemporaryFolder.GetFileAsync(dbFile.Name);
                    await file.DeleteAsync();
                }
                StorageFile storageFile = await dbFile.CopyAsync(ApplicationData.Current.TemporaryFolder);
                
                dbPath = storageFile.Path;
                FilePathTextBlock.Text = dbPath;
                ExecuteNonQueryButton.IsEnabled = true;
            }
        }

        private void OpenFileInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            _ = OpenFileInExplorer();
        }

        async Task OpenFileInExplorer()
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.TemporaryFolder);
        }

        private async void ExecuteNonQueryButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteNonQueryAsync();
        }

        async Task ExecuteNonQueryAsync()
        {
            using (var dbHelper = new SQLiteConnection(dbPath))
            {
                string SQLCommend = SQLTextBox.Text;
                ConsoleTextBox.Text += await dbHelper.ExecuteNonQueryAsync(SQLCommend) + "\n";
            };
        }

        private async void ExecuteScalarButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScalarAsync();
        }

        async Task ExecuteScalarAsync()
        {
            using (var dbHelper = new SQLiteConnection(dbPath))
            {
                string SQLCommend = SQLTextBox.Text;
                ConsoleTextBox.Text += await dbHelper.ExecuteScalarAsync<int>(SQLCommend) + "\n";
            }
            ;
        }

        private void QuerySingleButton_Click(object sender, RoutedEventArgs e)
        {
            _ = QuerySingleAsync();
        }

        async Task QuerySingleAsync()
        {
            if (CheckDataClassJsonTextBox() == false)
            {
                ConsoleTextBox.Text += "请配置实体类JSON！\n";
                DataClassJsonTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                DataClassJsonTextBox.BorderBrush = ConsoleTextBox.BorderBrush;
            }
            using (var dbHelper = new SQLiteConnection(dbPath))
            {
                string SQLCommend = SQLTextBox.Text;
                var mapFunc = DynamicMapHelper.CreateDynamicMapFunc(DataClassJsonTextBox.Text);
                var user = await dbHelper.QuerySingleAsync(
                    SQLCommend,
                    mapFunc
                );
                Dictionary<string, string> columnDictionary = DynamicMapHelper.GetColumnHeaders(DataClassJsonTextBox.Text);
                var users = new List<ExpandoObject> { user };
                //DynamicMapHelper.PrintAsTableToConsole(users, columnDictionary);
                ConsoleTextBox.Text += DynamicMapHelper.PrintAsTableToString(users,columnDictionary);
            }
        }

        private void QueryListButton_Click(object sender, RoutedEventArgs e)
        {
            _ = QueryListAsync();
        }

        async Task QueryListAsync()
        {
            if(CheckDataClassJsonTextBox() == false)
            {
                ConsoleTextBox.Text += "请配置实体类JSON！\n";
                DataClassJsonTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                DataClassJsonTextBox.BorderBrush = ConsoleTextBox.BorderBrush;
            }
            using (var dbHelper = new SQLiteConnection(dbPath))
            {
                string SQLCommend = SQLTextBox.Text;
                var mapFunc = DynamicMapHelper.CreateDynamicMapFunc(DataClassJsonTextBox.Text);
                var users = await dbHelper.QueryListAsync(
                    SQLCommend,
                    mapFunc
                );
                Dictionary<string, string> columnDictionary = DynamicMapHelper.GetColumnHeaders(DataClassJsonTextBox.Text);
                //DynamicMapHelper.PrintAsTableToConsole(users, columnDictionary);
                ConsoleTextBox.Text += DynamicMapHelper.PrintAsTableToString(users, columnDictionary);
            }
        }

        bool CheckDataClassJsonTextBox()
        {
            if (String.IsNullOrEmpty(DataClassJsonTextBox.Text))
                return false;
            else
                return true;
        }

        private void QueryByPageButton_Click(object sender, RoutedEventArgs e)
        {
            _ = QueryByPageAsync();
        }

        async Task QueryByPageAsync()
        {
            if (CheckDataClassJsonTextBox() == false)
            {
                ConsoleTextBox.Text += "请配置实体类JSON！\n";
                DataClassJsonTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                DataClassJsonTextBox.BorderBrush = ConsoleTextBox.BorderBrush;
            }

            if (String.IsNullOrEmpty(PageSizeTextBox.Text))
            {
                PageSizeTextBox.Text = "10";
            }
            if(String.IsNullOrEmpty(PageIndexTextBox.Text))
            {
                PageIndexTextBox.Text = "1";
            }
            if (CheckPageIndexTextBoxFormat() == false)
            {
                PageIndexTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else
                PageIndexTextBox.BorderBrush = ConsoleTextBox.BorderBrush;
            if (CheckPageSizeTextBoxFormat() == false)
            {
                PageSizeTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else
                PageSizeTextBox.BorderBrush = ConsoleTextBox.BorderBrush;

            using (var dbHelper = new SQLiteConnection(dbPath))
            {
                string SQLCommend = SQLTextBox.Text;
                var mapFunc = DynamicMapHelper.CreateDynamicMapFunc(DataClassJsonTextBox.Text);
                var users = await dbHelper.QueryByPageAsync(
                    SQLCommend,
                    OrderFieldTextBox.Text,
                    Convert.ToInt32(PageIndexTextBox.Text),
                    Convert.ToInt32(PageSizeTextBox.Text),
                    mapFunc
                );
                Dictionary<string, string> columnDictionary = DynamicMapHelper.GetColumnHeaders(DataClassJsonTextBox.Text);
                ConsoleTextBox.Text += DynamicMapHelper.PrintAsTableToString(users, columnDictionary);
            }
        }

        bool CheckPageSizeTextBoxFormat()
        {
            if(PageSizeTextBox.Text.All(char.IsNumber))
                return true;
            else
                return false;
        }

        bool CheckPageIndexTextBoxFormat()
        {
            if (PageIndexTextBox.Text.All(char.IsNumber))
                return true;
            else
                return false;
        }

        
    }
}
