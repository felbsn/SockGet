using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SockGet.Serialization
{
    public class Serializer : ISerializer
    {
        public byte[] Serialize(object o)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            };
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(o, settings);
            var data = Encoding.UTF8.GetBytes(str);
            return data;
        }
        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default;


            var str = Encoding.UTF8.GetString(data);

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(
                str,
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
                }
            );
            return obj;
        }
    }
}
