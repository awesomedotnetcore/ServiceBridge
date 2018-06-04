using System;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper
{
    public class AlwaysOnZooKeeperClient : ZooKeeperClient
    {
        /// <summary>
        /// 尝试再次链接，也许还没连上
        /// </summary>
        public event Func<Task> OnRecconectingAsync;

        public AlwaysOnZooKeeperClient(string host) : base(host)
        {
            //只有session过期才重新创建client，否则等待client自动尝试重连
            this.OnSessionExpiredAsync += this.ReConnect;
        }

        protected async Task ReConnect()
        {
            if (this.IsDisposing)
            {
                //销毁的时候取消重试链接
                return;
            }

            this.CloseClient();
            this.CreateClient();

            if (this.OnRecconectingAsync != null) { await this.OnRecconectingAsync.Invoke(); }

            await Task.FromResult(1);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
