using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Exceptions
{
    public class ConnectionTimeoutException : Exception
    {
        public ConnectionTimeoutException(string message) : base(message)
        {

        }
    }
}
