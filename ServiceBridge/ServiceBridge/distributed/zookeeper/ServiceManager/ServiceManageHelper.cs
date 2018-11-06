using ServiceBridge.extension;
using System;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    public static class ServiceManageHelper
    {
        public static string ParseServiceName<T>() => ParseServiceName(typeof(T));

        public static string ParseServiceName(Type t) => $"{t.FullName}".RemoveWhitespace();

        public static string EndpointNodeName(string node_id) => $"node_{node_id}";
    }
}
