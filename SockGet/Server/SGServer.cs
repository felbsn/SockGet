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

        public bool KeepAlive { get; set; } = true;
        public uint KeepAliveInterval { get; set; } = 1000;
        public uint KeepAliveTime { get; set; } = 1000;

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
                        if (KeepAlive)
                            SetKeepAlive(KeepAlive, KeepAliveTime, KeepAliveInterval);

                        var client = new SGClient(sock);
                        client.AuthRequested += (s, e) =>
                        {
                            var args = new ClientAuthRequestedEventArgs(client, e.AuthToken , e.Response);
                            ClientAuthRequested?.Invoke(this , args);

                            e.Response = args.Response;
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

        void SetKeepAlive(bool on, uint keepAliveTime, uint keepAliveInterval)
        {
            int size = Marshal.SizeOf(new uint());

            var inOptionValues = new byte[size * 3];

            BitConverter.GetBytes((uint)(on ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(inOptionValues, size * 2);

            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
    }
}
