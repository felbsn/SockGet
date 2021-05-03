using SockGet.Client;
using System;

namespace SockGet.Events
{
    public class ClientAuthRequestedEventArgs : EventArgs
    {
        public ClientAuthRequestedEventArgs(SGClient client, string token)
        {
            Client = client;
            Token = token;
        }

        public SGClient Client { get; }
        public string Token { get; }

        public bool Reject { get; set; }

    }
}