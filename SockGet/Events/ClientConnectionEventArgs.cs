using SockGet.Client;
using SockGet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class ClientConnectionEventArgs : ClientConnectionEventArgs<object>
    {
        public ClientConnectionEventArgs(SgClient<object> client, string reason = null) : base(client, reason)
        {
        }
    }
    public class ClientConnectionEventArgs<T> : DisconnectedEventArgs
    {
        public ClientConnectionEventArgs(SgClient<T> client , string reason = null) : base(reason)
        {
            Client = client;
        }
        public SgClient<T> Client { get; }
    }
}
