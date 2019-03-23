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
using Smod2;

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
            if (plugin.GetConfigString("dra_password") == null)
            {
                plugin.Error("The config option 'dra_password' is not set, this is incredibly unsafe! The plugin will not run untill this is set.");
                plugin.pluginManager.DisablePlugin(plugin);
                return;
            }
            if (!plugin.GetConfigBool("dra_status"))
            { plugin.pluginManager.DisablePlugin(plugin); return; }
            plugin.Info($"Started TCP Server on port {plugin.GetConfigInt("dra_port")}!");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(Commander, client);
            }
        }

        private static int GetCurrentTime()
        {

            DateTime currentTime = DateTime.Now;
            return currentTime.Minute;
        }

        private static void Commander(object obj)
        {
            bool accept = true;
            var tcpClient = (TcpClient)obj;
            if (queue > 10)
                accept = false;
            try
            {

                string ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

                NetworkStream stream = tcpClient.GetStream();
                string password = plugin.GetConfigString("dra_password");
                string[] data = Recieve(stream);

                if (!accept)
                    SendData(stream, "fullQueue");

                queue += 1;
                if (plugin.GetConfigBool("dra_logs"))
                {
                    plugin.Info("Queue: " + queue + "/10");
                    plugin.Info("Connection from " + ip);
                }
   
                switch (data[0])
                {
                    // Login
                    case "login":
                        if (plugin.GetConfigBool("dra_logs"))
                            plugin.Info("Client wanting to login!");
                        try
                        {
                            Crypto.DecryptStringAES(data[1], password);
                            if (plugin.GetConfigBool("dra_logs"))
                                plugin.Info("Login accepted!");
                            if (dic.ContainsKey(ip))
                            {
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
                                }
                            }
                            else
                                SendData(stream, "true");
                            break;
                        }
                        catch
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
                            plugin.Warn("Client tried to login, but failed!");
                            SendData(stream, "false");
                            break;
                        }
                    #region commands
                    case "cmd":
                        try
                        {
                            if (plugin.GetConfigBool("dra_logs"))
                                plugin.Info("Accepted Command Auth");
                            Crypto.DecryptStringAES(data[1], password);
                        }
                        catch
                        {
                            SendData(stream, "false");
                            break;
                        }
                        if (plugin.GetConfigBool("dra_logs"))
                            plugin.Info(GetCurrentTime() + " | Command Recieved...");
                        if (plugin.GetConfigBool("dra_logs"))
                            plugin.Info("Command: " + data[2]);
                        switch (data[2])
                        {
                            case "getPlayers":
                                if (int.Parse(data[3]) == GetCurrentTime())
                                {
                                    if (Variables.canGetPlayers)
                                    {
                                        string players = "";
                                        List<Player> a = plugin.Server.GetPlayers();
                                        foreach (var s in a)
                                        {
                                            players += $"{s.Name}\n";
                                        }
                                        SendData(stream, players);
                                        break;
                                    }
                                    else
                                    {
                                        SendData(stream, "notStarted");
                                        break;
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                            case "getPlayerInfo":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        string pData = $"Ip: {p.IpAddress}\nRank: {p.GetRankName()}\nRole: {p.TeamRole.Role}";
                                        SendData(stream, pData);
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "forceClassPlayer":
                                if (int.Parse(data[5]) == GetCurrentTime())
                                {
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
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "kickPlayer":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        p.Ban(0);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "banPlayer":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        p.Ban(int.Parse(data[4]));
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "infectPlayer":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        p.Infect(100);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "killPlayer":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        p.Kill();
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "sendPBC":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Finding Player");
                                        Player p = FindPlayer(data[3]);
                                        p.PersonalBroadcast(10, data[4], true);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "pNotFound");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "restartRound":
                                if (int.Parse(data[3]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (plugin.GetConfigBool("dra_logs"))
                                            plugin.Info("Restarting Round");
                                        plugin.Server.Round.RestartRound();
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "broadcast":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.Server.Map.Broadcast(10, data[3], true);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "nuke":
                                if (int.Parse(data[5]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        if (data[4] == "true")
                                            plugin.Server.Map.StartWarhead();
                                        else if (data[4] == "false")
                                            plugin.Server.Map.StopWarhead();
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "spawnNTF":
                                if (int.Parse(data[3]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.Server.Round.MTFRespawn(false);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "spawnCI":
                                if (int.Parse(data[3]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.Server.Round.MTFRespawn(true);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "runCMD":
                                if (int.Parse(data[5]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.pluginManager.CommandManager.CallCommand(plugin.Server, data[4], data[5].Split(' '));
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "getPlugins":
                                if (int.Parse(data[3]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        string pluginNames = "";
                                        List<Plugin> a = plugin.pluginManager.Plugins;
                                        foreach (Plugin aa in a)
                                        {
                                            pluginNames = "\n" + aa.Details.name + "|By " + aa.Details.author + "|Version " + aa.Details.version + "|" + aa.Details.id;
                                        }
                                        SendData(stream, pluginNames);
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "disablePlugin":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.pluginManager.DisablePlugin(data[3]);
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                            case "enablePlugin":
                                if (int.Parse(data[4]) == GetCurrentTime())
                                {
                                    try
                                    {
                                        plugin.pluginManager.EnablePlugin(plugin.pluginManager.GetDisabledPlugin(data[3]));
                                        SendData(stream, "true");
                                    }
                                    catch
                                    {
                                        SendData(stream, "false");
                                    }
                                }
                                else
                                {
                                    SendData(stream, "timeSkip");
                                    break;
                                }
                                break;
                        }
                        break;
                        #endregion
                }
            }
            catch (Exception e)
            {
                plugin.Error("Connection Failed on TCPServer's Side.\n" + e.ToString());
            }
            if (accept)
            {
                queue -= 1;
                if (plugin.GetConfigBool("dra_logs"))
                    plugin.Info("Queue: " + queue + "/10");
            }
            tcpClient.Close();
        }

        #region RoleCheck
        public static Role GetRoleFromString(string roleName)
        {
            Role a = Role.UNASSIGNED;
            switch (roleName)
            {
                case "CHAOS_INSURGENCY":
                    a = Role.CHAOS_INSURGENCY;
                    break;
                case "CLASSD":
                    a = Role.CLASSD;
                    break;
                case "FACILITY_GUARD":
                    a = Role.FACILITY_GUARD;
                    break;
                case "NTF_CADET":
                    a = Role.NTF_CADET;
                    break;
                case "NTF_COMMANDER":
                    a = Role.NTF_COMMANDER;
                    break;
                case "NTF_LIEUTENANT":
                    a = Role.NTF_LIEUTENANT;
                    break;
                case "NTF_SCIENTIST":
                    a = Role.NTF_SCIENTIST;
                    break;
                case "SCIENTIST":
                    a = Role.SCIENTIST;
                    break;
                case "SCP_049":
                    a = Role.SCP_049;
                    break;
                case "SCP_049_2":
                    a = Role.SCP_049_2;
                    break;
                case "SCP_079":
                    a = Role.SCP_079;
                    break;
                case "SCP_096":
                    a = Role.SCP_096;
                    break;
                case "SCP_106":
                    a = Role.SCP_106;
                    break;
                case "SCP_173":
                    a = Role.SCP_173;
                    break;
                case "SCP_939_53":
                    a = Role.SCP_939_53;
                    break;
                case "SCP_939_89":
                    a = Role.SCP_939_89;
                    break;
                case "TUTORIAL":
                    a = Role.TUTORIAL;
                    break;
                case "SPECTATOR":
                    a = Role.SPECTATOR;
                    break;
                case "UNASSIGNED":
                    a = Role.UNASSIGNED;
                    break;
            }
            return a;
        }
        #endregion

        public static Player FindPlayer(string name)
        {
            Player p;
            List<Player> a = plugin.Server.GetPlayers(name);
            p = a.OrderBy(ply => ply.Name.Length).First();
            return p;
        }
        public static bool SendData(NetworkStream stream, string data)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data + "|");

                stream.Write(bytes, 0, bytes.Length);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string[] Recieve(NetworkStream stream)
        {
            try
            {
                byte[] bytes = new byte[1024];

                stream.Read(bytes, 0, bytes.Length);

                string[] data = Encoding.UTF8.GetString(bytes).Split('|');

                stream.Flush();

                return data;
            }
            catch (Exception)
            {
                return new string[] { "failed to recieve message" };
            }

        }
    }
}
