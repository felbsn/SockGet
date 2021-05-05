using SockGet.Client;
using SockGet.Core;
using SockGet.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Server
{
    public class SGServer
    {
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopped;



        public event EventHandler<ClientAuthRequestedEventArgs> ClientAuthRequested;
        public event EventHandler<ClientConnectionEventArgs> ClientConnected;
        public event EventHandler<ClientConnectionEventArgs> ClientDisconnected;

        Socket socket;
        List<SGSocket> clients;

        public IReadOnlyList<SGSocket> Clients => clients;

        public void Stop()
        {
            socket.Close();
            socket = null;
        }

        public SGServer()
        {
            clients = new List<SGSocket>();
        }

        public void Serve(int port)
        {
            IPAddress ip = IPAddress.Any;
       
            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);
            socket.Bind(localEndPoint);
            Accept();
            Started?.Invoke(this, EventArgs.Empty);
        }

        void Accept()
        {
            Task.Run((Action)(() =>
            {
                try
                {
                    while (socket.IsBound)
                    {
                        socket.Listen(1);
                        var sock = socket.Accept();

                        var client = new SGClient(sock);
                        client.AuthRequested += (s, e) =>
                        {
                            
                            var args = new ClientAuthRequestedEventArgs(client, e.AuthToken , e.Response);
                            ClientAuthRequested?.Invoke(this , args);
                     
                            e.Reject = args.Reject || (args.Response != null && args.Response.IsError);
                            if(!e.Reject)
                            {
                                client.Disconnected += (s1, e1) =>
                                {
                                    clients.Remove(client);
                                    ClientDisconnected?.Invoke(this, new ClientConnectionEventArgs(client));
                                };

                                clients.Add(client);
                                ClientConnected?.Invoke(this, new ClientConnectionEventArgs(client));
                            }    
                        };
                        client.Listen();
                    }
                }
                catch (Exception ex)
                {
                    _ = ex;
                    Stopped?.Invoke(this , EventArgs.Empty);
                }
                finally
                {
                    foreach (var client in Clients)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch (Exception ex)
                        {
                            _ = ex;
                        }    
                    }
                    clients.Clear();
                }
            }));
        }

  
    }
}
