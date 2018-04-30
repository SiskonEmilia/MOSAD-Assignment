using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace InternetApplication
{
    // Model, used to store infomation of each song
    public class MusicPiece
    {
        public string Title;
        public string Artist;
        public BitmapImage AlbumCover;
        public bool isMusic = true;
    }

    public class MusicVM
    {
       

        // Single Instance Mode
        // Private instance
        private static MusicVM _instance;
        // Get instance method
        // Create (if haven't been created) and return it
        public static MusicVM GetInstance()
        {
            if (_instance == null)
                _instance = new MusicVM();
            return _instance;
        }
        // Private constructor, initialize the player's source and add event-handler
        // Then add the initial music
        private MusicVM()
        {
            playbackList.AutoRepeatEnabled = true;
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
            AddFirstMusic();
        }

        // Playing list and its player
        private static MediaPlaybackList playbackList = new MediaPlaybackList();

        // Viewmodel, used to bind with template
        private ObservableCollection<MusicPiece> musics = new ObservableCollection<MusicPiece>();
        public ObservableCollection<MusicPiece> Musics
        {
            get
            {
                return this.musics;
            }
            set
            {
                this.musics = value;
            }
        }

        public ObservableCollection<MusicPiece> GetMusics()
        {
            return Musics;
        }

        /*
         * AddMusic method, get a StorageFile and add information and the music itself into
         * the Viewmodel and playing list
         */
        public async void AddMusic(StorageFile file)
        {
            if (file == null)
                return;

            MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
            MusicPiece song = new MusicPiece()
            {
                Title = musicProperties.Title,
                Artist = musicProperties.Artist,
                AlbumCover = new BitmapImage(),
                isMusic = true
            };

            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300);
            if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                song.AlbumCover.SetSource(thumbnail);
            else
                song.AlbumCover.UriSource = new Uri("ms-appx:///Assets/LockScreenLogo.scale-200.png");

            musics.Add(song);

            var mediaSource = MediaSource.CreateFromStorageFile(file);
            var item = new MediaPlaybackItem(mediaSource);
            item.CanSkip = true;
            playbackList.Items.Add(item);
        }

        public async void AddVideo(StorageFile file)
        {
            if (file == null)
                return;

            MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
            MusicPiece song = new MusicPiece()
            {
                Title = file.Name,
                Artist = "",
                AlbumCover = new BitmapImage(),
                isMusic = false
            };

            song.AlbumCover.UriSource = new Uri("ms-appx:///Assets/LockScreenLogo.scale-200.png");
            musics.Add(song);

            var mediaSource = MediaSource.CreateFromStorageFile(file);
            var item = new MediaPlaybackItem(mediaSource);
            item.CanSkip = true;
            playbackList.Items.Add(item);
        }

        
        public async void AddFirstMusic()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Delacey - Dream It Possible.mp3"));
            AddMusic(file);
        }

        // If the current item change, refresh the GUI
        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            RefreshGUI(playbackList.CurrentItemIndex);
        }

        // Refresh the GUI
        private async void RefreshGUI(uint Uindex)
        {
            var test = (int)Uindex;
            if (test > musics.Count || test < 0)
                return;
            var item = musics[(int)Uindex];
            
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (PlayerPage.Current != null)
                    PlayerPage.Current.RefreshGUI(item.Title, item.Artist, item.AlbumCover, item.isMusic);
            });
            
        }

        public MediaPlaybackList GetList()
        {
            return playbackList;
        }

        public int hasMedia (string name)
        {
            for(int i = 0; i < musics.Count; ++i)
            {
                if (musics[i].Title == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public void MoveTo(int index)
        {
            playbackList.MoveTo((uint)index);
        }

        public void MoveNext()
        {
            playbackList.MoveNext();
        }

        public void MovePrevious()
        {
            playbackList.MovePrevious();
        }
    }
}
