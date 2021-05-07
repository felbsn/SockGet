using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Exceptions
{
    class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException(string message) : base(message)
        {

        }
    }
}
