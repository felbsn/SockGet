using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Utils
{
    public static class Util
    {
        // merging arrays has never been easier  ( -_-) 
        public static byte[] MergeBytes(params byte[][] arrays)
        {
            if (arrays.Length == 0)
            {
                return null;
            }

            var totalLength = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                totalLength += arrays[i].Length;
            }

            var arr = new byte[totalLength];
            var arrIndex = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                Array.Copy(arrays[i], 0, arr, arrIndex, arrays[i].Length);
                arrIndex += arrays[i].Length;
            }
            return arr;
        }
    }

}
