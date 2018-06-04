using org.apache.zookeeper;
using ServiceBridge.distributed.zookeeper.watcher;
using ServiceBridge.extension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper
{
    /// <summary>
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : IDisposable
    {
        protected bool IsDisposing = false;
        private ZooKeeper _client;
        //param
        protected readonly string _host;
        protected readonly TimeSpan _timeout;
        protected readonly Watcher _connection_status_watcher;
        //zk是否可用的信号量
        private readonly ManualResetEvent _client_lock = new ManualResetEvent(false);
        //创建zk的锁
        private readonly object _create_client_lock = new object();

        /// <summary>
        /// 链接成功
        /// </summary>
        public event Func<Task> OnConnectedAsync;

        /// <summary>
        /// 链接丢失，zk将自动重试链接
        /// </summary>
        public event Func<Task> OnUnConnectedAsync;

        /// <summary>
        /// session过期，zk将放弃链接
        /// </summary>
        public event Func<Task> OnSessionExpiredAsync;

        /// <summary>
        /// 捕获到异常
        /// </summary>
        public event Action<Exception> OnError;

        public ZooKeeperClient(ZooKeeperConfigSection configuration) :
            this(configuration.Server, TimeSpan.FromMilliseconds(configuration.SessionTimeOut))
        {
            //
        }

        public ZooKeeperClient(string host, TimeSpan? timeout = null)
        {
            this._host = host ?? throw new ArgumentNullException(nameof(host));
            this._timeout = timeout ?? TimeSpan.FromSeconds(30);
            //监控zk的状态
            this._connection_status_watcher = new ConnectionStatusWatcher(async status =>
            {
                this._client_lock.Reset();
                switch (status)
                {
                    case Watcher.Event.KeeperState.SyncConnected:
                    case Watcher.Event.KeeperState.ConnectedReadOnly:
                        //服务可用
                        this._client_lock.Set();
                        if (this.OnConnectedAsync != null) { await this.OnConnectedAsync.Invoke(); }
                        break;

                    case Watcher.Event.KeeperState.Disconnected:
                        //链接丢失，等待再次连接
                        if (this.OnUnConnectedAsync != null) { await this.OnUnConnectedAsync.Invoke(); }
                        break;

                    case Watcher.Event.KeeperState.Expired:
                        //session过期，重新创建客户端
                        if (this.OnSessionExpiredAsync != null) { await this.OnSessionExpiredAsync.Invoke(); }
                        break;

                    case Watcher.Event.KeeperState.AuthFailed:
                        //验证错误没必要再试尝试
                        this.Dispose();
                        var e = new Exception("zk auth验证失败，已经销毁所有链接");
                        e.AddErrorLog();
                        throw e;
                }
            });

            //这里注释掉，让用户手动调用
            //this.CreateClient();
        }

        public virtual void CreateClient()
        {
            if (this._client == null)
            {
                lock (this._create_client_lock)
                {
                    if (this._client == null)
                    {
                        this._client = new ZooKeeper(
                            this._host,
                            (int)this._timeout.TotalMilliseconds,
                            this._connection_status_watcher);
                    }
                }
            }
        }

        public ZooKeeper Client { get => this.GetClientManager(); }

        /// <summary>
        /// 等待可用链接，默认30秒超时
        /// </summary>
        /// <returns></returns>
        public virtual ZooKeeper GetClientManager(TimeSpan? timeout = null)
        {
            try
            {
                //等待连上
                this._client_lock.WaitOneOrThrow(timeout ?? TimeSpan.FromSeconds(30), "无法链接zk");

                if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }

                return this._client;
            }
            catch (KeeperException.ConnectionLossException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw new Exception("zk链接丢失", e);
            }
            catch (KeeperException.SessionExpiredException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw new Exception("zk会话丢失", e);
            }
            catch (Exception e)
            {
                this.OnError?.Invoke(e);
                throw e;
            }
        }

        public virtual void CloseClient()
        {
            try
            {
                if (this._client != null)
                {
                    Task.Run(async () => await this._client.closeAsync()).Wait();
                }
            }
            catch (Exception e)
            {
                this.OnError?.Invoke(e);
                e.AddErrorLog("关闭zk客户端失败");
            }
            finally
            {
                //关闭信号
                this._client_lock.Reset();
                this._client = null;
            }
        }

        public virtual void Dispose()
        {
            this.IsDisposing = true;

            this.CloseClient();
            this._client_lock.Dispose();
        }
    }
}
