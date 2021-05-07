using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Exceptions
{
    class HeaderReceiveException : Exception
    {
        public HeaderReceiveException(string message) : base(message)
        {

        }
    }
}
