using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SockGet.Data;
using SockGet.Server;


namespace ConsoleServer
{
    class Server
    {
        static void Log(string message)
        {
            Console.WriteLine($"[Log]:{message}");
        }
        static void Message(string name, string message)
        {
            Console.WriteLine($"({name}):{message}");
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
            try
            {
                var server = new SGServer();

                server.Serve(9999);

                server.ClientAuthRequested += (s, e) =>
                {
                    bool reject = false; // new Random().NextDouble() > 0.5;

                    if (server.Clients.Count > 2)
                        reject = true;

                    Info($"Auth requested with '{e.Token}', reject status: {(reject ? "Rejected" : "Accepted")}");
                    e.Reject = reject;

                };
                server.ClientConnected += (s, e) =>
                {
                    string name = null;
                    e.Client.Tags?.TryGetValue("name", out name);
                    Info($"Client connected {name ?? "Unnamed"}");
                    e.Client.DataReceived += (ss, ee) =>
                    {
                        Message(ee.Data.Head, ee.Data.Body);

                        if(ee.IsResponseRequired)
                        {
                            if(ee.Data.Body?.Contains("x") == true)
                            {


                                ee.Response = Response.Cancel();

                            }
                            else
                            {
                                foreach (var client in server.Clients)
                                {
                                    client.Message(ee.Data.Head + "X", ee.Data.Body);
                                }

                                ee.Response = Response.OK();
                            }

                        }

                  
                    };
                };

                while (true)
                {
                    var line = Console.ReadLine();

                    foreach (var client in server.Clients)
                    {
                        client.Message("server", line);
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message, ex.StackTrace);
            }


            Console.WriteLine("end of this");
            Console.ReadLine();
        }
    }
}
