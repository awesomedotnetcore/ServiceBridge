using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    /// <summary>
    /// 客户端只要配置zk地址
    /// </summary>
    class Program
    {
        private static readonly Lazy<ServiceBridge.distributed.zookeeper.ServiceManager.ServiceSubscribe> sub =
            new Lazy<ServiceBridge.distributed.zookeeper.ServiceManager.ServiceSubscribe>(() =>
            new ServiceBridge.distributed.zookeeper.ServiceManager.ServiceSubscribe("es.qipeilong.net:2181"));

        class UserServiceClient : ServiceBridge.rpc.ServiceClient<Wcf.Contract.IUserService>
        {
            public UserServiceClient() : base(sub.Value.ResolveSvc<Wcf.Contract.IUserService>())
            { }
        }

        static void Main(string[] args)
        {
            foreach (var i in ServiceBridge.helper.Com.Range(100))
            {
                while (sub.Value.Resolve<Wcf.Contract.IUserService>() == null)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("等待服务上线");
                }
                try
                {
                    System.Threading.Thread.Sleep(1000);
                    using (var client = new UserServiceClient())
                    {
                        var name = client.Instance.GetUserName("123");
                        Console.WriteLine($"服务返回数据：{name}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            sub.Value.Dispose();
        }
    }
}
