using ServiceBridge.extension;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace ServiceBridge.rpc
{
    /// <summary>
    /// wcf请求client，直接使用using就可以正确关闭链接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    {
        public ServiceClient()
        {
            //read config
        }

        public ServiceClient(Binding binding, EndpointAddress endpoint) : base(binding, endpoint)
        {
            //manual config
        }

        public ServiceClient(Binding binding, string url) : this(binding, new EndpointAddress(url))
        {
            //
        }

        public ServiceClient(string url) :
            this(new BasicHttpBinding()
            {
                MaxBufferSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                MaxReceivedMessageSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                ReceiveTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.ReceiveTimeoutSecond"] ?? "20").ToInt(null)),
                SendTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.SendTimeoutSecond"] ?? "20").ToInt(null))
            }, new EndpointAddress(url))
        {
            //with default config
        }

        /// <summary>
        /// 接口实例
        /// </summary>
        public T Instance
        {
            get
            {
                return this.Channel;
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.SafeClose_();
        }
    }

    /// <summary>
    /// 直接使用，不用using
    /// </summary>
    public static class ServiceHelper<T> where T : class
    {
        /// <summary>
        /// 同步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperationResult<R> Invoke<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return client.Invoke(func);
            }
        }

        /// <summary>
        /// 同步转异步执行
        /// https://www.zhihu.com/question/56539006
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("使用Task.Run。调用时请确保是IO密集型操作，而不是计算密集型！具体参见此方法备注中的URL")]
        public static async Task<OperationResult<R>> InvokeSyncAsAsync<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeSyncAsAsync(func);
            }
        }

        /// <summary>
        /// 异步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<OperationResult<R>> InvokeAsync<R>(Func<T, Task<R>> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeAsync(func);
            }
        }
    }
}
