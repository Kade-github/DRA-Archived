using DRA_PLUGIN.Game;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Smod2.API;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Timers;

namespace DRA_PLUGIN
{
    class tcpServer
    {
        public static DesktopRemoteAdmin plugin;

        static int queue = 0;

        static Dictionary<string, int> dic = new Dictionary<string, int>();

        static Dictionary<string, DateTime> bans = new Dictionary<string, DateTime>();

        public static void StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, plugin.GetConfigInt("dra_port"));
            listener.Start();
            plugin.Info($"Started TCP Server on port {plugin.GetConfigInt("dra_port")}!");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(Commander, client);
            }
        }

        private static void Unban(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Hello World!");
        }

        private static void Commander(object obj)
        {
            var tcpClient = (TcpClient)obj;
            try
            {

                string ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

                NetworkStream stream = tcpClient.GetStream();

                string[] data = Recieve(stream);

                if (queue > 10)
                    SendData(stream, "fullQueue");

                queue += 1;
                plugin.Info("Connection from " + ip);


                plugin.Debug("Command: " + string.Join("|", data));

                string password = plugin.GetConfigString("dra_password");

                switch (data[0])
                {
                    // Login
                    case "login":
                        plugin.Info("Client wanting to login!");
                        if (data[1] != password)
                        {
                            // Banning
                            if (dic.ContainsKey(ip))
                                if (dic[ip] == 3)
                                {
                                    DateTime currentTime = DateTime.Now;
                                    if (bans.ContainsKey(ip))
                                    {
                                        int result = DateTime.Compare(currentTime, bans[ip]);
                                        if (result > 0)
                                        {
                                            dic.Remove(ip);
                                            bans.Remove(ip);
                                        }
                                        else
                                            SendData(stream, "banned");
                                    }
                                    else
                                    {
                                        SendData(stream, "banned");
                                        bans.Add(ip, currentTime.AddHours(2));
                                    }
                                }
                                else
                                    dic.Add(ip, dic[ip] + 1);
                            else
                                dic.Add(ip, 1);
                            plugin.Warn("Client tried to login, but failed! Using password " + data[1]);
                            SendData(stream, "false");
                            break;
                        }
                        plugin.Info("Login accepted!");
                        SendData(stream, "true");
                        break;
                    #region commands
                    case "cmd":
                        if (data[1] != password)
                        {
                            plugin.Warn("Client tried to use a command, but the password was incorrect!\nPassword: " + data[1]);
                            SendData(stream, "false");
                            break;
                        }
                        switch (data[2])
                        {
                            case "getPlayers":
                                string players = "";
                                List<Player> a = plugin.Server.GetPlayers();
                                foreach (Player s in a)
                                {
                                    if (players == "")
                                        players = s.Name;
                                    else
                                        players = "\n" + s.Name;
                                }
                                SendData(stream, players);
                                break;
                            case "getPlayerInfo":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    string pData = $"Ip: {p.IpAddress}\nRank: {p.GetRankName()}\nRole: {p.TeamRole.Role}";
                                    SendData(stream, pData);
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }

                                break;
                            case "forceClassPlayer":
                                try
                                {
                                    Player player = FindPlayer(data[3]);
                                    player.ChangeRole(GetRoleFromString(data[4]), true, true, false, true);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                            case "kickPlayer":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    p.Ban(0);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }
                                break;
                            case "banPlayer":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    p.Ban(int.Parse(data[4]));
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }
                                break;
                            case "infectPlayer":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    p.Infect(100);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }
                                break;
                            case "killPlayer":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    p.Kill();
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }
                                break;
                            case "sendPBC":
                                try
                                {
                                    plugin.Info("Finding Player");
                                    Player p = FindPlayer(data[3]);
                                    p.PersonalBroadcast(10, data[4], true);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "pNotFound");
                                }
                                break;
                            case "restartRound":
                                try
                                {
                                    plugin.Info("Restarting Round");
                                    plugin.Server.Round.RestartRound();
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                            case "broadcast":
                                try
                                {
                                    plugin.Server.Map.Broadcast(10, data[3], true);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                            case "nuke":
                                try
                                {
                                    if (data[3] == "true")
                                        plugin.Server.Map.StartWarhead();
                                    else if (data[3] == "false")
                                        plugin.Server.Map.StopWarhead();
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                            case "spawnNTF":
                                try
                                {
                                    plugin.Server.Round.MTFRespawn(false);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                            case "spawnCI":
                                try
                                {
                                    plugin.Server.Round.MTFRespawn(true);
                                    SendData(stream, "true");
                                }
                                catch
                                {
                                    SendData(stream, "false");
                                }
                                break;
                        }
                        break;
                        #endregion
                }
                queue -= 1;
            }
            catch (Exception e)
            {
                plugin.Error("Connection Failed on TCPServer's Side.\n" + e.ToString());
            }
            tcpClient.Close();
        }

        public static Role GetRoleFromString(string roleName)
        {
            Role role = (Role)System.Enum.Parse(typeof(Role), roleName);
            return role;
        }

        public static Player FindPlayer(string name)
        {
            Player p;
            List<Player> a = plugin.Server.GetPlayers(name);
            p = a.OrderBy(ply => ply.Name.Length).First();
            return p;
        }
        private static void SendData(NetworkStream stream, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data + "|");

            stream.Write(bytes, 0, bytes.Length);
        }

        private static string[] Recieve(NetworkStream stream)
        {
            byte[] bytes = new byte[1024];

            stream.Read(bytes, 0, bytes.Length);

            string[] data = Encoding.UTF8.GetString(bytes).Split('|');

            stream.Flush();

            return data;
        }
    }
}
