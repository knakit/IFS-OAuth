using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knakit.Ifs.Auth.Openid
{
    class Util
    {
    }

    internal class AuthResult
    {
        public AuthResultType ResultType { get; set; }
        public string Response { get; set; }
    }

    internal enum AuthResultType
    {
        Success,
        Error,
        UserCancel
    }
}
