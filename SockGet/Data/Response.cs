using SockGet.Core.Enums;
using SockGet.Core.Extensions;
using SockGet.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data
{
    public class Response : Message
    {
        public Status Status { get; set; }
        public static Response Cancel(string message = null, string details = null)
        {
            return new Response()
            {
                Head = message,
                Body = details.ToStream(),
                Status = Status.Cancel,
            };
        }
        public static Response Ok(string message = null, string details = null)
        {
            return new Response()
            {
                Head = message,
                Body = details.ToStream(),
                Status = Status.OK,
            };
        }
        public static Response Error(string message = null,string details = null)
        {
            return new Response()
            {
                Body = details.ToStream(),
                Head = message,
                Status = Status.Error,
            };
        }

        public static Response Reject(string message = null, string details = null)
        {
            return new Response()
            {
                Body = details.ToStream(),
                Head = message,
                Status = Status.Rejected
            };
        }

        public static Response From(string head  , object body , Status status = Status.OK)
        {
            var response = new Response();
            switch (body)
            {
                case byte[] data :
                    response.Load(head, data);
                    break;
                case Stream data:
                    response.Load(head, data);
                    break;
                default:
                    response.Load(head, body);
                    break;
            }
            return response;
        }
        public static Response From(object body, Status status = Status.OK)
        {
            return From(String.Empty, body, status);
        }

        public static Response Empty  => new Response() { };

        public bool IsError { get; internal set; }
    }
}
