using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Extensions
{
    public static class SocketExtensions
    {
        public static void ReadBytes(this Socket socket , int count , out byte[] buffer)
        {
            buffer = new byte[count];
            int received = 0;
            int read = 0;
            
            while(received + read < count)
            {
                read = socket.Receive(buffer, received, count - received, SocketFlags.None);
                received += read;
                read = 0;
            }
        }
        public static int ReadBytes(this Socket socket, byte[] buffer , int offset, int count)
        { 
            int received = 0;

            while (received  < count)
            {
                int read = socket.Receive(buffer, received + offset, count - received, SocketFlags.None);
                if (read == 0)
                    return 0;

                received += read;
            }
            return received;
        }
    }
}
