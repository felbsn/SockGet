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
        SGClient client;
        string name;

        public ClientForm()
        {
            InitializeComponent();
            
            name = new Random().Next(1000, 10000).ToString();

            Text = "Client " + name;

            client = new SGClient();

            client.Tags["name"] = name;

            client.Connected += (s, e) =>
            {
                logPanel.Info("Connected");
            };
            client.Disconnected += (s, e) =>
            {
                logPanel.Info("Disconnected");
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
                logPanel.Message(e.Data.Head, e.Data.Body);
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
                    if (string.IsNullOrEmpty(tbAddress.Text))
                    {
                        res = await client.ConnectAsync(port , 0);
                    }
                    else
                    {
                        res = await client.ConnectAsync(tbAddress.Text, port , 0);
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
                    client.AuthToken = str;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            client.SendMessage(name, tbMessage.Text);
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
    }
}
