using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPTools.Models;
using UWPTools.Pages.ControlPages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace UWPTools
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SettingsManager.InitUserSettings();
        }

        Dictionary<string, Type> NavigationDictionary = new Dictionary<string, Type>()
        {
            {"ExplorerControl",typeof(ExplorerControlPage)}
        };

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type type;
            if(NavigationDictionary.TryGetValue(((NavigationViewItem)sender.SelectedItem).Tag.ToString(),out type))
            {
                MainFrame.Navigate(type);
            }
            
        }
    }
}
