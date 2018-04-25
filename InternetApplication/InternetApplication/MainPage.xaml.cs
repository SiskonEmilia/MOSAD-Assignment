using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace InternetApplication
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // 动态添加item
            NavView.MenuItems.Add(new NavigationViewItemSeparator());
            NavView.MenuItems.Add(new NavigationViewItem()
            { Content = "Chat Robot", Icon = new SymbolIcon(Symbol.Character), Tag = "Chat Robot" });
            NavView.MenuItems.Add(new NavigationViewItem()
            { Content = "IP Query", Icon = new SymbolIcon(Symbol.Find), Tag = "IP Query" });
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // 本函数处理左侧导航栏点击事件
            // 寻找点击的Item
            var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
            NavView_Navigate(item as NavigationViewItem);
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Content)
            {
                case "Chat Robot":
                    if (ContentFrame.CurrentSourcePageType != typeof(ChatPage))
                        ContentFrame.Navigate(typeof(ChatPage));
                    break;
                case "IP Query":
                    if (ContentFrame.CurrentSourcePageType != typeof(QueryPage))
                        ContentFrame.Navigate(typeof(QueryPage));
                    break;
                default:
                    break;
            }
        }

        
    }
}
