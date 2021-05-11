using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SockGet.Core;
using SockGet.Core.Enums;
using SockGet.Data.Streams;
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
 
        internal void Load(string head , object body)
        {
            Head = head;
            if(body == null)
            {
                Body = "";
            }else
            if (body is string)
            {
                Info = "@string";
                Body = (string)body;
            }
            else
            {
                Info = body.GetType().Name;
                Body = Serializer.Serialize(body);
            }
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
