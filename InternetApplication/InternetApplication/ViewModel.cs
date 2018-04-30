using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InternetApplication
{
    class ViewModel
    {
        private ObservableCollection<Message> messages = new ObservableCollection<Message>();

        public ObservableCollection<Message> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                this.messages = value;
            }
        }

        public static ViewModel GetInstance(bool? token = null)
        {
            if (token == null)
            {
                if (_instance == null)
                {
                    _instance = new ViewModel();
                    _instance.messages.Add(new Message()
                    {
                        Name = "Emilia",
                        Published = DateTime.Now,
                        Comment = "哥哥你好！我是艾米莉娅~你可以向我随意提问哦。",
                        IsSelf = false
                    });
                }
                return _instance;
            }

            if (_instance2 == null)
            {
                _instance2 = new ViewModel();
                _instance2.messages.Add(new Message()
                {
                    Name = "IP Query",
                    Published = DateTime.Now,
                    Comment = "你好！我是 IP 查询助手，输入 IP 地址，我就可以告诉你它的信息哦。\n" +
                    "您也可以发送“本机IP”来查询本机 IP 地址。",
                    IsSelf = false
                });
            }
            return _instance2;
        }

        private static ViewModel _instance = null;
        private static ViewModel _instance2 = null;
        private ViewModel() {  }

        public void AddSelf(string content)
        {
            messages.Add(new Message()
            {
                Name = "Me",
                Published = DateTime.Now,
                Comment = content,
                IsSelf = true
            });
        }

        public void AddEmilia(string content)
        {
            messages.Add(new Message()
            {
                Name = (this.Equals(_instance2)) ? "IP Query" : "Emilia",
                Published = DateTime.Now,
                Comment = content,
                IsSelf = false
            });
        }

    }

    public class Message
    {
        public string Name { get; set; }

        public DateTime Published { get; set; }

        public string Comment { get; set; }

        public bool IsSelf { get; set; }
    }

    

    public class MessageItemDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if ((item as Message).IsSelf)
            {
                return App.Current.Resources["SelfMessageDataTemplate"] as DataTemplate;
            }
            else
            {
                return App.Current.Resources["MessageDataTemplate"] as DataTemplate;
            }
        }
    }
}
