using SockGet.Data;
using SockGet.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformServer
{
    public partial class ServerForm : Form
    {

        SgServer server;


        public ServerForm()
        {
            InitializeComponent();
            server = new SgServer();


            //server.UseHeartbeat = true;
            server.HeartbeatInterval = 5_000;
            server.HeartbeatTimeout = 5_000;

            server.ClientAuthRequested += (s, e) =>
            {
                logPanel.Info ("Auth Request:" + e.Client.Tags["name"] + " Token:" + string.Concat(e.Token.Take(64)));
            };

            server.ClientConnected += (s, e) =>
            {
                logPanel.Info("Client connected " + e.Client.Tags["name"]);

                e.Client["peki"] = "test"; 

                Invoke(new Action(() => lblConnectionCount.Text = server.Clients.Count.ToString()));

                e.Client.DataReceived += (ss, ee) =>
                {
                    if(ee.Data.Head == "file")
                    {
                        logPanel.Info("file received from" + e.Client.Tags["name"]);
                        ee.Data.AsFile("TempFile.dat");

                        var fs = new FileStream("TempFile.dat", FileMode.Open);
                        ee.Response = Response.From("file", fs);

                        return;
                    }

                    var content = ee.Data.AsString();
                    if (content != null && content.Contains("cef"))
                    {
                        logPanel.Error(ee.Data.Head + " Message rejected" );
                        ee.Response = Response.Error("some error occurred");
                    }else
                    {
                        string receivers = "";
                        foreach (var c in server.Clients)
                        {
                            if (c != e.Client)
                                receivers +=c["name"] + ",";
                            var msg = new SockGet.Data.Message().Load(ee.Data.Head, content);
                            c.Send(msg);
                        }
                        ee.Response = Response.Ok("Message recevied by", receivers);
                    }
                };
            };

            server.ClientDisconnected += (s, e) =>
            {
                Invoke(new Action(() => lblConnectionCount.Text = server.Clients.Count.ToString()));
                logPanel.Info("Client disconnected "  + e.Client.Tags["name"]);
            };

            server.Started += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    btnStart.Text = "Stop";
                    logPanel.Info("Server is running....");
                }));
         
            };

            server.Stopped += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    btnStart.Text = "Start";
                    logPanel.Info("Stopped");
                }));

            };

            Load += (s, e) =>
            {
                btnStart.PerformClick();
            };
        }


        bool connected = false;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if(connected)
            {
                server.Stop();
                connected = false;
            }
            else
            {
                if(int.TryParse(tbPort.Text , out var port))
                {
                    try
                    {
                        server.Serve(port);

                        connected = true;
                    }
                    catch (Exception ex)
                    {
                        logPanel.Error(ex.Message + "\r" + ex.StackTrace);
                    }
                }
            }
        }

        private void lblConnectionCount_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
