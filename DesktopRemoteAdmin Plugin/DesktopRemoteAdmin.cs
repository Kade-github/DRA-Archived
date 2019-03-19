using DRA_PLUGIN.Handlers;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.Lang;
using System.Threading;

namespace DRA_PLUGIN
{
    [PluginDetails(
        author = "Kade",
        name = "Desktop Remote Admin",
        description = "A Desktop version of Remote Admin.",
        id = "kade.dra",
        version = "1.4",
        SmodMajor = 3,
        SmodMinor = 3,
        SmodRevision = 1
        )]
    public class DesktopRemoteAdmin : Plugin
    {
        public override void OnDisable()
        {
            Info("Ended Service...");
        }

        public override void OnEnable()
        {
                Info("\n---------------\nD E S K T O P  R E M O T E  A D M I N\nCreated by Kade#6969\nThe best Desktop Tool for remote adminstration!\n---------------");
        }

        public override void Register()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                tcpServer.plugin = this;
                tcpServer.StartServer();
            }).Start();
            // Configs
            AddConfig(new ConfigSetting("dra_password", "", SettingType.STRING, true, "The password used to login to the UI."));
            AddConfig(new ConfigSetting("dra_status", true, SettingType.BOOL, true, "Set it to false to disable to plugin"));
            AddConfig(new ConfigSetting("dra_port", 7790, SettingType.NUMERIC, true, "The port for it to connect."));
            AddConfig(new ConfigSetting("dra_logs", false, SettingType.BOOL, true, "How much logs are annoying (true = hella annoying)"));
            // Events
            AddEventHandlers(new RoundEvent(this));
        }
    }
}
