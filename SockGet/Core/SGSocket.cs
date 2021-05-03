using SockGet.Client;
using SockGet.Core.Enums;
using SockGet.Data;
using SockGet.Events;
using SockGet.Exceptions;
using SockGet.Extensions;
using SockGet.Serialization;
using SockGet.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SockGet.Core
{
    public abstract class SGSocket
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<AuthRespondEventArgs> AuthRespond;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public bool IsAuthorised { get;  protected set; }
        public bool ThrowExceptionOnAuthFailed { get; set; } = true;
        public Dictionary<string, string> Tags => tags;
        public DataReceiver Receiver { get; set; }
        public string AuthToken { get;  set; }
        public void Close()
        {
            socket.Close();
            socket = null;
        }
        public bool IsConnected()
        {
            return socket != null && !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
        }
        public void SendMessage(string action, string content)
        {
            Send(new Message()
            {
                Head = action,
                Body = content
            });
        }
        public Result SendRequest(string action, string content)
        {
            return Request(new Message()
            {
                Head = action,
                Body = content
            });
        }



        protected Socket socket;
        protected SGSocket()
        {
            pending = new Dictionary<byte, TaskCompletionSource<Result>>();
            tags = new Dictionary<string, string>();
        }


  
        int counter;
        Dictionary<string, string> tags;
        Dictionary<byte, TaskCompletionSource<Result>> pending;

        internal event EventHandler<AuthRequestedEventArgs> AuthRequested;
  

        internal void Listen()
        {
            Task.Run((() =>
            {
                try
                {
                    while (IsConnected())
                    {
                        byte[] buffer = new byte[10];
                        var count = socket.Receive(buffer, 10, SocketFlags.None);
                        if (count == 10)
                        {
                            var header = Header.Parse(buffer);
                            //if (header.version == 1)
                            {
                                var token = (Token)header.token;

              
                                socket.ReadBytes(header.infoLength, out var infoBuffer);
                                socket.ReadBytes(header.headLength, out var headBuffer);
                         
                                var info = Encoding.UTF8.GetString(infoBuffer);
                                var head = Encoding.UTF8.GetString(headBuffer);

                                var sgstream = new SGSocketStream(socket, header.bodyLength);

                                object obj = null;
                                string body = null;

                                if(sgstream.CanRead)
                                {
                                    if (Receiver != null && (token == Token.Message || token == Token.Request || token == Token.Response))
                                    {
                                        Receiver?.Invoke(head, info, sgstream);
                                        sgstream.FinishRead();
                                    }
                                    else
                                    {
                                        using (var reader = new StreamReader(sgstream))
                                            body = reader.ReadToEnd();
                                    }
                                }

                                var received = new Result(head, body, info, obj);

                                switch (token)
                                {
                                    case Token.AuthRequest:
                                        {
                                            tags = Serializer.Deserialize<Dictionary<string, string>>(body);

                                            var args = new AuthRequestedEventArgs(head);
                                            AuthRequested?.Invoke(this, args);
                                            if (args.Reject)
                                            {
                                                Close();
                                            }
                                            else
                                            {
                                                Authorize(args.Response ?? Data.Response.Empty);
                                            }
                                        }
                                        break;
                                    case Token.AuthResponse:
                                        {
                                            var args = new AuthRespondEventArgs(head, body, false);
                                            AuthRespond?.Invoke(this, args);
                                        }
                                        break;
                                    case Token.Message:
                                        {
                                            var args = new DataReceivedEventArgs(received, false, null);
                                            Task.Run(() => DataReceived?.Invoke(this, args));
                                        }
                                        break;
                                    case Token.Request:
                                        {
                                            var args = new DataReceivedEventArgs(received, true, Data.Response.Empty);
                                            Task.Run(() => DataReceived?.Invoke(this, args))
                                            .ContinueWith(t => Response(header.id, args.Response));
                                        }
                                        break;
                                    case Token.Response:
                                        {
                                            if (pending.TryGetValue(header.id, out var tcs))
                                            {
                                                tcs.SetResult(received);
                                                pending.Remove(header.id);
                                            }
                                            else
                                            {
                                                throw new Exception("Unable to find pending response task!");
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
                finally
                {
                    // exception probably caused by internal logic, so closing connection maybe the best option available
                    if (IsConnected())
                    {
                        socket.Close();
                    }


                    if (IsAuthorised)
                    {
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        var args = new AuthRespondEventArgs(null, null, true);
                        AuthRespond?.Invoke(this, args);
                    }
                    IsAuthorised = false;
                    socket?.Dispose();
                    socket = null;
                }
            }));
        }
        internal void Transmit(Header header, Stream stream)
        {
            lock (socket)
            {
                socket.Send(header.GetBytes());

                int read;
                byte[] buffer = new byte[1024 * 1024];
                while ((read = stream.Read(buffer, 0, 1024 * 1024)) > 0)
                {
                    socket.Send(buffer ,  read , SocketFlags.None);
                }
            }
        }
        internal void Authorize(Message data)
        {
            var stream = data.GetStream(out var header);

            header.token = (byte)Token.AuthResponse;
            Transmit(header, stream);
        }
        internal bool Authenticate()
        {
            var body = Serializer.Serialize(tags);

            var msg = new Message()
            {
                Head = AuthToken,
                Body = body
            };

            var tcs = new TaskCompletionSource<bool>();
            EventHandler<AuthRespondEventArgs> authRespondEvent = null;
            authRespondEvent = (s, e) =>
            {
                if (e.IsRejected)
                    tcs.SetResult(false);
                else
                    tcs.SetResult(true);

                AuthRespond -= authRespondEvent;
            };
            AuthRespond += authRespondEvent;

            var stream = msg.GetStream(out var header);
            header.token = (byte)Token.AuthRequest;
            Transmit(header, stream);

            try
            {
                tcs.Task.Wait();
            }
            catch (Exception)
            {
                return false;
            }

            IsAuthorised = tcs.Task.Result;

            if (!IsAuthorised && ThrowExceptionOnAuthFailed)
            {
                throw new AuthorizationException("Authorization is unsuccessful");
            }

            Connected?.Invoke(this, EventArgs.Empty);

            return IsAuthorised;
        }
        internal void Send(Message message)
        {
            var stream = message.GetStream(out var header);
            header.token = (byte)Token.Message;
            Transmit(header, stream);
        }
        internal void Response(byte id , Message message)
        {
            var stream = message.GetStream(out var header);
            header.id = id;
            header.token = (byte)Token.Response;
            Transmit(header, stream);
        }
        internal Result Request(Message message)
        {
            byte id = (byte)Interlocked.Increment(ref counter);

            var tcs = new TaskCompletionSource<Result>();
            pending.Add(id, tcs);

            var stream = message.GetStream(out var header);
            header.id = id;
            header.token = (byte)Token.Request;
            Transmit(header, stream);

            try
            {
                return tcs.Task.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
