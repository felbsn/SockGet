using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core.Enums
{
    internal enum Token : byte
    {
        Message,
        AuthRequest,
        AuthResponse,
        TagRequest,
        TagSync,
        Request,
        Response,
    }
}
