using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using Windows.Data.Xml.Dom;

namespace Lab1
{
    public class TileController
    {
        private string title;
        private string detail;
        private DateTime date;
        static TileController _instance;

        private TileController()
        {
            // enable the notification queue
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            Update();
        } // constructor

        public static TileController GetInstance()
        {
            if (_instance == null)
                _instance = new TileController();
            return _instance;
        }

        public void Update()
        {
            var count = App.ViewModel.Memos.Count;
            Memorandum memo;
            for (int i = 0; i <  count && i < 5; ++i)
            {
                memo = App.ViewModel.Memos[i];
                Add(memo.MemoTitle, memo.MemoDetail, memo.MemoDate, i);
            }
        }

        private void Add(string title, string detail, DateTime date, int index)
        {
            this.title = title;
            this.detail = detail;
            this.date = date;
            var notification = new TileNotification(GetMyContent().GetXml());
            notification.Tag = index.ToString();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        private TileContent GetMyContent()
        {
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {

                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/bg1a 2.jpg",
                                HintOverlay = 20
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                }
                            }
                        }
                    } ,
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {

                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/bg1a 2.jpg",
                                HintOverlay = 20
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },

                                new AdaptiveText()
                                {
                                    Text = date.ToShortDateString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = detail,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }, // Mid-size

                    TileWide = new TileBinding()
                    {

                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/bg1a 2.jpg",
                                HintOverlay = 20
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },

                                new AdaptiveText()
                                {
                                    Text = date.ToShortDateString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = detail,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }, // Wide-size
                    TileLarge = new TileBinding()
                    {

                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/bg1a 2.jpg",
                                HintOverlay = 20
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },

                                new AdaptiveText()
                                {
                                    Text = date.ToShortDateString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = detail,
                                    HintWrap = true,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    } // Wide-size
                }
            }; // construct the content template

            return content;
        }
    }
}
