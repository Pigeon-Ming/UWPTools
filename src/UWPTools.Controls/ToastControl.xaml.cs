using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Net.WebRequestMethods;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace UWPTools.Controls
{
    public class ToastData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string title;
        public string Title { get { return title; }  set  { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TitleVisibility"));} }

        public Visibility TitleVisibility
        {
            get { return string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible; }
        }

        private string content;

        public string Content { get { return content; } set {  content = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContentVisibility")); } }

        public Visibility ContentVisibility
        {
            get { return string.IsNullOrEmpty(Content) ? Visibility.Collapsed : Visibility.Visible; }
        }

        private int progressValue { get { return progressValue; } set { progressValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressValue")); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressVisibility")); } } 

        public int ProgressValue { get; set; } = -1;

        public Visibility ProgressBarVisibility
        {
            get { return ProgressValue == -1 ? Visibility.Collapsed : Visibility.Visible; }
        }

        private int progressMaximum;

        public int ProgressMaximum { get { return progressMaximum; } set { progressMaximum = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressMaximum")); } }
    }

    public sealed partial class ToastControl : UserControl
    {
        private Popup popup;

        private int duration;

        //ToastData toastData;

        public ToastControl()
        {
            this.InitializeComponent();

            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;

            popup = new Popup();
            popup.Child = this;

            Loaded += ToastControl_Loaded;
        }

        private void ToastControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.PopupIn.Begin();
            //当进入动画执行之后，代表着弹窗已经到指定位置了，再指定位置等一秒 就可以消失回去了
            this.PopupIn.Completed += PopupIn_Completed;
        }

        public void Show(string title, string content, int duration)
        {
            toastData.Title = title;
            toastData.Content = content;
            this.duration = duration;
            Show();
        }

        public void Show(string content, int duration)
        {
            toastData.Title = null;
            toastData.Content = content;
            this.duration = duration;
            Show();
        }

        private void Show()
        {
            popup.IsOpen = true;
        }

        private async void PopupIn_Completed(object sender, object e)
        {
            PopupIn.Completed -= PopupIn_Completed;
            await Task.Delay(duration);
            PopupOut.Begin();
            PopupOut.Completed += PopupOut_Completed;
        }

        private void PopupOut_Completed(object sender, object e)
        {
            PopupOut.Completed -= PopupOut_Completed;
            popup.IsOpen = false;
        }
    }
}
