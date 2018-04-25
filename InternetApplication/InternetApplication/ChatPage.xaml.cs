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
        public ChatPage()
        {
            this.InitializeComponent();

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

            RequestJSON.SetSource(body);
            var jsonurl = RequestJSON.GetURL();
            var text = RequestJSON.GetText();
            if (jsonurl != "")
            {
                text += "\n" + jsonurl;
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
