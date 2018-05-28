using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    public abstract class ServiceSubscribeBase : ServiceManageBase
    {
        protected readonly ManualResetEvent _client_ready = new ManualResetEvent(false);

        protected readonly List<AddressModel> _endpoints = new List<AddressModel>();
        protected readonly Random _ran = new Random((int)DateTime.Now.Ticks);

        public event Action<AddressModel> OnServerSelected;

        public ServiceSubscribeBase(string host) : base(host) { }

        public IReadOnlyList<AddressModel> AllService() => this._endpoints.AsReadOnly();

        public AddressModel Resolve<T>(TimeSpan timeout)
        {
            this._client_ready.WaitOneOrThrow(timeout, $"超时：{timeout}，客户端尚未完成服务订阅");

            var name = ServiceManageHelper.ParseServiceName<T>();
            var list = this._endpoints.Where(x => x.ServiceNodeName == name).ToList();
            if (ValidateHelper.IsPlumpList(list))
            {
                //这里用thread local比较好，一个线程共享一个随机对象
                lock (this._ran)
                {
                    var theone = this._ran.Choice(list) ??
                        throw new Exception("server information is empty");
                    //根据权重选择
                    //this._ran.ChoiceByWeight(list, x => x.Weight);
                    this.OnServerSelected?.Invoke(theone);
                    return theone;
                }
            }
            return null;
        }

        public string ResolveSvc<T>(TimeSpan? timeout = null) =>
            this.Resolve<T>(timeout ?? TimeSpan.FromSeconds(20))?.Url;
    }
}
