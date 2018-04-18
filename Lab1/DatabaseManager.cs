using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Lab1
{
    public class DatabaseManager
    {
        public class StorageMemo
        {
            [PrimaryKey]
            public int Id { get; set; }

            public bool state { get; set; }
            public string Title { get; set; }
            public string Detail { get; set; }
            public string Time { get; set; }
            public byte[] Image { get; set; }
        }
        private static DatabaseManager instance;
        private static string MyPath;
        private static SQLiteConnection db;
        private const string pattern = "MM/dd/yyyy hh:mm:ss";

        private DatabaseManager()
        {
            MyPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "BeautifulList.sqlite");
            InitializeDatabase();
            // ClearMyDatabaseAndEscape(); // This is used to reset my database~
        }

        public static DatabaseManager GetInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseManager();
            }
            return instance;
        }

        public static void InitializeDatabase()
        {
            db = new SQLiteConnection(new SQLitePlatformWinRT(), MyPath);
            db.CreateTable<StorageMemo>();
        }

        public void Loadmemos(string token = "")
        {
            App.ViewModel.Memos.Clear();
            string sql = "SELECT * FROM StorageMemo";
            if (token != "")
            {
                sql += " WHERE Detail LIKE '%" + token;
                sql += "%' OR Title LIKE '%" + token;
                sql += "%' OR Time LIKE '%" + token + "%'";
            }
            var memos = db.Query<StorageMemo>(sql);
            foreach (var memo in memos)
            {
                BitmapImage bitmap = (memo.Image == null) ? new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png")) : BytesToBitmapImage(memo.Image);
                Memorandum toAdd = new Memorandum(memo.Id)
                {
                    MemoDate = DateTime.ParseExact(memo.Time, pattern, null, System.Globalization.DateTimeStyles.None),
                    MemoDetail = memo.Detail,
                    MemoTitle = memo.Title,
                    Bytes = memo.Image,
                    MemoImg = bitmap,
                    IsDone = memo.state
                };
                App.ViewModel.Memos.Add(toAdd);
            }
            
        }

        public void ClearMyDatabaseAndEscape()
        {
            db.Execute("DELETE FROM StorageMemo");
        }

        public void RestorageMemos()
        {
            ClearMyDatabaseAndEscape();
            foreach (var toAdd in App.ViewModel.Memos)
            {
                db.Insert(new StorageMemo()
                {
                    Id = toAdd.Id,
                    Detail = toAdd.MemoDetail,
                    Title = toAdd.MemoTitle,
                    Time = toAdd.MemoDate.ToString(pattern),
                    Image = toAdd.Bytes,
                    state = toAdd.IsDone
                });
            }
        }

        public void UpdateMemo(Memorandum toAdd)
        {
            db.Update(new StorageMemo()
            {
                Id = toAdd.Id,
                Detail = toAdd.MemoDetail,
                Title = toAdd.MemoTitle,
                Time = toAdd.MemoDate.ToString(pattern),
                Image = toAdd.Bytes,
                state = toAdd.IsDone
            });
        }

        public async void LoadAndShow(string keyword)
        {
            int counter = 0;
            string sql = "SELECT * FROM StorageMemo";
            if (keyword != "")
            {
                sql += " WHERE Detail LIKE '%" + keyword;
                sql += "%' OR Title LIKE '%" + keyword;
                sql += "%' OR Time LIKE '%" + keyword + "%'";
            }
            var memos = db.Query<StorageMemo>(sql);
            var stringbuilder = new StringBuilder();
            foreach(var memo in memos)
            {
                ++counter;
                if (counter > 3)
                {
                    stringbuilder.AppendLine("\nAnd mode...");
                    break;
                }
                stringbuilder.AppendLine($"\nTask {counter}\nTitle: {memo.Title}\nDetail: {memo.Detail}\nTime: {memo.Time}");
            }
            if (counter == 0)
            {
                stringbuilder.AppendLine("NO TASK FOUND");
            }
            var dialog = new MessageDialog(stringbuilder.ToString(), "Query Result");
            await dialog.ShowAsync();
        }

        public void InsertMemo(Memorandum toAdd)
        {
            db.Insert(new StorageMemo()
            {
                Id = toAdd.Id,
                Detail = toAdd.MemoDetail,
                Title = toAdd.MemoTitle,
                Time = toAdd.MemoDate.ToString(pattern),
                Image = toAdd.Bytes,
                state = toAdd.IsDone
            });
        }

        public void DeleteMemo(int id)
        {
            db.Execute("DELETE FROM StorageMemo WHERE Id = " + id);
        }

        public static BitmapImage BytesToBitmapImage(byte[] bytes)
        {
            BitmapImage bitmap = new BitmapImage();
            IRandomAccessStream stream = ConvertToRandomAccessStream(new MemoryStream(bytes));
            bitmap.SetSource(stream);
            return bitmap;
        }

        public static IRandomAccessStream ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var wrStr = outputStream.AsStreamForWrite();
            wrStr.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            wrStr.Flush();
            return randomAccessStream;
        }
    }
}
