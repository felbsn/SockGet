using SockGet.Client;
using SockGet.Core;
using SockGet.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

        public bool UseHeartbeat { get; set; } = false;
        public int HeartbeatInterval { get; set; } = 5000;
        public int HeartbeatTimeout { get; set; } = 1000;

        public int AuthenticationTimeout { get; set; } = 2000;

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

            if (UseHeartbeat)
                HeartBeatRunner();
        }

        void Accept()
        {
            Task.Run((() =>
            {
                try
                {
                    while (socket.IsBound)
                    {
                        socket.Listen(1);
                        var sock = socket.Accept();

                        lock(this)
                        {
                            var client = new SGClient(sock);
                            client.AuthRequested += (s, e) =>
                            {
                                var args = new ClientAuthRequestedEventArgs(client, e.AuthToken, e.Response);
                                ClientAuthRequested?.Invoke(this, args);

                                e.Response = args.Response;
                                e.Reject = args.Reject || (args.Response != null && args.Response.IsError);
                                if (!e.Reject)
                                {
                                    client.Disconnected += (s1, e1) =>
                                    {
                                        lock (this)
                                        {
                                            clients.Remove(client);
                                        }

                                        ClientDisconnected?.Invoke(this, new ClientConnectionEventArgs(client , e1.Reason));
                                    };

                                    lock (this)
                                    {
                                        clients.Add(client);
                                    }

                                    ClientConnected?.Invoke(this, new ClientConnectionEventArgs(client));
                                }
                            };
                            client.Listen();
                            Task.Delay(AuthenticationTimeout).ContinueWith(t =>
                            {
                                if(!client.IsAuthorised)
                                {
                                    client.Close();
                                }
                            });
                           
                        }
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

        void HeartBeatRunner()
        {
            Task.Run(async () =>
            {
                try
                {
                    while(socket != null && socket.IsBound && UseHeartbeat)
                    {
                       await Task.Delay(HeartbeatInterval);

                       var current = clients.ToArray();
                       foreach (var client in current)
                       {
                           if (!client.IsReceiving && (DateTime.Now - client.LastReceive).TotalMilliseconds > HeartbeatInterval)
                           {
                                try
                                {
                                    Task.Run(() =>
                                    {
                                        var interval = HeartbeatInterval + HeartbeatTimeout;
                                        bool alive = client.Heartbeat(DateTime.Now.ToString(), interval, HeartbeatTimeout);
                                        if (!alive)
                                        {
                                            client.Close(0, "No heartbeat response received");
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    _ = ex;
                                }
                           }
                       }
                    }

                    if(socket != null && socket.IsBound && !UseHeartbeat)
                    {
                        var current = clients.ToArray();
                        foreach (var client in current)
                        {
                            if (!client.IsReceiving && (DateTime.Now - client.LastReceive).TotalMilliseconds > HeartbeatInterval)
                            {
                                try
                                {
                                    bool alive = client.Heartbeat(DateTime.Now.ToString(), 0 ,  HeartbeatTimeout);

                                    if (!alive)
                                    {
                                        client.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _ = ex;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            });

        }
    }
}
