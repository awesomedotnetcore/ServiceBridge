using org.apache.zookeeper;
using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly Func<List<ContractModel>> _contracts;

        public ServiceRegister(string host, Func<List<ContractModel>> _contracts) : base(host)
        {
            this._contracts = _contracts ?? throw new ArgumentNullException(nameof(_contracts));

            //链接成功后调用注册
            this.OnConnectedAsync += this.Reg;
            //尝试打开链接
            this.CreateClient();
        }

        public async Task Reg()
        {
            try
            {
                await this.RetryAsync(async () => await this.RegisterService());
            }
            catch (Exception e)
            {
                var err = new Exception("注册服务失败", e);
                err.AddErrorLog();
            }
        }

        private async Task RegisterService()
        {
            var _node_id = this.Client.getSessionId().ToString();

            var list = this._contracts.Invoke().Select(x => new AddressModel()
            {
                Url = x.Url,
                ServiceNodeName = ServiceManageHelper.ParseServiceName(x.Contract),
                EndpointNodeName = ServiceManageHelper.EndpointNodeName(_node_id),
            }).ToList();

            var now = DateTime.Now;
            foreach (var m in list)
            {
                m.UpdateTime = now;

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
