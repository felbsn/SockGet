using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core
{
    public struct Header
    {
        public static readonly byte CurrentVersion = 1;

        public byte version;
        public byte token;
        public byte id;
        public byte infoLength;
        public ushort  headLength;
        public int bodyLength;

        public byte[] GetBytes()
        {
            var bytes = new byte[10];
            bytes[0] = version;
            bytes[1] = token;
            bytes[2] = id;
            bytes[3] = infoLength;

            Array.Copy(BitConverter.GetBytes(headLength), 0, bytes, 4, 2);
            Array.Copy(BitConverter.GetBytes(bodyLength), 0, bytes, 6, 4);

            return bytes;
        }
        public static bool TryParse(byte[] bytes , out Header messageHeader)
        {
            if(bytes.Length == 64)
            {
                messageHeader = new Header();


                messageHeader.version = bytes[0];
                messageHeader.token = bytes[1];
                messageHeader.id = bytes[2];
                messageHeader.infoLength = bytes[3];

                messageHeader.headLength =  BitConverter.ToUInt16(bytes, 4);
                messageHeader.bodyLength =  BitConverter.ToInt32(bytes, 6);

                return true;
            }
            else
            {
                messageHeader = default;
                return false;
            }
        }
        public static Header Parse(byte[] bytes)
        {
                var messageHeader = new Header();

                messageHeader.version = bytes[0];
                messageHeader.token = bytes[1];
                messageHeader.id = bytes[2];
                messageHeader.infoLength = bytes[3];

                messageHeader.headLength = BitConverter.ToUInt16(bytes, 4);
                messageHeader.bodyLength = BitConverter.ToInt32(bytes, 6);

                return messageHeader;
        }
    }


}
