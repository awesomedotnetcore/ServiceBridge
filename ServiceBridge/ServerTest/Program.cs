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
                ServiceBridge.rpc.ServiceHostManager.Host.StartService("http://localhost:10000/", typeof(Program).Assembly);
                reg = new ServiceBridge.distributed.zookeeper.ServiceManager.ServiceRegister("***:2181",
                    () => ServiceBridge.rpc.ServiceHostManager.Host.GetContractInfo());

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
                ServiceBridge.rpc.ServiceHostManager.Host.Dispose();
            }
        }
    }
}
