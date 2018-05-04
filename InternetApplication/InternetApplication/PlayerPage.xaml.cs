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
using Windows.Storage;
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
        SystemMediaTransportControls _systemMediaTransportControls;
        int index = -1;

        public PlayerPage()
        {
            this.InitializeComponent();
            Current = this;
            mediaPlayer.AutoPlay = false;
            mediaPlayer.SetPlaybackSource(playlist.GetList());
            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            Musics = playlist.GetMusics();

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            // Get current SMTC
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.IsPreviousEnabled = true;
            _systemMediaTransportControls.IsNextEnabled = true;
            _systemMediaTransportControls.IsStopEnabled = true;

            _systemMediaTransportControls.ButtonPressed += SystemControls_ButtonPressed;
            // Handle ButtonPressed Event
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (index != -1)
            {
                playlist.MoveTo(index);
                mediaPlayer.Play();
                PlayButtonView.Symbol = Symbol.Pause;
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
        }

        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaPlayer.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaPlayer.Pause();
                    });
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        playlist.MovePrevious();
                    });
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        playlist.MoveNext();
                    });
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaPlayer.Stop();
                    });
                break;
                default:
                    break;
            }
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
            switch (mediaPlayer.CurrentState)
            {
                case MediaElementState.Playing:
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    PlayButtonView.Symbol = Symbol.Pause;
                    StoryboardR.Resume();
                    break;
                case MediaElementState.Paused:
                    if (index != -1)
                    {
                        index = -1;
                        break;
                    }
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    PlayButtonView.Symbol = Symbol.Play;
                    StoryboardR.Pause();
                    break;
                case MediaElementState.Stopped:
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    PlayButtonView.Symbol = Symbol.Play;
                    StoryboardR.Stop();
                    break;
                case MediaElementState.Closed:
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }
        }

        private void MediaPlayer_VolumeChanged(object sender, RoutedEventArgs e)
        {
            VolumeController.Value = mediaPlayer.Volume * 100;
        }

        public async void RefreshGUI(string Title, string Artist, BitmapImage Cover, bool isMusic, StorageFile file)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(0);
            if (isMusic)
            {
                TitleHolder.Text = Title;
                ArtistHolder.Text = Artist;
                CoverHolder.Source = Cover;
                BGImage.ImageSource = Cover;
                mediaPlayer.Visibility = Visibility.Collapsed;

                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;

                await updater.CopyFromFileAsync(MediaPlaybackType.Music, file);

                // Update the system media transport controls
                updater.Update();
            }
            else
            {
                TitleHolder.Text = "";
                ArtistHolder.Text = "";
                CoverHolder.Source = null;
                BGImage.ImageSource = null;
                mediaPlayer.Visibility = Visibility.Visible;

                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;

                await updater.CopyFromFileAsync(MediaPlaybackType.Video, file);

                updater.VideoProperties.Title = Title;

                // Update the system media transport controls
                updater.Update();
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
            _systemMediaTransportControls.ButtonPressed -= SystemControls_ButtonPressed;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                index = (int)e.Parameter;
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
            else if (RepeatIcon.Symbol == Symbol.RepeatOne)
            {
                RepeatIcon.Symbol = Symbol.Shuffle;
                mediaPlayer.IsLooping = false;
                playlist.GetList().AutoRepeatEnabled = true;
                playlist.GetList().ShuffleEnabled = true;
            }
            else if (RepeatIcon.Symbol == Symbol.Shuffle)
            {
                RepeatIcon.Symbol = Symbol.GlobalNavigationButton;
                mediaPlayer.IsLooping = false;
                playlist.GetList().AutoRepeatEnabled = false;
                playlist.GetList().ShuffleEnabled = false;
            }
            else
            {
                RepeatIcon.Symbol = Symbol.RepeatAll;
                playlist.GetList().AutoRepeatEnabled = true;
                mediaPlayer.IsLooping = false;
                playlist.GetList().ShuffleEnabled = false;
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

        private void RemoveMusic_Click(object sender, RoutedEventArgs e)
        {
            var index = playlist.hasMedia((sender as Button).Tag as string);

            playlist.Remove(index);
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
