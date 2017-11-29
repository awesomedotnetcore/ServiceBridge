using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Wcf.Contract
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        string GetUserName(string uid);
    }
}
