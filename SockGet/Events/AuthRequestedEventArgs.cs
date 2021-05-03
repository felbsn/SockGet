using SockGet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class AuthRequestedEventArgs : EventArgs
    {
        public AuthRequestedEventArgs(string authToken)
        {
            AuthToken = authToken;
        }
        public string AuthToken { get; private set; }
        public bool Reject{ get; set; }

        public Response Response { get; set; }
    }
}
