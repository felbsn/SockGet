﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class DisconnectedEventArgs
    {
        public DisconnectedEventArgs(string reason)
        {
            Reason = reason;
        }
        public string Reason { get; }
    }
}
