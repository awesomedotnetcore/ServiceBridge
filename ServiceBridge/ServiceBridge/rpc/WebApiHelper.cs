using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Polly;

namespace Lib.rpc
{
    public class ServerSetting
    {
        public string Server { get; set; }

        public byte Weight { get; set; }

        private long CallTimes { get; set; }

        /// <summary>
        /// 记录请求，重新计算权重
        /// </summary>
        public void LogRequest() { }
    }

    /// <summary>
    /// api调用，自动降权升权，按照权重选择服务器
    /// </summary>
    public class WebApiHelper
    {
        public WebApiHelper()
        {
            var server_set = new List<ServerSetting>();

            var ran = new Random((int)DateTime.Now.Ticks);
            var server = ran.ChoiceByWeight(server_set, x => x.Weight).Server;
        }

        public T Post<T>(string path, object param)
        {
            throw new NotImplementedException();
        }

    }
}
