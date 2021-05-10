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
        public int HeartbeatInterval { get; set; } = 10_000;
        public int HeartbeatTimeout { get; set; } = 5_000;

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
            var timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = HeartbeatInterval;
            timer.Elapsed += (s, e) =>
            {
                var current = clients.ToArray();
                foreach (var client in current)
                {
                    try
                    {
                        if (!client.IsTransmitting)
                        {
                            Task.Run(() =>
                            {
                                bool alive = client.Heartbeat(DateTime.Now.ToString(), HeartbeatInterval, HeartbeatTimeout);
                                if (!alive)
                                    client.Close(0, "No heartbeat response received");
                                
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex;
                    }
                }
            };
            Stopped += (s, e) => timer.Stop();
            timer.Start();
        }
    }
}
