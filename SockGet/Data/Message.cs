using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SockGet.Core;
using SockGet.Core.Enums;
using SockGet.Core.Extensions;
using SockGet.Data.Streams;
using SockGet.Serialization;

namespace SockGet.Data
{
    public class Message  
    {
        public string Head { get; set; }
        public string Info { get; set; }
        public Stream Body { get; set; }
 
        public Message Load(string head , Stream body )
        {
            Head = head;
            Body = body;
            return this;
        }
        public Message Load(object body)
        {
            return Load(String.Empty, body);
        }
        public Message Load(string head, object body)
        {
            switch (body)
            {
                case string str:
                    return Load(head, str.ToStream());
                case byte[] data:
                    return Load(head, new MemoryStream(data));
                case Stream stream:
                    return Load(head, stream);
                default:
                    return Load(head, new MemoryStream(SgSocket.Serializer.Serialize(body)));
            }
        }
    }
}
