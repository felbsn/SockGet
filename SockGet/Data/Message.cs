using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SockGet.Core;
using SockGet.Core.Enums;
using SockGet.Serialization;

namespace SockGet.Data
{
    public class Message
    {
        public Message()
        {
        }

        public string Info
        {
            get => Info == null ? null :  new StreamReader(info).ReadToEnd();
            internal set => info = string.IsNullOrEmpty(value) ? null : new MemoryStream(Encoding.UTF8.GetBytes(value));
        }
        public string Head
        {
            get => head == null ? null : new StreamReader(head).ReadToEnd();
            set => head = string.IsNullOrEmpty(value) ? null : new MemoryStream(Encoding.UTF8.GetBytes(value));
        }
        public string Body
        {
            get => body == null ? null : (body.Length >= 64 * 1024 ? "..." : new StreamReader(body).ReadToEnd());
            set => body = string.IsNullOrEmpty(value) ? null : new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

        public static Message From(object obj)
        {
            return From("", obj);
        }
        public static Message From(string head , object body)
        {
            var msg = new Message();
            msg.Head = head;

            if (body is string)
            {
                msg.Info = "@string";
                msg.Body = (string)body;
            }
            else
            {
                msg.Info = body.GetType().Name;
                msg.Body = Serializer.Serialize(body);
            }

            return msg;
        }

        Stream info;
        Stream head;
        Stream body;
 
        internal Stream GetStream(out Header header)
        {
            header = new Header();
            header.version = Header.CurrentVersion;
            header.bodyLength = body == null ? (int)0 : (int)body.Length;
            header.headLength = head == null ? (ushort)0 : (ushort)head.Length;
            header.infoLength = info == null ? (byte)0 : (byte)info.Length;

            return new SGMergedStream(info, head, body);
        }
    }
}
