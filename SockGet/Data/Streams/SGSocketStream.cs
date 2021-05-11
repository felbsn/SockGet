using SockGet.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data.Streams
{
    public class SGSocketStream : Stream
    {
        Socket socket;
        int position;
    
        internal SGSocketStream( Socket socket , int length)
        {
            this.socket = socket;
            Length = length;
            position = 0;
        }
        public override bool CanRead { get => position < Length; } 
        public override bool CanSeek { get => false; }
        public override bool CanWrite { get => false; } 
        public override long Length { get; }
        public override long Position { get => position; set => throw new NotImplementedException(); }


        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CanRead)
            {
                var receiveCount = Math.Min((int)Length - position, count);

                int read = 0;



                var readed = socket.ReadBytes(buffer , offset, receiveCount);

                position += receiveCount;

                return receiveCount;
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
