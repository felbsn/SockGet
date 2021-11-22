using SockGet.Client;
using SockGet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class ClientConnectionEventArgs : DisconnectedEventArgs
    {
        public ClientConnectionEventArgs(SgClient client , string reason = null) : base(reason)
        {
            Client = client;
        }
        public SgClient Client { get; }
    }
}
