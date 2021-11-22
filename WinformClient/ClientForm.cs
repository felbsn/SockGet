using SockGet.Client;
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

namespace WinformClient
{
    public partial class ClientForm : Form
    {
        SgClient client;
        string name;

        public ClientForm()
        {
            InitializeComponent();
            
            name = new Random().Next(1000, 10000).ToString();

            Text = "Client " + name;

            client = new SgClient();
            client.Receiver = (h, i, s) =>
            {
                var ms = new MemoryStream();
                s.CopyTo(ms);
                return ms;
            };

            client.Tags["name"] = name;

            client.Connected += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    tbPort.ReadOnly = true;
                    tbAddress.ReadOnly = true;
                    logPanel.Info("Connected");
                    tbConnect.Text = "Connected";
                }));
            };
            client.Disconnected += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    tbAddress.ReadOnly = false;
                    tbPort.ReadOnly = false;
                    logPanel.Info("Disconnected:"+e.Reason);
                    tbConnect.Text = "Connect";
                }));
               
                connected = false;

            };
            client.AuthRespond += (s, e) =>
            {
                if (e.IsRejected)
                {
                    logPanel.Error("Auth Rejected");
                }
                else
                    logPanel.Info("Auth Successful");
            };
            client.DataReceived += (s, e) =>
            {
                if(e.Data.Head == "file")
                {
                    logPanel.Message("incoming file size ", e.Data.AsStream().Length.ToString());
                }
                else

                logPanel.Message(e.Data.Head, e.Data.AsString());
            };

           
            lblName.Text = name;

            tbMessage.AutoCompleteMode = AutoCompleteMode.Append;
            tbMessage.AutoCompleteCustomSource = new AutoCompleteStringCollection();
        }


        bool connected = false;
        private async void tbConnect_Click(object sender, EventArgs e)
        {
            if(connected)
            {
                client.Close();
                tbConnect.Text = "Connect";
            }
            else
            {
                tbConnect.Text = "Connecting...";
                tbConnect.Enabled = false;
                if (int.TryParse(tbPort.Text, out var port))
                {
                    bool res = false;

                    try
                    {
                        if (string.IsNullOrEmpty(tbAddress.Text))
                        {
                            res = await client.ConnectAsync(port, 0);
                        }
                        else
                        {
                            res = await client.ConnectAsync(tbAddress.Text, port, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex;
                    }

                    if(res)
                    {
                        connected = true;       
                        tbConnect.Text = "Connected";
                        logPanel.Info("Connected successfully.");
                    }else
                    {
                        tbConnect.Text = "Connect";
                        logPanel.Error("Unable to connect server.");
                    }

                    tbConnect.Enabled = true;



                }
                else
                    MessageBox.Show("Given port is invalid");
            }
        }

        private void btnSelectAuth_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                var res = fd.ShowDialog();
                if (res == DialogResult.OK)
                {
                    tbAuthFile.Text = Path.GetFileName(fd.FileName);
                    var str = File.ReadAllText(fd.FileName);
                    if (str.Length < 65535) 
                        client.AuthToken = str;
                }
            }
        }

        private  async void btnSend_Click(object sender, EventArgs e)
        {
            //await
            var response =  client.Request(new SockGet.Data.Message().Load(name, tbMessage.Text));//Async
            if (response.Status == SockGet.Enums.Status.OK)
            {
                logPanel.Info("Response:'" + response.Head + "', body:'" + response.AsString()+"'");
            }else
            {
                logPanel.Error("Response returned error");
            }
        }

        private void tbMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                btnSend_Click(this, e);
                tbMessage.Text = "";
            }else
            {
                
            }    

        }

        private void btnTagTest_Click(object sender, EventArgs e)
        {
            var rng = new Random();
            client["a"] = rng.Next(99).ToString();
            client["b"] = rng.Next(99).ToString();

            logPanel.Info($"Before");
            foreach (var pair in client.Tags)
            {
                logPanel.Info($"[{pair.Key}]:{pair.Value}");
            }

            client.Sync();

            logPanel.Info($"After");
            foreach (var pair in client.Tags)
            {
                logPanel.Info($"[{pair.Key}]:{pair.Value}");
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                var res = fd.ShowDialog();
                if (res == DialogResult.OK)
                {
                    using (var fs = new FileStream(fd.FileName , FileMode.Open))
                    {
                        logPanel.Info("file sent " + Path.GetFileName(fd.FileName));
                        client.Request("file", fs);
                    }
                    //tbAuthFile.Text = Path.GetFileName(fd.FileName);
                    //var str = File.ReadAllText(fd.FileName);
                    //if (str.Length < 65535)
                    //    client.AuthToken = str;
                }
            }
        }
    }
}
