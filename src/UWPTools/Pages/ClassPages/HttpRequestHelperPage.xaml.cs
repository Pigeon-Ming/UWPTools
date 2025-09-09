using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace UWPTools.Pages.ClassPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HttpRequestHelperPage : Page
    {
        HttpRequestHelper httpRequestHelper = new HttpRequestHelper();

        

        public HttpRequestHelperPage()
        {
            this.InitializeComponent();
        }

        private void SendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendRequestAsync();
        }

        async Task SendRequestAsync()
        {
            int RequestType = RequestTypeComboBox.SelectedIndex;
            switch (RequestType)
            {
                case 0:
                    //GET请求
                    await SendGetRequestAsync();
                    break;
                case 1:
                    await SendPostRequestAsync("");
                    break;
            }
        }

        private async Task SendGetRequestAsync()
        {
            string ResponseStr = await httpRequestHelper.GetAsync(UrlTextBox.Text);
            ResponseTextBox.Text = JSONHelper.FormatJson(ResponseStr);
        }

        private async Task SendPostRequestAsync<TRequestBody>(TRequestBody body)
        {
            string ResponseStr = await httpRequestHelper.PostAsync<string,TRequestBody>(UrlTextBox.Text, body);
            ResponseTextBox.Text = JSONHelper.FormatJson(ResponseStr);
        }

        
    }
}
