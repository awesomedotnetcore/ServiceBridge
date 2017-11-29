using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Wcf.Contract
{
    /// <summary>
    /// 在server端直接引用这个项目
    /// 在client端需要先把这个项目发布nuget，再引用（其实也是可以拷过去，但是项目太大会比较麻烦）
    /// </summary>
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        string GetUserName(string uid);
    }
}
