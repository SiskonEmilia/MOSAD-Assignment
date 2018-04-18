using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Lab1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static Memorandum.MemorandumViewModel ViewModel;
        public static int PresentItem { get; set; }
        public static MainPage Current;
        public MainPage()
        {
            ViewModel = App.ViewModel;
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("myNavigationState"))
            {
                ContentFrame.SetNavigationState((string)ApplicationData.Current.LocalSettings.Values["myNavigationState"]);
                ApplicationData.Current.LocalSettings.Values.Remove("myNavigationState");
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (ContentFrame.CanGoBack) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
            else
                ContentFrame.Navigate(typeof(NavigatorPage));
            ContentFrame.Visibility = Visibility.Visible;
            Current = this;
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // 返回按钮逻辑
            if (!e.Handled)
            {
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                    e.Handled = true;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // 本函数处理左侧导航栏点击事件
            // 寻找点击的Item
            var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
            NavView_Navigate(item as NavigationViewItem);

            // 处理右侧Frame跳转逻辑
            if (ContentFrame.CurrentSourcePageType != typeof(NavigatorPage))
            {
                if (ContentFrame.CurrentSourcePageType != typeof(NewPage) || !ContentFrame.CanGoBack)
                    ContentFrame.Navigate(typeof(NavigatorPage));
                else
                    ContentFrame.GoBack();
                    
            }
            else if (ContentFrame.Visibility == Visibility.Visible)
            {
                ContentFrame.Visibility = Visibility.Collapsed;
            }
            else
            {
                ContentFrame.Visibility = Visibility.Visible;
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // 动态添加item
            NavView.MenuItems.Add(new NavigationViewItemSeparator());
            NavView.MenuItems.Add(new NavigationViewItem()
            { Content = "文件夹", Icon = new SymbolIcon(Symbol.Folder), Tag = "文件夹" });
            // 委托导航事件处理函数
            ContentFrame.Navigated += On_Navigated;
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            /*This place is left for future use*/
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (ContentFrame.CanGoBack) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            /*
            此处为右侧Frame导航后的处理函数
             */
        }

        private void AddPage_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 800)
            {
                if (ContentFrame.CurrentSourcePageType != typeof(NewPage))
                    ContentFrame.Navigate(typeof(NewPage));
                ContentFrame.Visibility = Visibility.Visible;
                NavigatorPage.isCreating = true;
                NewPage.Current.myDate.Date = System.DateTime.Now;
                NewPage.Current.myTitle.Text = "";
                NewPage.Current.myDetail.Text = "";
                NewPage.Current.myImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));
                NewPage.Current.bytes = null;
                NewPage.Current.DeletePage.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ContentFrame.CurrentSourcePageType != typeof(NavigatorPage))
                {
                    if (ContentFrame.CurrentSourcePageType != typeof(NewPage) || !ContentFrame.CanGoBack)
                        ContentFrame.Navigate(typeof(NavigatorPage));
                    else
                        ContentFrame.GoBack();

                }
                else
                {
                    ContentFrame.Visibility = Visibility.Visible;
                }
                NavigatorPage.Current.RightPad.Visibility = Visibility.Visible;
                NavigatorPage.isCreating = true;
                NavigatorPage.Current.myDate.Date = System.DateTime.Now;
                NavigatorPage.Current.myTitle.Text = "";
                NavigatorPage.Current.myDetail.Text = "";
                NavigatorPage.Current.myImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));
                NavigatorPage.Current.bytes = null;
                NavigatorPage.Current.DeletePage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
