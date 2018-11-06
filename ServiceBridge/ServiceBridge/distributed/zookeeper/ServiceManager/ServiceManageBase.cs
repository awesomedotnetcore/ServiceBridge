using ServiceBridge.data;
using ServiceBridge.extension;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// /QPL/WCF/ORDER/m-1
    /// /QPL/WCF/ORDER/m-2
    /// /QPL/WCF/ORDER/m-3
    /// /QPL/WCF/ORDER/m-4
    /// </summary>
    public abstract class ServiceManageBase : AlwaysOnZooKeeperClient
    {
        protected readonly string _base_path;
        protected readonly int _base_path_level;
        protected readonly int _service_path_level;
        protected readonly int _endpoint_path_level;

        protected readonly SerializeHelper _serializer = new SerializeHelper();

        public ServiceManageBase(string host) : this(host, "/QPL/WCF") { }

        public ServiceManageBase(string host, string path) : base(host)
        {
            this._base_path = path ?? throw new ArgumentNullException(path);
            if (!this._base_path.StartsWith("/") || this._base_path.EndsWith("/"))
            {
                throw new Exception("path必须以/开头，并且不能以/结尾");
            }
            this._base_path_level = this._base_path.SplitZookeeperPath().Count;
            this._service_path_level = this._base_path_level + 1;
            this._endpoint_path_level = this._service_path_level + 1;

            //链接上了初始化root目录
            this.OnConnectedAsync += this.InitBasePath;
        }

        protected async Task InitBasePath()
        {
            try
            {
                var client = this.GetClientManager();
                await this.RetryAsync(async () => await client.EnsurePath(this._base_path));
            }
            catch (Exception e)
            {
                var err = new Exception("尝试创建服务注册base path失败", e);
                err.AddErrorLog();
            }
        }

        protected async Task RetryAsync(Func<Task> action)
        {
            var retry_count = 3;
            while (true)
            {
                try
                {
                    await action.Invoke();
                    break;
                }
                catch when (--retry_count > 0)
                {
                    //continue
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        protected bool IsServiceRootLevel(string path) =>
            path.SplitZookeeperPath().Count == this._base_path_level;

        protected bool IsServiceLevel(string path) =>
            path.SplitZookeeperPath().Count == this._service_path_level;

        protected bool IsEndpointLevel(string path) =>
            path.SplitZookeeperPath().Count == this._endpoint_path_level;

        protected void GetServiceAndEndpointNodeName(string path, out string service_name, out string endpoint_name)
        {
            if (!this.IsEndpointLevel(path)) { throw new Exception("只有终结点才能获取服务和节点信息"); }

            var data = path.SplitZookeeperPath().Reverse_();
            endpoint_name = data.Take(1).FirstOrDefault();
            service_name = data.Skip(1).Take(1).FirstOrDefault();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
