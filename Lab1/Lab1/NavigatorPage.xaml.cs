using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Lab1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
 
    public sealed partial class NavigatorPage : Page
    {
        public static NavigatorPage Current;
        public static bool isCreating;
        public static Memorandum memo;
        public Memorandum.MemorandumViewModel ViewModel;
        public byte[] bytes;
        public string fileToken;
        public NavigatorPage()
        {
            ViewModel = MainPage.ViewModel;
            this.InitializeComponent();
            Current = this;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            bytes = null;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string myNotice = "\n";
            if (myTitle.Text == "")
            {
                myNotice += "标题不能为空\n";
            }
            if (myDate.Date < DateTime.Now.Date)
            {
                myNotice += "你不能选择一个过去的时间戳\n";
            }
            if (myDetail.Text == "")
            {
                myNotice += "描述不能为空\n";
            }
            if (myNotice == "\n")
            {
                if (isCreating)
                {
                    ViewModel.Memos.Add(new Memorandum());
                    PageList.SelectedIndex = (PageList.Items.Count - 1);
                    memo.MemoTitle = myTitle.Text;
                    memo.MemoDetail = myDetail.Text;
                    memo.MemoDate = myDate.Date.DateTime;
                    memo.MemoImg = myImg.Source as BitmapImage;
                    memo.Bytes = bytes;
                    myNotice += "创建成功\n";
                    App.tileController.Update();
                    App.databaseManager.InsertMemo(memo);
                }
                else
                {
                    memo.MemoTitle = myTitle.Text;
                    memo.MemoDetail = myDetail.Text;
                    memo.MemoDate = myDate.Date.DateTime;
                    memo.MemoImg = myImg.Source as BitmapImage;
                    memo.Bytes = bytes;
                    myNotice += "更新成功\n";
                    App.tileController.Update();
                    App.databaseManager.UpdateMemo(memo);
                }
            }
            (new Windows.UI.Popups.MessageDialog(myNotice, "提示信息")).ShowAsync();
        }
        

        private void ResetChanges_Click(object sender, RoutedEventArgs e)
        {
            if (isCreating)
            {
                myTitle.Text = "";
                myDetail.Text = "";
                myDate.Date = DateTime.Now.Date;
                myImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));
                bytes = null;
                fileToken = null;
            }
            else
            {
                RightPad.Visibility = Visibility.Visible;
                myTitle.Text = memo.MemoTitle;
                myDetail.Text = memo.MemoDetail;
                myDate.Date = memo.MemoDate.Date;
                myImg.Source = memo.MemoImg;
                bytes = memo.Bytes;
                fileToken = null;
            }
        }

        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainPage.PresentItem = PageList.SelectedIndex;
            if (MainPage.PresentItem == -1)
                return;
            memo = ViewModel.Memos.ElementAt(MainPage.PresentItem);
            if (Window.Current.Bounds.Width > 880)
            {
                RightPad.Visibility = Visibility.Visible;
                if (!isCreating)
                {
                    myTitle.Text = memo.MemoTitle;
                    myDetail.Text = memo.MemoDetail;
                    myDate.Date = memo.MemoDate.Date;
                    myImg.Source = memo.MemoImg;
                    bytes = memo.Bytes;
                    fileToken = null;
                }
                else
                {
                    isCreating = false;
                    DeletePage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (!isCreating)
                {
                    NewPage.Current.myTitle.Text = memo.MemoTitle;
                    NewPage.Current.myDetail.Text = memo.MemoDetail;
                    NewPage.Current.myDate.Date = memo.MemoDate.Date;
                    NewPage.Current.myImg.Source = memo.MemoImg;
                }
                else
                {
                    isCreating = false;
                    NewPage.Current.DeletePage.Visibility = Visibility.Visible;
                }
            }
        }

        private void PageList_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (Window.Current.Bounds.Width > 880)
            {
                if (memo != null)
                {
                    myTitle.Text = memo.MemoTitle;
                    myDetail.Text = memo.MemoDetail;
                    myDate.Date = memo.MemoDate.Date;
                    myImg.Source = memo.MemoImg;
                    bytes = memo.Bytes;
                    fileToken = null;
                }
                if (isCreating)
                {
                    isCreating = false;
                    DeletePage.Visibility = Visibility.Visible;
                    RightPad.Visibility = Visibility.Visible;
                }
                else
                    RightPad.Visibility = (RightPad.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                Frame.Navigate(typeof(NewPage));
                if (memo != null)
                {
                    NewPage.Current.myTitle.Text = memo.MemoTitle;
                    NewPage.Current.myDetail.Text = memo.MemoDetail;
                    NewPage.Current.myDate.Date = memo.MemoDate.Date;
                    NewPage.Current.myImg.Source = memo.MemoImg;
                    NewPage.Current.bytes = memo.Bytes;
                }
                if (isCreating)
                {
                    isCreating = false;
                    NewPage.Current.DeletePage.Visibility = Visibility.Visible;
                }
            }
        }

        private void DeletePage_Click(object sender, RoutedEventArgs e)
        {
            App.databaseManager.DeleteMemo(memo.Id);
            memo = null;
            fileToken = null;
            bytes = null;
            ViewModel.Memos.RemoveAt(MainPage.PresentItem);
            PageList.SelectedIndex = -1;
            (new Windows.UI.Popups.MessageDialog("删除成功", "提示信息")).ShowAsync();
            RightPad.Visibility = Visibility.Collapsed;
            App.tileController.Update();
        }

        private async System.Threading.Tasks.Task SelectImage_ClickAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".jpg");openPicker.FileTypeFilter.Add(".jpeg");openPicker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                BitmapImage bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                myImg.Source = bitmap;
                readBytes(file);
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            SelectImage_ClickAsync();
        }

        private void ScaleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                myImg.Height = myImg.Width = slider.Value + 250;
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("NavigationPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NavigationPage"))
                {
                    // Read data
                    var composite = ApplicationData.Current.LocalSettings.Values["NavigationPage"] as ApplicationDataCompositeValue;
                    myTitle.Text = (string)composite["myTitle"];
                    myDetail.Text = (string)composite["myDetail"];
                    myDate.Date = (DateTimeOffset)composite["myDate"];
                    RightPad.Visibility = ((bool)composite["RightPad"]) ? Visibility.Visible : Visibility.Collapsed;
                    readAndLoad(fileToken = (string)composite["myImg"]);
                    isCreating = (bool)composite["isCreating"];
                    if ((int)composite["Selected"] >= 0)
                    {
                        memo = ViewModel.Memos.ElementAt((int)composite["Selected"]);
                        bytes = memo.Bytes;
                    }
                    DeletePage.Visibility = isCreating ? Visibility.Collapsed : Visibility.Visible;

                    // Clean data
                    ApplicationData.Current.LocalSettings.Values.Remove("NavigationPage");
                }
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (((App)App.Current).issuspend)
            {
                ((App)App.Current).issuspend = false;
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["myTitle"] = myTitle.Text;
                composite["myDetail"] = myDetail.Text;
                composite["myDate"] = myDate.Date;
                composite["myImg"] = fileToken;
                composite["RightPad"] = (RightPad.Visibility == Visibility.Visible) ? true : false;
                composite["isCreating"] = isCreating;
                composite["Selected"] = PageList.SelectedIndex;
                ApplicationData.Current.LocalSettings.Values["NavigationPage"] = composite;
            }
            base.OnNavigatedFrom(e);
        }

        private async System.Threading.Tasks.Task readAndLoad(string token)
        {
            StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            if (file == null) return;
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            BitmapImage bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            myImg.Source = bitmap as ImageSource;
            readBytes(file);
        }

        private void SharePage_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = myTitle.Text;
            request.Data.Properties.Description = myDetail.Text;
            request.Data.SetText(myDetail.Text);
            readBitmap(request);
         }

        private async System.Threading.Tasks.Task readBitmap (DataRequest request)
        {
            if (fileToken != null)
            {
                StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(fileToken);
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            }
            else
            {
                if (bytes == null)
                    request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png")));
                else
                    request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(DatabaseManager.ConvertToRandomAccessStream(new MemoryStream(bytes))));
            }
        }

        public async System.Threading.Tasks.Task readBytes (StorageFile file)
        {
            var stream = await file.OpenReadAsync();
            using (var dataReader = new DataReader(stream))
            {
                bytes = new byte[stream.Size];
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            App.databaseManager.LoadAndShow(args.QueryText);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                App.databaseManager.Loadmemos(sender.Text);
            }
        }

        private void itemCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach(var memo in ViewModel.Memos)
            {
                App.databaseManager.UpdateMemo(memo);
            }
        }
    }
}
