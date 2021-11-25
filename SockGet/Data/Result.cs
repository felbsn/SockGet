using SockGet.Core;
using SockGet.Enums;
using SockGet.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data
{
    public class Result
    {
        public string Head => head_str;
        public string Info => info_str ;

        //public string Body => body_str ?? (body_str = new StreamReader(body).ReadToEnd() ?? String.Empty);
        public Status Status { get;internal set; }

        internal string head_str = null;
        internal string info_str = null;
        internal string body_str = null;

        internal byte[] head;
        internal byte[] info;
        internal Stream body;

        public Stream AsStream()
        {
            return body;
        }

        public string AsString()
        {
            body.Position = 0;
            return body.Length > 0 ? new StreamReader(body).ReadToEnd() : String.Empty;
        }
        public T As<T>()
        {
            var bytes = new byte[body.Length];
            body.Position = 0;
            body.Read(bytes, 0, (int)body.Length);
            return SgSocket.Serializer.Deserialize<T>(bytes);
        }

        public bool AsFile(string path)
        {
            using(FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                body.CopyTo(fs);
            }
            return true;
        }

        internal Result()
        {
        }
    }
}
