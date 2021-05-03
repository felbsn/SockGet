using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformClient
{
    public partial class LogPanel : UserControl
    {
        public LogPanel()
        {
            InitializeComponent();
        }


        public void Message( string name ,  string message)
        {
            Log($"[{name}]", message, Color.Black);
        }

        public void Error( string message)
        {
            Log($"Error", message , Color.Red);
        }

        public void Warning( string message)
        {
            Log($"Warning", message, Color.Yellow);
        }

        public void Info( string message)
        {
            Log($"Info", message , Color.Black);
        }



        protected void Log(string code ,  string str , Color color   )
        {
            if (InvokeRequired)
                Invoke(new Action(() => Log(code, str,color)));
            else
            {
                str += "\n";
                AppendText(tbMain, $"{code}:{str}", color);

                tbMain.SelectionStart = tbMain.Text.Length;
                // scroll it automatically
                tbMain.ScrollToCaret();
            }
        }

        public void AppendText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }



    }
}
