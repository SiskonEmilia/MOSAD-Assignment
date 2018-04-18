using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Web.Syndication;

namespace BackgroundTask
{
    public sealed class EmiliaSiskonBGT : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 获取一个Deferral(What's this?)，以防止这个任务
            // 当异步方法还在执行的时候提前关闭
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            // 获取更新内容
            

            // 更新磁贴

            // 通知系统后台任务搞定啦~
            deferral.Complete();
        }
    }
}
