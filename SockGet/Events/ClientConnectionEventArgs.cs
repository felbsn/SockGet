using SockGet.Client;
using SockGet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public ClientConnectionEventArgs(SGClient client)
        {
            Client = client;
        }

        public SGClient Client { get; }
    }
}
