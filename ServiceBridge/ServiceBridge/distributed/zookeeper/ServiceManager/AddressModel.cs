using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    public class AddressModel
    {
        public virtual string ServiceNodeName { get; set; }

        public virtual string EndpointNodeName { get; set; }

        public virtual string FullPathName { get => $"{this.ServiceNodeName}/{this.EndpointNodeName}"; }

        public virtual string Url { get; set; }
    }
}
