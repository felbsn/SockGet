using SockGet.Data;
using SockGet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Events
{
    public class DataReceivedEventArgs :EventArgs
    {
        public DataReceivedEventArgs(Result received, bool isResponseRequired, Response response)
        {
            Data = received;
            IsResponseRequired = isResponseRequired;
            Response = response;
        }
         
        public Result Data { get; }
        public bool IsResponseRequired { get; }
        public Response Response { get; set; }
        public Status Status { get; internal set; }
    }
}
