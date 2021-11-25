using SockGet.Client;
using SockGet.Core.Enums;
using SockGet.Enums;
using SockGet.Data;
using SockGet.Data.Streams;
using SockGet.Delegates;
using SockGet.Events;
using SockGet.Exceptions;
using SockGet.Core.Extensions;
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
using Type = SockGet.Core.Enums.Type;

namespace SockGet.Core
{
    public abstract class SgSocket : SgSocket<object>
    {
        public static ISerializer Serializer;
    }

    public abstract class SgSocket<T>
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<AuthRespondEventArgs> AuthRespond;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public T Data { get; set; }
        public DataReceiver Receiver { get; set; }
        public bool IsAuthorised { get; protected set; }
        public Dictionary<string, string> Tags => tags;
      
        public string AuthToken { get; set; }
        public void Close(int timeout = 0, string reason = null)
        {
            CloseReason = reason;
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
            set => Tags[key] = value;
        }

        public void Send(Message msg, Status status = Status.OK)
        {
            var stream = msg.Build(status, Enums.Type.Message, Role.Message, 0);
            Transmit(stream);
        }
        public Result Request(Message msg, Status status = Status.OK, int timeout = -1)
        {
            var id = (uint)Interlocked.Increment(ref counter);
            var stream = msg.Build(status, Enums.Type.Message, Role.Request, id);
            return TransmitReceive(stream, id , timeout);
        }

        public void Send(string head, object body, Status status = Status.OK)
        {
            Send(new Message().Load(head, body), status);
        }

        public Result Request(string head, object body, Status status = Status.OK, int timeout = -1)
        {
            return Request(new Message().Load(head, body), status ,timeout);
        }

        public Task<Result> RequestAsync(string head, object body, Status status = Status.OK, int timeout = -1) => Task.Run(()=> Request(head, body, status, timeout));
        public Task SendAsync(string head, object body, Status status = Status.OK) => Task.Run(()=> Send(head, body, status));


        public void Sync(int timeout = -1)
        {
            var id = (uint)Interlocked.Increment(ref counter);
            var msg = new Message();

            msg.Load("tags", tags);

            var stream = msg.Build(Status.OK, Enums.Type.Sync, Role.Request, id);
            TransmitReceive(stream, id , timeout);
        }
        public bool Heartbeat(string echo, int interval, int timeout)
        {
            var msg = new Message();
            msg.Load(echo, ((interval, timeout)));

            var id = (uint)Interlocked.Increment(ref counter);
            var stream = msg.Build(Status.OK, Enums.Type.Heartbeat, Role.Request, id);
            var result = TransmitReceive(stream, id, timeout);

            return result?.Head == echo;
        }


        protected Socket socket;
        protected SgSocket()
        {
            pending = new Dictionary<uint, TaskCompletionSource<Result>>();
            tags = new Dictionary<string, string>();

            SgSocket.Serializer = SgSocket.Serializer ?? new Serializer();
        }

        internal event EventHandler<AuthRequestedEventArgs> AuthRequested;

        long counter;
        Dictionary<uint, TaskCompletionSource<Result>> pending;
        Dictionary<string, string> tags;

        internal string CloseReason;
        internal DateTimeOffset LastReceive { get; set; }
        internal DateTimeOffset LastHeartbeat { get; set; }

        internal object ReceiveLock = "";

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
                        LastReceive = DateTimeOffset.Now;
                        IsReceiving = false;

                        int count = 0;
                        byte[] buffer = new byte[Header.Size];
                        count = socket.ReadBytes(buffer, 0, Header.Size);
                        if (count == Header.Size)
                        {
                            IsReceiving = true;

                            var header = Header.Parse(buffer);
                            if (header.version == 2)
                            {
                                var type = (Enums.Type)header.type;

                                socket.ReadBytes(header.infoLength, out var infoBuffer);
                                socket.ReadBytes(header.headLength, out var headBuffer);

                                var info_str = Encoding.UTF8.GetString(infoBuffer);
                                var head_str = Encoding.UTF8.GetString(headBuffer);

                                var sgstream = new SgSocketStream(socket, header.bodyLength);

                                Stream stream;
                                if(sgstream.Length > 0)
                                {
                                    if (type == Type.Message && Receiver != null)
                                    {
                                        stream = Receiver?.Invoke(head_str, info_str, sgstream);
                                        if (sgstream.CanRead)
                                            sgstream.ReadToEnd();
                                    }
                                    else
                                    {
                                        stream = new MemoryStream();
                                        sgstream.CopyTo(stream);
                                        stream.Position = 0;
                                    }
                                }else
                                {
                                    stream = new MemoryStream();
                                }
                                

                                var received = new Result()
                                {
                                    head_str = head_str,
                                    info_str = info_str,
                                    head = headBuffer,
                                    info = infoBuffer,
                                    body = stream,
                                    Status = (Status)header.status
                                };

                                switch (header.Role)
                                {
                                    case Role.Message:
                                        HandleMessage(header, received);
                                        break;
                                    case Role.Request:
                                        HandleRequest(header, received);
                                        break;
                                    case Role.Response:
                                        HandleResponse(header, received);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                                throw new UnsupportedVersionException("Unsupported SG version !");
                        }
                        else
                        {
                            Close();
                            throw new HeaderReceiveException("Header byte count corrupt.");
                        }
                    }
                }
                catch (SocketException ex)
                {
                    CloseReason = ex.Message;
                }
                catch (Exception ex)
                {
                    CloseReason = ex.Message;
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
                        Disconnected?.Invoke(this, new DisconnectedEventArgs(CloseReason));

                        // reset reason after emit.
                        CloseReason = null;
                    }
                    else
                    {
                        //if auth failed without response, simulate respond event with rejection
                        //var args = new AuthRespondEventArgs(null, null, true);
                        //AuthRespond?.Invoke(this, args);
                    }

                    IsAuthorised = false;
                    socket?.Dispose();
                    socket = null;
                }
            }));
        }

        bool HeartbeatListening { get; set; } = false;


        private void ListenHeartbeat(int interval, int timeout)
        {
            if (!HeartbeatListening)
            {
                HeartbeatListening = true;

                var t = new System.Timers.Timer();
                t.Interval = interval;
                t.AutoReset = true;
                t.Elapsed += (s, e) =>
                {
                    if (HeartbeatListening)
                    {
                        if (!IsReceiving)
                        {
                            var diff = (DateTimeOffset.Now - LastReceive);

                            if (diff.TotalMilliseconds > interval + timeout)
                            {
                                // close connection
                                Close(0, "No heartbeat signal received from server.");
                            }
                            t.Interval = interval - (int)(diff.Milliseconds);
                        }
                        else
                            t.Interval = interval;
                    }
                    else
                    {
                        t.Stop();
                    }
                };

                Disconnected += (s, e) =>
                {
                    HeartbeatListening = false;
                    t.Stop();
                };
                t.Start();

            }
            else
            {
                if (interval <= 0)
                {
                    HeartbeatListening = false;
                }
            }
        }

        private void HandleMessage(Header header, Result received)
        {
            var args = new DataReceivedEventArgs(received, false, null);
            Task.Run(() =>
            {
                DataReceived?.Invoke(this, args);
            });
        }
        private void HandleRequest(Header header, Result received)
        {
            switch (header.Type)
            {
                case Enums.Type.Message:
                    {
                        var args = new DataReceivedEventArgs(received, true,  Response.Empty);
                        Task.Run(() =>
                        {
                            DataReceived?.Invoke(this, args);
                            var stream = args.Response.Build(args.Status, Type.Message, Role.Response, header.id);
                            Transmit(stream);
                            stream.Close();
                        });
                    }
                    break;
                case Enums.Type.Auth:
                    {
                        var recvTags = received.As<Dictionary<string, string>>();

                        if(recvTags != null)
                        foreach (var pair in recvTags)
                        {
                            tags[pair.Key] = pair.Value;
                        }

                        var args = new AuthRequestedEventArgs(received.Head);
                        AuthRequested?.Invoke(this, args);
 

                        var response = args.Response ?? Response.Empty;
                        if (args.Reject || response.IsError)
                        {
                            Authorize(response, false);
                        }
                        else
                        {
                            Authorize(response, true);
                        }
                    }
                    break;
                case Enums.Type.Heartbeat:
                    {
                        Transmit(new Message().Load(received.Head, new byte[0]).Build(Status.OK, Type.Heartbeat, Role.Response, header.id));
 
                        var (interval, timeout) = received.As<(int, int)>();
                        if (interval > 0)
                        {
                            ListenHeartbeat(interval, timeout);
                        }
                        else
                        {
                            HeartbeatListening = false;
                        }
                    }
                    break;
                case Enums.Type.Sync:
                    {
                        var dict = received.As<Dictionary<string, string>>();
                        var diffs = tags.Keys.Except(dict.Keys);
                        foreach (var pair in dict)
                        {
                            tags[pair.Key] = pair.Value;
                        }

                        var diffDict = new Dictionary<string, string>();
                        foreach (var key in diffs)
                        {
                            diffDict[key] = tags[key];
                        }

                        Transmit(new Message().Load(diffDict).Build(Status.OK, Type.Sync, Role.Response, header.id));
                    }
                    break;
                default:
                    break;
            }
        }
        private void HandleResponse(Header header, Result received)
        {
            var args = new DataReceivedEventArgs(received, false, null);
            switch (header.Type)
            {
                case Enums.Type.Auth:
                    {
                        //todo:proper auth body 
                        AuthRespond?.Invoke(this, new AuthRespondEventArgs(received.Head, string.Empty, header.Status != Status.OK));
                    }
                    break;
                case Enums.Type.Sync:
                    {
                        var dict = received.As<Dictionary<string, string>>();
                        foreach (var pair in dict)
                        {
                            tags[pair.Key] = pair.Value;
                        }
                        DispatchResult(header.id, received);
                    }
                    break;
                default:
                    DispatchResult(header.id, received);
                    break;
            }
        }


        private void DispatchResult(uint id, Result received)
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

        internal void Transmit(Stream stream)
        {
            lock (socket)
            {
                IsTransmitting = true;
                int read;
                byte[] buffer = new byte[1024 * 1024];
                while ((read = stream.Read(buffer, 0, 1024 * 1024)) > 0)
                {
                    socket.Send(buffer, read, SocketFlags.None);
                }
                IsTransmitting = false;
            }
        }
        internal Result Receive(uint id, int timeout = -1)
        {
            var tcs = new TaskCompletionSource<Result>();
            pending.Add(id, tcs);
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
        internal Result TransmitReceive(Stream stream, uint id, int timeout = -1)
        {
            var tcs = new TaskCompletionSource<Result>();
            pending.Add(id, tcs);
            var task = tcs.Task;
            try
            {
                Transmit(stream);
                return task.Wait(timeout) ? task.Result : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
 
        internal void Authorize(Message data, bool accept)
        {
            Transmit(data.Build(accept ? Status.OK : Status.Rejected, Type.Auth, Role.Response, 0));
            IsAuthorised = accept;
            if (!IsAuthorised)
            {
                //todo: adjustable timeout?
                Close(1000, "Authorization is unsuccessful");
            }
        }
        internal bool Authenticate()
        {
            //todo: authenticate with more data....
            var msg = new Message();
            msg.Load(AuthToken, Tags);

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


            IsAuthorised = false;

            try
            {
                var stream = msg.Build(Status.OK, Enums.Type.Auth, Role.Request, 0);
                Transmit(stream);
                //Send(msg, Enums.Type.Auth, Status.OK, Enums.Role.Request, 0);
                tcs.Task.Wait();
                IsAuthorised = tcs.Task.Result;
            }
            catch (Exception)
            {

            }

            if (!IsAuthorised)

                throw new AuthorizationException("Authorization is unsuccessful");


            Connected?.Invoke(this, new ConnectedEventArgs());

            return IsAuthorised;
        }
    
        //internal void Response(uint id, Message message, Enums.Type token, Status status) => Send(message, token, status, Enums.Transmission.Response, id);
        //internal Task ResponseAsync(uint id, Message message, Enums.Type token, Status status) => Task.Run(() => Response(id, message, token, status));
    }
}
