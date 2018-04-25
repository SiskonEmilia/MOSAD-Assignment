using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class NewPage : Page
    {
        public static Memorandum.MemorandumViewModel ViewModel;
        public static NewPage Current;
        public string fileToken;
        public byte[] bytes;
        public NewPage()
        {
            ViewModel = MainPage.ViewModel;
            this.InitializeComponent();
            Current = this;
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
                if (NavigatorPage.isCreating)
                {
                    ViewModel.Memos.Add(new Memorandum());
                    NavigatorPage.Current.PageList.SelectedIndex = (NavigatorPage.Current.PageList.Items.Count - 1);
                    NavigatorPage.memo.MemoTitle = myTitle.Text;
                    NavigatorPage.memo.MemoDetail = myDetail.Text;
                    NavigatorPage.memo.MemoDate = myDate.Date.DateTime;
                    NavigatorPage.memo.MemoImg = myImg.Source as BitmapImage;
                    NavigatorPage.memo.Bytes = bytes;
                    myNotice += "创建成功\n";
                    App.tileController.Update();
                    App.databaseManager.InsertMemo(NavigatorPage.memo);
                }
                else
                {
                    NavigatorPage.memo.MemoTitle = myTitle.Text;
                    NavigatorPage.memo.MemoDetail = myDetail.Text;
                    NavigatorPage.memo.MemoDate = myDate.Date.DateTime;
                    NavigatorPage.memo.MemoImg = myImg.Source as BitmapImage;
                    NavigatorPage.memo.Bytes = bytes;
                    myNotice += "更新成功\n";
                    App.tileController.Update();
                    App.databaseManager.UpdateMemo(NavigatorPage.memo);
                }

                
                ((Frame)Parent).GoBack();
            }
            (new Windows.UI.Popups.MessageDialog(myNotice, "提示信息")).ShowAsync();
        }

        private void ResetChanges_Click(object sender, RoutedEventArgs e)
        {
            if (NavigatorPage.isCreating)
            {
                myTitle.Text = "";
                myDetail.Text = "";
                myDate.Date = DateTime.Now.Date;
                myImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));
                fileToken = null;
                bytes = null;
            }
            else
            {
                RightPad.Visibility = Visibility.Visible;
                myTitle.Text = NavigatorPage.memo.MemoTitle;
                myDetail.Text = NavigatorPage.memo.MemoDetail;
                myDate.Date = NavigatorPage.memo.MemoDate.Date;
                myImg.Source = NavigatorPage.memo.MemoImg;
                bytes = NavigatorPage.memo.Bytes;
                fileToken = null;
            }
        }

        private void DeletePage_Click(object sender, RoutedEventArgs e)
        {
            NavigatorPage.memo = null;
            NavigatorPage.Current.fileToken = fileToken = null;
            NavigatorPage.Current.bytes = bytes = null;
            ViewModel.Memos.RemoveAt(MainPage.PresentItem);
            NavigatorPage.Current.PageList.SelectedIndex = -1;
            (new Windows.UI.Popups.MessageDialog("删除成功", "提示信息")).ShowAsync();
            ((Frame)Parent).GoBack();
            App.tileController.Update();
        }

        private async System.Threading.Tasks.Task SelectImage_ClickAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".jpg"); openPicker.FileTypeFilter.Add(".jpeg"); openPicker.FileTypeFilter.Add(".png");

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
                ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                {
                    // Read data
                    var composite = ApplicationData.Current.LocalSettings.Values["NewPage"] as ApplicationDataCompositeValue;
                    myTitle.Text = (string)composite["myTitle"];
                    myDetail.Text = (string)composite["myDetail"];
                    myDate.Date = (DateTimeOffset)composite["myDate"];
                    readAndLoad(fileToken = (string)composite["myImg"]);
                    NavigatorPage.isCreating = (bool)composite["isCreating"];
                    if ((int)composite["Selected"] >= 0)
                    {
                        NavigatorPage.memo = ViewModel.Memos.ElementAt((int)composite["Selected"]);
                        NavigatorPage.Current.bytes = bytes = NavigatorPage.memo.Bytes;
                    }
                    DeletePage.Visibility = NavigatorPage.isCreating ? Visibility.Collapsed : Visibility.Visible;


                    // Clean data
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
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
                composite["isCreating"] = NavigatorPage.isCreating;
                composite["Selected"] = NavigatorPage.Current.PageList.SelectedIndex;
                ApplicationData.Current.LocalSettings.Values["NewPage"] = composite;
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

        public async System.Threading.Tasks.Task readBytes(StorageFile file)
        {
            var stream = await file.OpenReadAsync();
            using (var dataReader = new DataReader(stream))
            {
                bytes = new byte[stream.Size];
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);
            }
        }
    }  
}
