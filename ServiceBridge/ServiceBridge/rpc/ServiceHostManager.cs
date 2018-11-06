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
        
    }
}
