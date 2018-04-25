using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace InternetApplication
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class QueryPage : Page
    {
        private ViewModel viewmodel;

        public QueryPage()
        {
            this.InitializeComponent();

            this.DataContext = viewmodel = ViewModel.GetInstance(true);
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null, double.MaxValue, null);
        }
        

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var enter = Window.Current.CoreWindow.GetKeyState(VirtualKey.Enter);
                if (enter.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CheckMessage();
                }
            }
        }

        private void CheckMessage()
        {
            if (TypingArea.Text == "")
            {
                NotifyUser("You cannot send an empty message.");
                return;
            }
            string message = string.Copy(TypingArea.Text);
            TypingArea.Text = "";
            viewmodel.AddSelf(message);
            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null, MyScrollViewer.VerticalOffset, null);
            SendMessage(message);
        }

        private async void SendMessage(string message)
        {
            var client = new HttpClient();
            Uri requestUri;
            if (message.Contains("本机"))
            {
                requestUri = new Uri("http://ip-api.com/xml/");
            }
            else if (Regex.Match(message, "((25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d)))\\.){3}(25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d)))").Success)
            {
                requestUri = new Uri("http://ip-api.com/xml/" + message);
            }
            else
            {
                NotifyUser("您输入的语句有误，请直接输入您要查询的 IP 地址，或者输入\"本机IP\"");
                return;
            }
            string body = "";

            try
            {
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                body = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            ResponsXML.SetSource(body);
            string result = "";
            if (ResponsXML.GetField("status") == "success")
            {
                result += "IP 地址：" + ResponsXML.GetField("query") +
                    "\n国家：" + ResponsXML.GetField("country") +
                    "\n地区：" + ResponsXML.GetField("regionName") +
                    "\n城市：" + ResponsXML.GetField("city") +
                    "\n时区：" + ResponsXML.GetField("timezone") +
                    "\n服务提供商：" + ResponsXML.GetField("isp");
            }
            else
            {
                result = "请检查您输入的 IP 地址，他似乎不合法或不支持查询";
            }

            viewmodel.AddEmilia(result);
            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null, double.MaxValue, null);
        }

        private void NotifyUser(string notice)
        {
            (new Windows.UI.Popups.MessageDialog(notice, "Notice")).ShowAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null, double.MaxValue, null);
            base.OnNavigatedTo(e);
        }
    }
}
