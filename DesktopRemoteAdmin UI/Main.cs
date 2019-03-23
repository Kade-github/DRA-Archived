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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopRemoteAdmin_UI
{
    public partial class Main : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        static string ip = "127.0.0.1";
        static int port = 7790;
        static string player { get; set; }

        public Main(string setIp, string setPort)
        {
            InitializeComponent();
            ip = setIp;
            port = int.Parse(setPort);
        }

        private void Btn_Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InnerRim_Paint(object sender, PaintEventArgs e)
        {

        }
        public async void RefreshPlayers()
        {
            while (true)
            {
                try
                {
                    label5.Text = "Last Refresh: " + GetCurrentTime();
                listBox1.Items.Clear();
                listBox1.Items.Add("Refreshing User List.");
                    Thread.Sleep(1000);
                    NetworkStream s = Tcp.Connect(ip,port);

                Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|getPlayers|{GetCurrentTime()}",s);

                string[] data = Tcp.Recieve(s);

                    if (data[0] == "false")
                        MessageBox.Show("Failed to connect with the server,\nReason: Bad Password!", "DRA Connection");
                    else if (data[0] == "notStarted")
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.Add("The server has not fully started!");
                    }
                    else if (data[0] == "timeSkip")
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.Add("Your time skipped! Retrying in 10s");
                    }
                    else
                    {
                        string data2 = string.Join("\n", data);

                        string[] data3 = data2.Split('\n');
                        listBox1.Items.Clear();
                        foreach (string strin in data3)
                        {
                            listBox1.Items.Add(strin);
                        }
                    }
                Thread.Sleep(10000);
                }
                catch
                {
                    MessageBox.Show("Failed to connect!", "DRA Connection");
                    Application.Exit();
                }
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                RefreshPlayers();
            }).Start();

        }

        private void ListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                player = listBox1.GetItemText(listBox1.SelectedValue);
                NetworkStream s = Tcp.Connect(ip,port);
                Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|getPlayerInfo|{player}|{GetCurrentTime()}", s);
                string[] data = Tcp.Recieve(s);
                if (data[0] == "false")
                    MessageBox.Show("Failed to connect with the server,\nReason: Bad Password!", "DRA Connection");
                else if (data[0] == "pNotFound")
                    MessageBox.Show("That is not a Player!","DRA Commands");
                else
                    richTextBox1.Text = data[0];
            }
            catch
            {
                MessageBox.Show("That is not a Player!", "DRA Commands");
            }

        }

        private void Button24_Click(object sender, EventArgs e)
        {
            try
            {
                NetworkStream s = Tcp.Connect(ip,port);
                Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|getPlayerInfo|{player}|{GetCurrentTime()}", s);
                string[] data = Tcp.Recieve(s);
                if (data[0] == "false")
                    MessageBox.Show("Failed to connect with the server,\nReason: Bad Password!", "DRA Connection");
                else if (data[0] == "pNotFound")
                    MessageBox.Show("Player is Invalid!", "DRA Commands");
                else
                    richTextBox1.Text = data[0];
            }
            catch
            {
                MessageBox.Show("Player is Invalid!", "DRA Commands");
            }
        }
        public static void SetClass(string className)
        {
            try
            {
                NetworkStream s = Tcp.Connect(ip,port);
                Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|forceClassPlayer|{player}|{className}|{GetCurrentTime()}", s);
                string[] data = Tcp.Recieve(s);
                if (data[0] == "true")
                    MessageBox.Show("Success!", "DRA Commands");
                else
                    MessageBox.Show("Failed!", "DRA Commands");
            }
            catch
            {
                MessageBox.Show("Failed to Connect!", "DRA Commands");
            }
        }

        #region ForceClass
        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            SetClass("SPECTATOR");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SetClass("CLASSD");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SetClass("SCIENTIST");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            SetClass("FACILITY_GUARD");
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            SetClass("NTF_SCIENTIST");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            SetClass("NTF_CADET");
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            SetClass("NTF_LIEUTENANT");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            SetClass("NTF_COMMANDER");
        }
        private void Button8_Click(object sender, EventArgs e)
        {
            SetClass("SCP_173");
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            SetClass("SCP_049");
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            SetClass("SCP_106");
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            SetClass("SCP_049_2");
        }

        private void Button12_Click(object sender, EventArgs e)
        {
            SetClass("SCP_079");
        }

        private void Button13_Click(object sender, EventArgs e)
        {
            SetClass("SCP_939_53");
        }

        private void Button14_Click(object sender, EventArgs e)
        {
            SetClass("SCP_939_89");
        }

        private void Button15_Click(object sender, EventArgs e)
        {
            SetClass("TUTORIAL");
        }
        #endregion

        private void Button17_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|kickPlayer|{player}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button19_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|infectPlayer|{player}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button20_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|killPlayer|{player}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button16_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|killPlayer|{player}|{textBox1.Text}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button18_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|sendPBC|{player}|{textBox2.Text}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button22_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|restartRound|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button23_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|broadcast|{textBox4.Text}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button25_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|nuke|true|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button26_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|nuke|false|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button27_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|spawnNTF|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button28_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip,port);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|spawnCI|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        public static int GetCurrentTime()
        {
            
            DateTime currentTime = DateTime.Now;
            return currentTime.Minute;
        }

        private void Button21_Click(object sender, EventArgs e)
        {
            SetClass("SCP_096");
        }

        private void Button30_Click(object sender, EventArgs e)
        {
            NetworkStream s = Tcp.Connect(ip, port);
            string args = textBox3.Text.Substring(textBox3.Text.Split(' ')[0].Count() - 1);
            MessageBox.Show("Command: " + textBox3.Text.Split(' ')[0] + " | Args: " + args);
            Tcp.SendData($"cmd|{Crypto.EncryptStringAES("dudeIdkDecryptThisSheitLmAO", Variables.CachePassword)}|runCMD|{textBox3.Text.Split(' ')[0]}|{args}|{GetCurrentTime()}", s);

            string[] data = Tcp.Recieve(s);

            if (data[0] == "true")
                MessageBox.Show("Done!", "DRA Commands");
            else
                MessageBox.Show("Failure!", "DRA Commands");
        }

        private void Button31_Click(object sender, EventArgs e)
        {
            SetClass("UNASSIGNED");
        }
    }
}
