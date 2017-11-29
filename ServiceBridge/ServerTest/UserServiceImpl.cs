using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    [Lib.rpc.ServiceContractImpl]
    public class UserServiceImpl : Wcf.Contract.IUserService
    {
        public string GetUserName(string uid) => $"username for uid:{uid}";
    }
}
