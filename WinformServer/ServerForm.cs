using SockGet.Data;
using SockGet.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformServer
{
    public partial class ServerForm : Form
    {

        SGServer server;


        public ServerForm()
        {
            InitializeComponent();
            server = new SGServer();

            server.ClientAuthRequested += (s, e) =>
            {
                logPanel.Info ("Auth Request:" + e.Client.Tags["name"] + " Token:" + string.Concat(e.Token.Take(64)));
            };

            server.ClientConnected += (s, e) =>
            {
                logPanel.Info("Client connected " + e.Client.Tags["name"]);

                Invoke(new Action(() => lblConnectionCount.Text = server.Clients.Count.ToString()));

                e.Client.DataReceived += (ss, ee) =>
                {
                    if (ee.Data.Body != null && ee.Data.Body.Contains("cef"))
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

                            c.Message(ee.Data.Head, ee.Data.Body);
                        }
                        ee.Response = Response.From("Message recevied by", receivers);
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
