using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace InternetApplication
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        MusicVM playlist = MusicVM.GetInstance();
        ObservableCollection<MusicPiece> Musics;
        public static PlayerPage Current;

        public PlayerPage()
        {
            this.InitializeComponent();
            Current = this;
            mediaPlayer.SetPlaybackSource(playlist.GetList());
            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            Musics = playlist.GetMusics();

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var enter = Window.Current.CoreWindow.GetKeyState(VirtualKey.Escape);
                if (enter.HasFlag(CoreVirtualKeyStates.Down))
                {
                    if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsFullScreenMode)
                    {
                        Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ExitFullScreenMode();
                        mediaPlayer.Margin = new Thickness(0, 0, 0, 100);
                        PlayerControllers.Opacity = 1;
                    }
                }
            }
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                PlayButtonView.Symbol = Symbol.Pause;

                StoryboardR.Resume();
            }
            else
            {
                PlayButtonView.Symbol = Symbol.Play;
                
                StoryboardR.Pause();
            }
        }

        private void MediaPlayer_VolumeChanged(object sender, RoutedEventArgs e)
        {
            VolumeController.Value = mediaPlayer.Volume * 100;
        }

        public void RefreshGUI(string Title, string Artist, BitmapImage Cover, bool isMusic)
        {
            if (isMusic)
            {
                TitleHolder.Text = Title;
                ArtistHolder.Text = Artist;
                CoverHolder.Source = Cover;
                BGImage.ImageSource = Cover;
                mediaPlayer.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleHolder.Text = "";
                ArtistHolder.Text = "";
                CoverHolder.Source = null;
                BGImage.ImageSource = null;
                mediaPlayer.Visibility = Visibility.Visible;
            }
        }

        private void PlayerControllers_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PlayerControllers.Opacity = 1;
        }

        private void PlayerControllers_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsFullScreenMode && TitleHolder.Text == "")
            {
                PlayerControllers.Opacity = 0;
            }
        }
        

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                mediaPlayer.Play();
            }
            base.OnNavigatedTo(e);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            playlist.MovePrevious();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                mediaPlayer.Pause();
            }
            else
            {
                mediaPlayer.Play();
            }

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            playlist.MoveNext();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ExitFullScreenMode();
                mediaPlayer.Margin = new Thickness(0, 0, 0, 100);
            }
            else
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                mediaPlayer.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void VolumeController_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mediaPlayer.Volume = (sender as Slider).Value / 100;
        }

        private async void AddMusicButton_Click(object sender, RoutedEventArgs e)
        {

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".flac");

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                playlist.AddMusic(file);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyUser("Successfully added to playing list");
                });
            }
            
        }
        

        private async void AddVideoButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".mp4");

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                playlist.AddVideo(file);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyUser("Successfully added to playing list");
                });
            }
        }

        private async void NotifyUser(string notice)
        {
            await (new Windows.UI.Popups.MessageDialog(notice, "Notice")).ShowAsync();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepeatIcon.Symbol == Symbol.RepeatAll)
            {
                RepeatIcon.Symbol = Symbol.RepeatOne;
                mediaPlayer.IsLooping = true;
            }
            else if (RepeatIcon.Symbol == Symbol.RepeatOne) {
                RepeatIcon.Symbol = Symbol.GlobalNavigationButton;
                mediaPlayer.IsLooping = false;
                playlist.GetList().AutoRepeatEnabled = false;
            }
            else
            {
                RepeatIcon.Symbol = Symbol.RepeatAll;
                playlist.GetList().AutoRepeatEnabled = true;
                mediaPlayer.IsLooping = false;
            }
        }

        private void PlayMusic_Click(object sender, RoutedEventArgs e)
        {
            var index = playlist.hasMedia((sender as Button).Tag as string);

            playlist.MoveTo(index);
        }

        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            if (SwitchIcon.Symbol == Symbol.Rotate)
            {
                SwitchIcon.Symbol = Symbol.DisableUpdates;
                CoverHolder.Visibility = Visibility.Collapsed;
                RotateImage.Visibility = Visibility.Visible;
                StoryboardR.Begin();
            }
            else
            {
                SwitchIcon.Symbol = Symbol.Rotate;
                CoverHolder.Visibility = Visibility.Visible;
                RotateImage.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class MusicProcessConverter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }

    public class MusicProcessConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (((TimeSpan)value).ToString(@"mm\:ss"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
