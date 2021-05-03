using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Client
{
    public delegate object DataReceiver(string head, string type, SGSocketStream stream);
}
