using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Lab1
{
    public class Memorandum : INotifyPropertyChanged
    {
        private static int MaxId = 0;
        public int Id;
        private string memoTitle = "";
        public string MemoTitle { get { return this.memoTitle; }
            set
            {
                this.memoTitle = value;
                NotifyPropertyChanged("MemoTitle");
            }
        }
        private string memoDetail = "";
        public string MemoDetail
        {
            get { return this.memoDetail; }
            set
            {
                this.memoDetail = value;
                NotifyPropertyChanged("MemoDetail");
            }
        }
        private DateTime memoDate = new DateTime();
        public DateTime MemoDate
        {
            get { return this.memoDate; }
            set
            {
                this.memoDate = value;
                NotifyPropertyChanged("MemoDate");
            }
        }
        private bool isDone = false;
        public bool IsDone
        {
            get { return this.isDone; }
            set
            {
                this.isDone = value;
                NotifyPropertyChanged("IsDone");
            }
        }
        private BitmapImage memoImg = new BitmapImage();
        public BitmapImage MemoImg
        {
            get { return this.memoImg; }
            set
            {
                this.memoImg = value;
                NotifyPropertyChanged("MemoImg");
            }
        }
        private byte[] bytes = null;
        public byte[] Bytes
        {
            get { return this.bytes; }
            set
            {
                this.bytes = value;
            }
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Memorandum() { this.Id = ++MaxId; }
        public Memorandum(int Id)
        {
            this.Id = Id;
            if (MaxId < Id)
                MaxId = Id;
        }

        public class MemorandumViewModel
        {
            private ObservableCollection<Memorandum> memos = new ObservableCollection<Memorandum>();

            //public event NotifyCollectionChangedEventHandler CollectionChanged;

            public ObservableCollection<Memorandum> Memos { get { return this.memos; }
            set {
                    memos = value;
                }
            }
            public MemorandumViewModel() { }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value is bool)
                return (bool)value;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool?)
            {
                return (bool)value;
            }
            return false;
        }
    }
}
