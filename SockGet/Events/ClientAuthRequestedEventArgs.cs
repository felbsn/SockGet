using SockGet.Client;
using SockGet.Data;
using System;

namespace SockGet.Events
{
    public class ClientAuthRequestedEventArgs : ClientAuthRequestedEventArgs<object>
    {
        public ClientAuthRequestedEventArgs(SgClient<object> client, string token, Response response) : base(client, token, response)
        {
        }
    }
    public class ClientAuthRequestedEventArgs<T> : EventArgs
    {
        public ClientAuthRequestedEventArgs(SgClient<T> client, string token  ,Response response)
        {
            Client = client;
            Token = token;
            Response = response;
        }

        public SgClient<T> Client { get; }
        public string Token { get; }
        public Response Response { get; set; }
        public bool Reject { get; set; }

    }
}