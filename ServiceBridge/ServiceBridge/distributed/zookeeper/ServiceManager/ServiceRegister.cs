using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Lib.distributed.zookeeper;
using Polly;
using Lib.rpc;
using Lib.helper;
using org.apache.zookeeper;
using Lib.extension;
using System.ServiceModel;

namespace Lib.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly string _node_id;

        public ServiceRegister(string host) : base(host)
        {
            this._node_id = this.Client.getSessionId().ToString();

            this.Retry().Execute(() => this.Reg());
            this.OnRecconected += () => this.Reg();
        }

        public void Reg() => AsyncHelper_.RunSync(() => this.RegisterService());

        private async Task RegisterService()
        {
            var list = new List<AddressModel>();
            foreach (var m in ServiceHostManager.GetContractInfo())
            {
                var model = new AddressModel()
                {
                    Url = m.url,
                    ServiceNodeName = ServiceManageHelper.ParseServiceName(m.contract),
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
