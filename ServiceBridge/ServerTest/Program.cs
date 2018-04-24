using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    /// <summary>
    /// 去debug里找到exe文件右键以管理员权限启动
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBridge.distributed.zookeeper.ServiceManager.ServiceRegister reg = null;
            try
            {
                ServiceBridge.rpc.ServiceHostManager.StartService("http://localhost:10000/", typeof(Program).Assembly);
                reg = new ServiceBridge.distributed.zookeeper.ServiceManager.ServiceRegister("es.qipeilong.net:2181", () => ServiceBridge.rpc.ServiceHostManager.GetContractInfo().Select(x => new ServiceBridge.distributed.zookeeper.ServiceManager.ContractModel(x.contract, x.url)).ToList());

                Console.WriteLine("服务已经启动，按任意键退出");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                reg?.Dispose();
                ServiceBridge.rpc.ServiceHostManager.DisposeService();
            }
        }
    }
}
