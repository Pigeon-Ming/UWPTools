using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPTools.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace UWPTools.Pages.ControlPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ExplorerControlPage : Page
    {
        public ExplorerControlPage()
        {
            this.InitializeComponent();
        }

        private async void SelectRootFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            StorageFolder storageFolder = await folderPicker.PickSingleFolderAsync();
            if (storageFolder == null)
                return;
            RootFolderPath.Text = storageFolder.Path;
            ControlGrid.Children.Clear();
            ControlGrid.Children.Add(new ExplorerControl(storageFolder));
        }
    }
}
