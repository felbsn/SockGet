using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Data
{
    public class SGMergedStream : Stream
    {
        public Stream[] Streams { get; protected set; }

        int index;
        int total_position;
        int current_position;
        public SGMergedStream(params Stream[] streams)
        {
            index = 0;
            total_position = 0;
            current_position = 0;
            Streams = streams.Where(s => s != null && s.Length != 0).ToArray();
            Length = Streams.Sum(s => s.Length);
        }

        public override bool CanRead => index < Streams.Length ;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;
        public override long Length { get; }
        public override long Position { get => total_position; set => throw new NotImplementedException(); }


        public override int Read(byte[] buffer, int offset, int count)
        {
            if(CanRead)
            {
                var stream = Streams[index];
                var receivedCount = stream.Read(buffer, offset, count);
                total_position += receivedCount;

                if(stream.Position >= stream.Length)
                {
                    index++;
                }

                return receivedCount;

            }else
            {
                return 0;
            }
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
    }
}
