using DesktopRemoteAdmin_UI.Libs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopRemoteAdmin_UI
{
    public partial class PluginBox : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        static string ip = "127.0.0.1";
        static int port = 7790;
        string pluginId = "";

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public PluginBox(string setIp, int setPort)
        {
            InitializeComponent();
            ip = setIp;
            port = setPort;
        }

        private void Btn_Close_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void InnerRim_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                pluginId = listBox1.GetItemText(listBox1.SelectedValue).Split('|')[3];
                NetworkStream s = Tcp.Connect(ip, port);
                Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|getPlugins|{Main.GetCurrentTime()}", s);
                string[] data = Tcp.Recieve(s);
                string data2 = string.Join("\n", data);

                string[] data3 = data2.Split('\n');
                listBox1.Items.Clear();
                foreach (string strin in data3)
                {
                    listBox1.Items.Add(strin);
                }
            }
            catch
            {
                MessageBox.Show("Failed to get plugins...", "DRA Connection");
                Close();
            }
        }
    }
}
