# ServiceBridge
基于zookeeper的服务注册/发现/负载均衡

`1-服务端自动上报调用地址，客户端自动发现调用地址，从此WCF不用配置一行配置文件，也不用添加web引用。`

`2-多台服务器上线还可以实现基于权重的负载均衡。`

目前代码在lib项目中：

https://github.com/hiwjcn/hiwjcn/tree/master/Lib/distributed/zookeeper

计划稳定后剥到这个项目中

    Install-Package lib.com.ServiceBridge.nuget

# 服务端

``` c#
//标记这是一个服务，接口放在单独项目打包发布到nuget
[ServiceBridge.rpc.ServiceContractImpl]
public class ProductSearch : IProductSearch
{
//服务实现
}

//服务地址
var host = string.IsNullOrEmpty(this.base_url) ? Dns.GetHostName() : this.base_url;
//定义要查找的程序集
var target_assembly = typeof(QPL.WebService.Product.Service.UserProductService).Assembly;
//使用servicehost启动服务
ServiceHostManager.Host.StartService($"http://{host}:{this.host_port}/", target_assembly);
//注册服务
this._reg = new ServiceRegister(this.zookeeper_constring,
() => ServiceHostManager.Host.GetContractInfo().Select(x => new ContractModel(x.contract, x.url)).ToList());
```

# 客户端

``` c#
    //订阅服务
    public class ServiceSubManager : IDisposable
    {
        public static readonly ServiceSubManager Instance = new ServiceSubManager();

        public readonly Lazy_<ServiceSubscribe> _lz = new Lazy_<ServiceSubscribe>(() =>
        new ServiceSubscribe(ConfigurationManager.ConnectionStrings["ZK"]?.ConnectionString ?? throw new Exception("请配置zookeeper")))
            .WhenDispose((ref ServiceSubscribe x) => x.Dispose());

        public void Dispose()
        {
            if (this._lz.IsValueCreated)
            {
                this._lz.Dispose();
            }
        }
    }
    
    //定义服务base类
    public class AutoServiceClient<T> : ServiceClient<T> where T : class
    {
        public AutoServiceClient() :
            base(ServiceSubManager.Instance._lz.Value.ResolveSvc<T>() ?? throw new Exception($"{typeof(T)}服务已下线"))
        {
            //
        }
    }
    
    //定义client
    public class ProductSearchServiceClient : AutoServiceClient<IProductSearch> { }
    
    //使用客户端
    using(var client=new ProductSearchServiceClient())
    {
        client.instance.search(params);
    }
```
