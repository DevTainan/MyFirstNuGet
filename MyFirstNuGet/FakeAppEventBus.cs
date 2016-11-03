using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyFirstNuGet
{
    /// <summary>
    /// 通知管理員
    /// </summary>
    internal class AppEventBusManager
    {
        private static AppEventBusManager instance = null;
        private static readonly object padlock = new object();

        private AppEventBusManager()
        {
            Init();
        }

        private void Init()
        {
            lstObservers = new Dictionary<string, IObserver>();
        }

        public static AppEventBusManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new AppEventBusManager();
                        }
                    }
                }
                return instance;
            }
        }

        Dictionary<string, IObserver> lstObservers; // 使用List來存放觀察者名單

        // 訂閱
        public void Subscribe(string id, IObserver pObserver)
        {
            lstObservers.Add(id, pObserver);
        }

        // 取消訂閱
        public void Unsubscribe(string id)
        {
            if (lstObservers.ContainsKey(id))
            {
                lstObservers.Remove(id);
            }
        }

        // 發送新消息
        public void Send(string pContent)
        {
            //Console.WriteLine("Send News..");
            notifyObservers(pContent);
        }

        // 發送通知給在監聽名單中的觀察者
        public void notifyObservers(string pContent)
        {
            foreach (IObserver observer in lstObservers.Values)
            {
                observer.Notify(pContent);
            }
        }
    }

    // 觀察者介面
    public interface IObserver
    {
        void Notify(string pMessage);
        event EventHandler<NotifyMessageArgs> OnMessageReceived;
    }

    /// <summary>
    /// 觀察者
    /// </summary>
    public class AppEventBus : IObserver
    {
        public string Id { get; private set; }
        private AppEventBusManager cv_AppEventBusManager = AppEventBusManager.Instance;
        public event EventHandler<NotifyMessageArgs> OnMessageReceived;

        /// <summary>
        /// 建構式, 訂閱
        /// </summary>
        public AppEventBus()
        {
            Id = Guid.NewGuid().ToString();
            cv_AppEventBusManager.Subscribe(Id, this);
        }

        /// <summary>
        /// 解構式, 取消訂閱
        /// </summary>
        ~AppEventBus()
        {
            cv_AppEventBusManager.Unsubscribe(Id);
        }

        // 這邊是個缺點...居然是公開方法!!
        // 收到消息
        public void Notify(string pMessage)
        {
            if (OnMessageReceived == null)
            {
                return;
            }

            var arg = new NotifyMessageArgs
            {
                Id = this.Id,
                Message = pMessage,
            };
            OnMessageReceived(this, arg);
        }

        // 發送新消息
        public void Send(string pContent)
        {
            cv_AppEventBusManager.Send(pContent);
        }
    }

    public class NotifyMessageArgs : EventArgs
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }
}
