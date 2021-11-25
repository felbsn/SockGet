using SockGet.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data.Streams
{
    /// <summary>
    /// Read socket as a fixed length stream
    /// </summary>
    public class SgSocketStream : Stream
    {

        public override bool CanRead { get => position < Length; } 
        public override bool CanSeek { get => false; }
        public override bool CanWrite { get => false; } 
        public override long Length { get; }
        public override long Position { get => position; set => throw new NotImplementedException(); }

        Socket socket;
        int position;

        internal SgSocketStream(Socket socket, int length)
        {
            this.socket = socket;
            Length = length;
            position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CanRead)
            {
                var receiveCount = Math.Min((int)Length - position, count);

 
                var readed = socket.ReadBytes(buffer , offset, receiveCount);

                position += readed;

                return readed;
            }
            else
                return 0;
        }
 
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();

        }

        internal void FinishRead()
        {
            while(CanRead)
            {
                var temp = new byte[1024];
                Read(temp, 0, 1024);
            }
        }

        internal void ReadToEnd()
        {
            throw new NotImplementedException();
        }
    }
}
