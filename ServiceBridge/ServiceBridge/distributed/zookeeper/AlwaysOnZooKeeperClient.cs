using System;

namespace ServiceBridge.distributed.zookeeper
{
    public class AlwaysOnZooKeeperClient : ZooKeeperClient
    {
        /// <summary>
        /// 尝试再次链接，也许还没连上
        /// </summary>
        public event Action OnRecconecting;

        public AlwaysOnZooKeeperClient(string host) : base(host)
        {
            //只有session过期才重新创建client，否则等待client自动尝试重连
            this.OnSessionExpired += () => this.ReConnect();
        }

        protected virtual void ReConnect()
        {
            if (this.IsDisposing)
            {
                //销毁的时候取消重试链接
                return;
            }

            this.CloseClient();
            this.CreateClient();
            this.OnRecconecting?.Invoke();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
