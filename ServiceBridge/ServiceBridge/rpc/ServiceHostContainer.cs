using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ServiceBridge.rpc
{
    public class ServiceHostContainer : IDisposable
    {
        public int? MaxItemsInObjectGraph { get; set; }
        public int? MaxBufferSize { get; set; }
        public int? MaxRecieveMessageSize { get; set; }
        public TimeSpan? OpenTimeout { get; set; }
        public TimeSpan? CloseTimeout { get; set; }
        public TimeSpan? SendTimeout { get; set; }
        public TimeSpan? RecieveTimeout { get; set; }
        /// <summary>
        /// event
        /// </summary>
        public event Action<ServiceHost, Type, string> OnContractAdded;
        public event Action<ServiceHost, Type, BasicHttpBinding> OnBindingCreated;
        //host container
        private readonly List<ServiceHost> _hosts = new List<ServiceHost>();

        public List<(Type contract, string url)> GetContractInfo()
        {
            var data = new List<(Type contract, string url)>();

            foreach (var host in this._hosts)
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
            if (ValidateHelper.IsPlumpList(this._hosts)) { throw new Exception("服务已经启动"); }

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
                            var binding = new BasicHttpBinding()
                            {
                                OpenTimeout = this.OpenTimeout ?? TimeSpan.FromSeconds(30),
                                CloseTimeout = this.CloseTimeout ?? TimeSpan.FromSeconds(30),
                                SendTimeout = this.SendTimeout ?? TimeSpan.FromSeconds(30),
                                ReceiveTimeout = this.RecieveTimeout ?? TimeSpan.FromSeconds(30),
                                MaxBufferSize = this.MaxBufferSize ?? 2147483647,
                                MaxReceivedMessageSize = this.MaxRecieveMessageSize ?? 2147483647
                            };

                            this.OnBindingCreated?.Invoke(host, c, binding);
                            host.AddServiceEndpoint(c, binding, c.Name);
                            this.OnContractAdded?.Invoke(host, c, c.Name);
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
                            dataContractBehavior.MaxItemsInObjectGraph = this.MaxItemsInObjectGraph ?? 65536000;
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
                        this._hosts.Add(host);
                    }
                }
                return ValidateHelper.IsPlumpList(this._hosts);
            }
            catch (Exception e)
            {
                this.Dispose();
                throw new Exception("一个或多个服务启动失败，已经销毁所有已经启动的服务", e);
            }
        }

        public void Dispose()
        {
            lock (this._hosts)
            {
                foreach (var s in this._hosts)
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
                this._hosts.Clear();
            }
        }
    }
}
