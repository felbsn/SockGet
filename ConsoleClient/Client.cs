using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SockGet.Client;

namespace ConsoleClient
{
    class Client
    {
        static void Log(string message)
        {
            Console.WriteLine($"[Log]:{message}");
        }
        static void Message(string name, string message)
        {
            Console.WriteLine($"({name}):{message}");
            Console.Write($">");
        }
        static void Info(string message)
        {
            Console.WriteLine($"[Info]:{message}");
        }
        static void Error(string message, string details)
        {
            Console.WriteLine($"[Error]:{message} - {details}");
        }

        static void Main(string[] args)
        {
            var name = new Random().Next(1000, 10000).ToString();

            try
            {
                var client = new SgClient();

                client.AuthToken = name;
                client.Tags["name"] = "client_" + name;

                Info($"Starting client {name}");

                client.AuthRespond += (s, e) =>
                {
                    if (e.IsRejected)
                    {
                        Info("Auth rejected");
                    }
                    else
                        Info("Auth successful");
                };

                client.Disconnected += (s, e) =>
                {
                    Info("Disconnected");
                    client.ReconnectAsync(int.MaxValue);
                };
 
                client.Connected += (s, e) => Info("Connected to server");
              

                var res = client.Connect(9999);

                client.DataReceived += (s, e) =>
                {
                    if (e.Data.Head != name)
                    {
                        Message(e.Data.Head, e.Data.AsString());
                    }
                };

                if (client.IsConnected())
                {
                    while (true)
                    {
                        Console.Write(">");
                        var line = Console.ReadLine();

                        var response = client.Request(new SockGet.Data.Message().Load(name, line));
                        if(response.Status == SockGet.Enums.Status.OK)
                        {
                            Log("message was successfully sent");
                        }else
                        {
                            Log("message send respond with an error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.StackTrace);
            }

            Console.WriteLine("finished");
            Console.ReadLine();
        }
    }
}
