using SockGet.Core.Enums;
using SockGet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = SockGet.Core.Enums.Type;

namespace SockGet.Core
{
    public struct Header
    {
        public static byte CurrentVersion => 2;
        public static int Size => 16;

        internal Type Type { get => (Type)type; set => type = (byte) value; }
        internal Role Role { get => (Role)role; set => role = (byte)value; }
        internal Status Status { get => (Status)status; set => status = (byte)value; }


        public byte version; 
        public byte type; 
        public byte role; 
        public byte status;

        public uint id; 

        public ushort infoLength; 
        public ushort  headLength; 
        public int bodyLength;

        public byte[] GetBytes()
        {
            var bytes = new byte[Size];

            bytes[0] = version;
            bytes[1] = type;
            bytes[2] = role;
            bytes[3] = status;

            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(infoLength), 0, bytes, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(headLength), 0, bytes, 10, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(bodyLength), 0, bytes, 12, 4);

            return bytes;
        }
        public static Header Parse(byte[] bytes)
        {
                var messageHeader = new Header();

                messageHeader.version = bytes[0];
                messageHeader.type = bytes[1];
                messageHeader.role = bytes[2];
                messageHeader.status = bytes[3];

                messageHeader.id = BitConverter.ToUInt32(bytes, 4);
                messageHeader.infoLength = BitConverter.ToUInt16(bytes, 8);
                messageHeader.headLength = BitConverter.ToUInt16(bytes, 10);
                messageHeader.bodyLength = BitConverter.ToInt32(bytes, 12);

                return messageHeader;
        }
    }


}
