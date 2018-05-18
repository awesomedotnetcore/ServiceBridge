using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 写到zk中的json实体
    /// </summary>
    public class AddressModel
    {
        public virtual string ServiceNodeName { get; set; }

        public virtual string EndpointNodeName { get; set; }

        public virtual string FullPathName { get => $"{this.ServiceNodeName}/{this.EndpointNodeName}"; }

        public virtual string Url { get; set; }

        public virtual int Weight { get; set; } = 1;

        public virtual DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// wcf中协议和服务地址的容器
    /// </summary>
    public class ContractModel
    {
        public ContractModel(Type contract, string url)
        {
            this.Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public virtual Type Contract { get; private set; }

        public virtual string Url { get; private set; }
    }
}
