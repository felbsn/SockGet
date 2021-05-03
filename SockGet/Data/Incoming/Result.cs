using SockGet.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data
{
    public class Result
    {
        public bool IsOK => !IsError && !IsCancelled;
        public bool IsError => Info == "@error";
        public bool IsCancelled => Info == "@cancel";
        public bool IsEmpty => Object == null && string.IsNullOrEmpty(Body) && string.IsNullOrEmpty(Head);

        public string Head { get; }
        public string Body { get; }
        public string Info { get; }
        public object Object { get; }

        public T As<T>()
        {
            if (Object != null)
            {
                if (Object is T)
                {
                    return (T)Object;
                }
                else
                {
                    return Serializer.Deserialize<T>(Body);
                }
            }
            else
                return Serializer.Deserialize<T>(Body);
        }

        internal Result(string head, string body, string info, object @object)
        {
            Head = head;
            Body = body;
            Info = info;
            Object = @object;
        }
    }
}
