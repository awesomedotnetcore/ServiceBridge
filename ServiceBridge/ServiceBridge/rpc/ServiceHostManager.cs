using Polly;
using ServiceBridge.core;
using System;

namespace ServiceBridge.rpc
{
    /// <summary>
    /// self host wcf
    /// </summary>
    public static class ServiceHostManager
    {
        private static readonly Lazy_<ServiceHostContainer> _lazy =
            new Lazy_<ServiceHostContainer>(() => new ServiceHostContainer());

        public static ServiceHostContainer Host => _lazy.Value;

        /// <summary>
        /// 多次尝试
        /// </summary>
        /// <param name="retry_count"></param>
        /// <param name="sleepDuration"></param>
        /// <param name="action"></param>
        public static void Retry(int retry_count, Func<int, TimeSpan> sleepDuration, Action action) =>
            Policy
            .Handle<Exception>()
            .WaitAndRetry(retry_count, sleepDuration)
            .Execute(() => action.Invoke());
    }
}
