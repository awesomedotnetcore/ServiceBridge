using org.apache.zookeeper;
using ServiceBridge.distributed.zookeeper.watcher;
using ServiceBridge.extension;
using ServiceBridge.helper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceSubscribe : ServiceSubscribeBase
    {
        private readonly Watcher _children_watcher;
        private readonly Watcher _node_watcher;

        public event Action OnServiceChanged;

        public ServiceSubscribe(string host) : base(host)
        {
            this._children_watcher = new CallBackWatcher(e =>
            {
                return this.WatchNodeChanges(e);
            });
            this._node_watcher = new CallBackWatcher(e =>
            {
                return this.WatchNodeChanges(e);
            });

            //链接上了就获取服务信息
            this.OnConnected += () => this.Init();
            //打开链接
            this.CreateClient();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            try
            {
                //清理无用节点
                Task.Factory.StartNew(async () =>
                {
                    await this.RetryAsync().ExecuteAsync(async () => await this.ClearDeadNodes());
                }).Wait();
            }
            catch (Exception e)
            {
                var err = new Exception("清理无用节点失败", e);
                err.AddErrorLog();
            }

            try
            {
                //读取节点并添加监视
                Task.Factory.StartNew(async () =>
                {
                    await this.RetryAsync().ExecuteAsync(async () => await this.WalkNodeAndWatch(this._base_path));
                }).Wait();
            }
            catch (Exception e)
            {
                var err = new Exception("订阅服务节点失败", e);
                err.AddErrorLog();
            }

            this._client_ready.Set();
        }

        /// <summary>
        /// 启动的时候清理一下无用节点
        /// 这个方法里不要watch
        /// </summary>
        private async Task ClearDeadNodes()
        {
            var services = await this.Client.GetChildrenOrThrow_(this._base_path);
            foreach (var service in services)
            {
                try
                {
                    var service_path = this._base_path + "/" + service;
                    var endpoints = await this.Client.GetChildrenOrThrow_(service_path);
                    if (!ValidateHelper.IsPlumpList(endpoints))
                    {
                        await this.Client.DeleteSingleNode_(service_path);
                    }
                }
                catch (Exception e)
                {
                    e.AddErrorLog("清理无用节点错误");
                }
            }
        }

        private async Task WalkNodeAndWatch(string path)
        {
            try
            {
                if (this.IsServiceRootLevel(path))
                {
                    //qpl/wcf
                    var services = await this.Client.GetChildrenOrThrow_(path, this._children_watcher);
                    foreach (var service in services.Where(x => ValidateHelper.IsPlumpString(x)))
                    {
                        var service_path = path + "/" + service;
                        await this.WalkNodeAndWatch(service_path);
                    }
                }
                else if (this.IsServiceLevel(path))
                {
                    //qpl/wcf/order
                    var endpoints = await this.Client.GetChildrenOrThrow_(path, this._children_watcher);
                    foreach (var endpoint in endpoints)
                    {
                        //处理节点
                        await this.GetEndpointData(path + "/" + endpoint);
                    }
                }
                else
                {
                    $"不能处理的节点{path}".AddBusinessInfoLog();
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog($"订阅节点{path}失败");
            }
        }

        private async Task GetEndpointData(string path)
        {
            if (!this.IsEndpointLevel(path)) { return; }
            try
            {
                var bs = await this.Client.GetDataOrThrow_(path, this._node_watcher);
                if (!ValidateHelper.IsPlumpList(bs))
                {
                    await this.Client.DeleteNodeRecursively_(path);
                    return;
                }
                var data = this._serializer.Deserialize<AddressModel>(bs) ??
                    throw new ArgumentNullException("address model");
                if (!ValidateHelper.IsAllPlumpString(data.ServiceNodeName, data.EndpointNodeName, data.Url))
                {
                    throw new Exception($"address model数据错误:{data.ToJson()}");
                }
                var service_info = this.GetServiceAndEndpointNodeName(path);
                data.ServiceNodeName = service_info.service_name;
                data.EndpointNodeName = service_info.endpoint_name;

                this._endpoints.RemoveWhere_(x => x.FullPathName == data.FullPathName);
                this._endpoints.Add(data);
                this.OnServiceChanged?.Invoke();
            }
            catch (Exception e)
            {
                e.AddErrorLog($"读取节点数据失败：{path}");
            }
        }

        private async Task DeleteEndpoint(string path)
        {
            if (!this.IsEndpointLevel(path)) { return; }
            var data = this.GetServiceAndEndpointNodeName(path);

            this._endpoints.RemoveWhere_(
                x => x.ServiceNodeName == data.service_name && x.EndpointNodeName == data.endpoint_name);

            this.OnServiceChanged?.Invoke();

            await Task.FromResult(1);
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task WatchNodeChanges(WatchedEvent e)
        {
            var event_type = e.get_Type();
            var path = e.getPath();

            var path_level = path.SplitZookeeperPath().Count;
            if (path_level < this._base_path_level || path_level > this._endpoint_path_level)
            {
                $"节点无法被处理{path}".AddBusinessInfoLog();
                return;
            }

            //Console.WriteLine($"节点事件：{path}:{event_type}");

            switch (event_type)
            {
                case Watcher.Event.EventType.NodeChildrenChanged:
                    //子节点发生更改
                    await this.WalkNodeAndWatch(path);
                    break;

                case Watcher.Event.EventType.NodeDataChanged:
                    //单个节点数据发生修改
                    await this.GetEndpointData(path);
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                    //单个节点被删除
                    await this.DeleteEndpoint(path);
                    break;

                default:
                    break;
            }
        }

    }
}
