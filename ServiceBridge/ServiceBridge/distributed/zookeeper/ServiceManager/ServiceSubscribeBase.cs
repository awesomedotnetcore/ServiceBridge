using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    public abstract class ServiceSubscribeBase : ServiceManageBase
    {
        protected readonly List<AddressModel> _endpoints = new List<AddressModel>();
        protected readonly Random _ran = new Random((int)DateTime.Now.Ticks);
        
        public ServiceSubscribeBase(string host) : base(host) { }

        public IReadOnlyList<AddressModel> AllService() => this._endpoints.AsReadOnly();

        public AddressModel Resolve<T>()
        {
            var name = ServiceManageHelper.ParseServiceName<T>();
            var list = this._endpoints.Where(x => x.ServiceNodeName == name).ToList();
            if (ValidateHelper.IsPlumpList(list))
            {
                var theone = this._ran.Choice(list);
                //
                //根据权重选择
                //this._ran.ChoiceByWeight(list, x => x.Weight);
                Console.WriteLine($"选择了地址：{theone.Url}");
                return theone;
            }
            return null;
        }

        public string ResolveSvc<T>() => this.Resolve<T>()?.Url;
    }
}
