using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core.Extensions
{
    internal static class StringExtensions
    {
        public static Stream ToStream(this string value)
        {
            return value == null ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

    }
}
