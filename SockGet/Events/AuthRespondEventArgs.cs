using SockGet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class AuthRespondEventArgs : EventArgs
    {
        public AuthRespondEventArgs(string head , string body, bool isRejected)
        {
            Message = head;
            Body = body;
            IsRejected = isRejected;
        }
        public string Message { get; private set; }
        public string Body { get; private set; }
        public bool IsRejected { get;  }
        public Dictionary<string, string> Options { get; } = new Dictionary<string, string>();
    }
}
