using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core.Enums
{
    [Flags]
    internal enum Token : byte
    {
        Message,
        Auth,
        Heartbeat,
        Sync
    }
}
