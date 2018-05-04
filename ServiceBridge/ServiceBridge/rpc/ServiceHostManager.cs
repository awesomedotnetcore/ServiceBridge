using ServiceBridge.core;
using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;

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

        public static bool StartService(string base_url, params Assembly[] ass) =>
            _lazy.Value.StartService(base_url, ass);

        public static void DisposeService()
        {
            if (_lazy.IsValueCreated)
            {
                lock (_lazy)
                {
                    if (_lazy.IsValueCreated)
                    {
                        _lazy.Value.Dispose();
                    }
                }
            }
        }
    }

    public class ServiceHostContainer : IDisposable
    {
        private readonly List<ServiceHost> _hosts = new List<ServiceHost>();

        public List<(Type contract, string url)> GetContractInfo()
        {
            var data = new List<(Type contract, string url)>();

            foreach (var host in _hosts)
            {
                foreach (var ep in host.Description.Endpoints)
                {
                    var c = ep.Contract?.ContractType;
                    var u = ep.Address?.Uri?.AbsoluteUri;
                    if (c == null || !ValidateHelper.IsPlumpString(u)) { continue; }

                    data.Add((c, u));
                }
            }

            return data;
        }

        public bool StartService(string base_url, params Assembly[] ass)
        {
            if (ValidateHelper.IsPlumpList(_hosts)) { throw new Exception("服务已经启动"); }

            base_url = base_url ?? string.Empty;
            if (!base_url.EndsWith("/")) { throw new Exception("base_url必须以/结尾"); }
            if (!ValidateHelper.IsPlumpList(ass)) { throw new ArgumentNullException(nameof(ass)); }

            try
            {
                foreach (var a in ass)
                {
                    foreach (var service in a.FindServiceContractsImpl())
                    {
                        var contracts = service.FindServiceContracts();
                        if (!ValidateHelper.IsPlumpList(contracts)) { continue; }

                        var host = new ServiceHost(service, new Uri(base_url + service.Name));
                        foreach (var c in contracts)
                        {
                            host.AddServiceEndpoint(c, new BasicHttpBinding(), c.Name);
                        }

                        var metaBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                        if (metaBehavior != null)
                        {
                            metaBehavior.HttpGetEnabled = true;
                            metaBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                        }
                        else
                        {
                            metaBehavior = new ServiceMetadataBehavior();

                            metaBehavior.HttpGetEnabled = true;
                            metaBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

                            host.Description.Behaviors.Add(metaBehavior);
                        }

                        var dataContractBehavior = host.Description.Behaviors.Find<DataContractSerializerOperationBehavior>();
                        if (dataContractBehavior != null)
                        {
                            dataContractBehavior.MaxItemsInObjectGraph = 65536000;
                        }

                        var debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                        if (debugBehavior != null)
                        {
                            debugBehavior.IncludeExceptionDetailInFaults = true;
                        }
                        else
                        {
                            debugBehavior = new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true };
                            host.Description.Behaviors.Add(debugBehavior);
                        }

                        host.Open();
                        _hosts.Add(host);
                    }
                }
                return ValidateHelper.IsPlumpList(_hosts);
            }
            catch (Exception e)
            {
                this.Dispose();
                throw new Exception("一个或多个服务启动失败，已经销毁所有已经启动的服务", e);
            }
        }

        public void Dispose()
        {
            lock (_hosts)
            {
                foreach (var s in _hosts)
                {
                    try
                    {
                        s.Close();
                        ((IDisposable)s).Dispose();
                    }
                    catch (Exception e)
                    {
                        e.AddErrorLog();
                    }
                }
                _hosts.Clear();
            }
        }
    }
}
