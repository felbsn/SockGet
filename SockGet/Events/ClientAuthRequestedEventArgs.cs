using SockGet.Client;
using SockGet.Data;
using System;

namespace SockGet.Events
{
    public class ClientAuthRequestedEventArgs : EventArgs
    {
        public ClientAuthRequestedEventArgs(SGClient client, string token  ,Response response)
        {
            Client = client;
            Token = token;
            Response = response;
        }

        public SGClient Client { get; }
        public string Token { get; }
        public Response Response { get; set; }
        public bool Reject { get; set; }

    }
}