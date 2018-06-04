using ServiceBridge.distributed.zookeeper.ServiceManager;
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
            new ServiceBridge.distributed.zookeeper.ServiceManager.ServiceSubscribe("***:2181"));

        /// <summary>
        /// 定义一个客户端
        /// </summary>
        class UserServiceClient : ServiceBridge.rpc.ServiceClient<Wcf.Contract.IUserService>
        {
            public UserServiceClient() : base(sub.Value.ResolveSvc<Wcf.Contract.IUserService>())
            { }
        }

        static void Main(string[] args)
        {
            foreach (var i in ServiceBridge.helper.Com.Range(100))
            {
                using (var con = new ServiceSubscribe("es.qipeilong.net:2181"))
                {
                    con.OnSubscribeFinishedAsync += async () =>
                    {
                        Console.WriteLine("订阅完成");
                        await Task.FromResult(1);
                    };
                    con.ResolveSvc<Wcf.Contract.IUserService>();
                    Console.WriteLine("订阅服务" + con.AllService().Count());
                }
            }
            //取消监听
            sub.Value.Dispose();
        }
    }
}
