using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Serialization
{
    public interface ISerializer
    {
         byte[] Serialize(object o);
         T Deserialize<T>(byte[] data);
    }
}
