using SockGet.Core.Enums;
using SockGet.Data;
using SockGet.Data.Streams;
using SockGet.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = SockGet.Core.Enums.Type;

namespace SockGet.Core.Extensions
{
    internal static class MessageExtensions
    {
        internal static Stream Build(this Message msg, Status status, Type type, Role role, uint id)
        {
            return Build(msg, status, type, role, id, out var _);
        }
        internal static Stream Build(this Message msg, Status status, Type type, Role role, uint id,   out Header header)
        {
            var head = msg.Head.ToStream();
            var info  = msg.Info.ToStream();

            header = new Header()
            {
                version = Header.CurrentVersion,
                bodyLength = (int)(msg.Body?.Length ?? 0),
                headLength = (ushort)head.Length,
                infoLength = (byte)info.Length,
                role = (byte)role,
                type = (byte)type,
                status = (byte)status,
                id = id,
            };
            var headerBytes = header.GetBytes();
            var headerStream = new  MemoryStream(headerBytes);

            return new SgMergedStream(headerStream, info, head, msg.Body);
        }
    }
}
