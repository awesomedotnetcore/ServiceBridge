using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using ServiceBridge.distributed.zookeeper;
using Polly;
using ServiceBridge.rpc;
using ServiceBridge.helper;
using org.apache.zookeeper;
using ServiceBridge.extension;
using System.ServiceModel;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly string _node_id;
        private readonly Func<List<ContractModel>> _contracts;

        public ServiceRegister(string host, Func<List<ContractModel>> _contracts) : base(host)
        {
            this._contracts = _contracts ?? throw new ArgumentNullException(nameof(_contracts));

            this._node_id = this.Client.getSessionId().ToString();

            this.Retry().Execute(() => this.Reg());
            this.OnRecconected += () => this.Reg();
        }

        public void Reg() => AsyncHelper_.RunSync(() => this.RegisterService());

        private async Task RegisterService()
        {
            var list = new List<AddressModel>();
            foreach (var m in this._contracts.Invoke())
            {
                var model = new AddressModel()
                {
                    Url = m.Url,
                    ServiceNodeName = ServiceManageHelper.ParseServiceName(m.Contract),
                    EndpointNodeName = ServiceManageHelper.EndpointNodeName(this._node_id),
                };
                list.Add(model);
            }

            foreach (var m in list)
            {
                var service_path = this._base_path + "/" + m.ServiceNodeName;
                await this.Client.EnsurePath(service_path);

                var path = service_path + "/" + m.EndpointNodeName;
                var data = this._serializer.Serialize(m);
                if (await this.Client.ExistAsync_(path))
                {
                    await this.Client.SetDataAsync_(path, data);
                }
                else
                {
                    //创建临时节点，服务端下线自动删除
                    await this.Client.CreateNode_(path, CreateMode.EPHEMERAL, data);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
