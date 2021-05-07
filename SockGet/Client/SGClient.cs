using SockGet.Serialization;
using SockGet.Core;
using SockGet.Core.Enums;
using SockGet.Data;
using SockGet.Events;
using SockGet.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SockGet.Extensions;
using SockGet.Exceptions;

namespace SockGet.Client
{
    public class SGClient : SGSocket
    {
        public int Port { get; protected set; }
        public string Address { get; protected set; }
        public SGClient() 
        {
      
        }

        public bool Connect(int port)
        {
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = host.AddressList[0];

            return Connect(ipAddress.ToString(), port);
        }
        public bool Connect(string address, int port)
        {
            var ipAddress = IPAddress.Parse(address);
            Address = address;
            Port = port;

            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            try
            {
                socket.Connect(localEndPoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Listen();
            return Authenticate();
        }
        public bool Reconnect()
        {
            var ipAddress = IPAddress.Parse(Address);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(localEndPoint);
            }
            catch (Exception ex)
            {
                return false;
            }

            Listen();
            return Authenticate();
        }
        public async Task<bool> ReconnectAsync(int timeout)
        {
            if (timeout == 0)
                return Reconnect();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            bool connected = false;

            while (!connected && (timeout == int.MaxValue || (sw.ElapsedMilliseconds < timeout)))
            {
                connected = Reconnect();
                await Task.Delay(500);
            }
            return connected;
        }
        public async Task<bool> ConnectAsync(int port, int timeout = 2000)
        {
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = host.AddressList[0];

            return await ConnectAsync(ipAddress.ToString(), port, timeout);
        }
        public async Task<bool> ConnectAsync(string address, int port, int timeout = 2000)
        {
            if (timeout == 0)
                return await Task.Run(() => Connect(address, port));

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool connected = false;

            while (!connected && (timeout == -1 || (sw.ElapsedMilliseconds < timeout)))
            {
                connected = await Task.Run(() =>
                {
                    var t = Task.Run(() => Connect(address, port));

                    try
                    {
                        if (t.Wait(timeout == -1 ? timeout : timeout - (int)sw.ElapsedMilliseconds))
                        {
                            return t.Result;
                        }
                        else
                        {
                            socket.Close();
                            return false;
                        };
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException ?? ex;
                    }
                });
          
                if(!connected && (timeout == -1 || 500 < timeout - sw.ElapsedMilliseconds))
                    await Task.Delay(500);
            }
            return connected;
        }

        internal SGClient(Socket socket)
        {
            this.socket = socket;
            IsAuthorised = true;

            Address = socket.RemoteEndPoint.ToString();
        }
    }
}
