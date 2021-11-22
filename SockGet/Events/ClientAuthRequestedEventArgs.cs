using SockGet.Client;
using SockGet.Data;
using System;

namespace SockGet.Events
{
    public class ClientAuthRequestedEventArgs : EventArgs
    {
        public ClientAuthRequestedEventArgs(SgClient client, string token  ,Response response)
        {
            Client = client;
            Token = token;
            Response = response;
        }

        public SgClient Client { get; }
        public string Token { get; }
        public Response Response { get; set; }
        public bool Reject { get; set; }

    }
}