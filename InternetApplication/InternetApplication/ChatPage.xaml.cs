using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
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
using Windows.Data.Json;
using System.Net.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace InternetApplication
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private ViewModel viewmodel;
        public static ChatPage current;
        private static bool doParse = true;

        public ChatPage()
        {
            this.InitializeComponent();
            current = this;

            this.DataContext = viewmodel = ViewModel.GetInstance();
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
                    MyScrollViewer.UpdateLayout();
                    MyScrollViewer.ChangeView(null,double.MaxValue, null);
                }
            }
        }

        private void CheckMessage()
        {
            switch (TypingArea.Text)
            {
                case "":
                    NotifyUser("You cannot send an empty message.");
                    return;
                case "关闭解析":
                    viewmodel.AddSelf(TypingArea.Text);
                    TypingArea.Text = "";
                    viewmodel.AddEmilia("好的，JSON解析已关闭。不过这样哥哥还能看懂我说话吗？");
                    doParse = false;
                    return;
                case "开启解析":
                    viewmodel.AddSelf(TypingArea.Text);
                    TypingArea.Text = "";
                    viewmodel.AddEmilia("好的，JSON解析已开启。不过人类的语言用着有些别扭呢=v=");
                    doParse = true;
                    return;
                case "放点音乐吧":
                    viewmodel.AddSelf(TypingArea.Text);
                    TypingArea.Text = "";
                    viewmodel.AddEmilia("好的，那就来一首哥哥喜欢的 Dream It Possible 吧~");
                    (Parent as Frame).Navigate(typeof(PlayerPage), 0);
                    return;
                default:
                    break;
            }

            string message = string.Copy(TypingArea.Text);
            TypingArea.Text = "";
            viewmodel.AddSelf(message);
            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null,double.MaxValue, null);
            SendMessage(message);
        }

        public void ScrollToBottom()
        {
            MyScrollViewer.UpdateLayout();
            MyScrollViewer.ChangeView(null, double.MaxValue, null);
        }

        private async void SendMessage(string message)
        {
            var client = new HttpClient();
            var content = new StringContent(RequestJSON.GetJSON(message));
            Uri requestUri = new Uri("http://openapi.tuling123.com/openapi/api/v2");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            string body = "";

            try
            {
                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                body = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            string text = "";

            if (doParse)
            {
                RequestJSON.SetSource(body);
                var jsonurl = RequestJSON.GetURL();
                text = RequestJSON.GetText();
                if (jsonurl != "")
                {
                    text += "\n" + jsonurl;
                }
            }
            else
            {
                text = body;
            }

            viewmodel.AddEmilia(text);
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
