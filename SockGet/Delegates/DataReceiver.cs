using SockGet.Client;
using SockGet.Data.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Delegates
{
    public delegate object DataReceiver(string head, string type, SGSocketStream stream);
}
