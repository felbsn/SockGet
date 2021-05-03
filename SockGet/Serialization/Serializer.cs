using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SockGet.Serialization
{
    public static class Serializer
    {
        public static string Serialize(object o)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            };
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(o, settings);
            return data;
        }
        public static T Deserialize<T>(string data)
        {
            if (data == null)
                return default;
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
            };
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data, settings);
            return obj;
        }
        public static object Deserialize(string data , string type)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
            };

            var t = Type.GetType(type);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(data, t, settings);
            return obj;
        }
    }
}
