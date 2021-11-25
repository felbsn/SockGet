using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core.Extensions
{
    internal static class StreamExtensions
    {
        public static string ReadAll(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }

        public static void FinishRead(this Stream stream)
        {
            while (stream.CanRead)
            {
                var temp = new byte[1024];
                stream.Read(temp, 0, 1024);
            }
        }
    }
}
