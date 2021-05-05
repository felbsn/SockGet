using SockGet.Core.Enums;
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
        public static Response Cancel(string message = null, string details = null)
        {
            return new Response()
            {
                Head = message,
                Body = details,
                Info = "@cancel",
                IsError = true,
            };
        }
        public static Response OK(string message = null, string details = null)
        {
            return new Response()
            {
                Head = message,
                Body = details,
                Info = "@ok"
            };
        }
        public static Response Error(string message = null,string details = null)
        {
            return new Response()
            {
                Body = details,
                Head = message,
                Info = "@error"
                IsError = true,
            };
        }

        public static Response Reject(string message = null, string details = null)
        {
            return new Response()
            {
                Body = details,
                Head = message,
                Info = "@reject"
                IsError = true,
            };
        }

        public static Response From(object body)
        {
            return From("", body);
        }
        public static Response From(string head, object body)
        {
            var res = new Response();
            res.Load(head, body);
            return res;
        }

         

        public static readonly Response Empty  = new Response() { };

        public bool IsError { get; internal set; }
    }
}
