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
        public void Close(int timeout = 0)
        {
            socket?.Close(timeout);
            socket = null;
        }
        public bool IsConnected()
        {
            return socket != null && socket.Connected;
        }

        public string this[string key]
        {
            get => Tags.TryGetValue(key, out var value) ? value : null;
        }

        public Task<Result> RequestAsync(string action, string content, int timeout = -1) => Task.Run(() => Request(action, content , timeout));
        public Task<string> RequestTagAsync(string tagName, int timeout = -1) => Task.Run(() => RequestTag(tagName ));

        public void Message(string action, string content)
        {
            Send(new Message()
            {
                Head = action,
                Body = content
            }, Token.Message);
        }
        public Result Request(string action, string content ,int timeout = -1)
        {
            return Request(new Message()
            {
                Head = action,
                Body = content
            },Token.Message , timeout);
        }


        public string RequestTag(string tagName ,int timeout = -1)
        {
            return Request(new Message()
            {
                Head = tagName,
            },
            Token.Tag , timeout).Head;
        }
        public bool Heartbeat(string echo, int timeout = -1)
        {
            return Request(new Message()
            {
                Head = echo,
            },
            Token.Heartbeat, timeout)?.Head == echo;
        }




        protected Socket socket;
        protected SGSocket()
        {
            pending = new Dictionary<uint, TaskCompletionSource<Result>>();
            tags = new Dictionary<string, string>();
        }
  
        long counter;
        Dictionary<string, string> tags;
        Dictionary<uint, TaskCompletionSource<Result>> pending;

        internal event EventHandler<AuthRequestedEventArgs> AuthRequested;

        internal DateTime LastReceiveTime { get; set; }
        internal bool IsReceiving { get; set; }
        internal bool IsTransmitting { get; set; }

        internal void Listen()
        {
            Task.Run((() =>
            {
                try
                {
                    while (IsConnected())
                    {
                        IsReceiving = false;
                        LastReceiveTime = DateTime.Now;

                        int count = 0;
                        byte[] buffer = new byte[Header.Size];
                        count = socket.ReadBytes(buffer, 0, Header.Size);

                        IsReceiving = true;
                        if (count == Header.Size)
                        {
                            var header = Header.Parse(buffer);
                            if (header.version == 1 )
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
                                    if (Receiver != null && (token == Token.Message))
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

                                switch (header.Type)
                                {
                                    case Enums.Type.Message:
                                        HandleMessage(header, received);
                                        break;
                                    case Enums.Type.Request:
                                        HandleRequest(header, received);
                                        break;
                                    case Enums.Type.Response:
                                        HandleResponse(header, received);
                                        break;
                                    default:
                                        break;
                                }
                            }else
                            {
                                throw new UnsupportedVersionException("Unsupported SG version !");
                            }
                        }
                        else
                        {
                            Close();
                            throw new HeaderReceiveException("Header byte count corrupt.");
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
                    // or auth rejected by server 
                    if (IsConnected())
                    {
                        socket.Close(500);
                    }

                    if (IsAuthorised)
                    {

                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        //if auth failed without response, simulate respond event with rejection
                        var args = new AuthRespondEventArgs(null, null, true);
                        AuthRespond?.Invoke(this, args);
                    }

                    IsAuthorised = false;
                    socket?.Dispose();
                    socket = null;
                }
            }));
        }

        private void HandleMessage(Header header, Result received)
        {
            var args = new DataReceivedEventArgs(received, false, null);
            Task.Run(() => DataReceived?.Invoke(this, args));
        }
        private void HandleRequest(Header header, Result received)
        {
            switch (header.Token)
            {
                case Token.Message:
                    {
                        var args = new DataReceivedEventArgs(received, true, Data.Response.Empty);
                        Task.Run(() => DataReceived?.Invoke(this, args))
                            .ContinueWith(t => Response(header.id, args.Response, header.Token, args.Response?.IsError == true ? Status.Error : Status.OK));
                    }
                    break;
                case Token.Auth:
                    {
                        tags = received.As<Dictionary<string, string>>();

                        var args = new AuthRequestedEventArgs(received.Head);
                        AuthRequested?.Invoke(this, args);

                        var response = args.Response ?? Data.Response.Empty;
                        if (args.Reject || response.IsError)
                        {
                            Authorize(response, false);
                            // leave loop
                            break;
                        }
                        else
                        {
                            Authorize(response, true);
                        }
                    }
                    break;
                case Token.Tag:
                    break;
                case Token.Heartbeat:
                    {
                        Task.Run(() => Response(header.id, Data.Response.From(received.Head , null), header.Token,  Status.OK));
                    }
                    break;
                case Token.Sync:
                    break;
                default:
                    break;
            }
        }
        private void HandleResponse(Header header, Result received)
        {
            var args = new DataReceivedEventArgs(received, false, null);
            switch (header.Token)
            {
                case Token.Auth:
                    {
                        AuthRespond?.Invoke(this, new AuthRespondEventArgs(received.Head, received.Body, header.Status != Status.OK));
                    }
                    break;
                default:
                    DispatchResult(header.id, received);
                    break;
            }
        }
        private void DispatchResult(uint id , Result received)
        {
            if (pending.TryGetValue(id, out var tcs))
            {
                tcs.SetResult(received);
                pending.Remove(id);
            }
            else
            {
                throw new Exception("Unable to find pending response task!");
            }
        }

        internal void Transmit(Header header, Stream stream)
        {
            lock (socket)
            {
                IsTransmitting = true;
                socket.Send(header.GetBytes());

                int read;
                byte[] buffer = new byte[1024 * 1024];
                while ((read = stream.Read(buffer, 0, 1024 * 1024)) > 0)
                {
                    socket.Send(buffer ,  read , SocketFlags.None);
                }
                IsTransmitting = false;
            }
        }
        internal void Send(Message message, Token token = Token.Message, Status status = Status.OK, Enums.Type type = Enums.Type.Message , uint id = 0)
        {
            var stream = message.GetStream(out var header);
            header.id = id;
            header.Token = token;
            header.Status = status;
            header.Type = type;
            Transmit(header, stream);
        }
        internal void Authorize(Message data, bool accept) => Send(data, Token.Auth, accept ? Status.OK : Status.Error, Enums.Type.Response, 0);
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

            try
            {
                Send( msg, Token.Auth, Status.OK, Enums.Type.Request, 0);
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
        internal Result Request(Message message ,Token token, int timeout = -1)
        {
            var id =  (uint)Interlocked.Increment(ref counter);

            var tcs = new TaskCompletionSource<Result>();
            pending.Add(id, tcs);

            Send(message, token, Status.OK, Enums.Type.Request , id);

            var task = tcs.Task;
            try
            {
                return task.Wait(timeout) ? task.Result : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        internal void Response(uint id, Message message, Token token, Status status) => Send(message, token, status, Enums.Type.Response, id);

    }
}
