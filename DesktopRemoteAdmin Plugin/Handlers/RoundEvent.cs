using DRA_PLUGIN.Game;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Threading;

namespace DRA_PLUGIN.Handlers 
{
    class RoundEvent : IEventHandlerWaitingForPlayers, IEventHandlerRoundEnd
    {
        private readonly DesktopRemoteAdmin plugin;

        static bool serverStarted = false;
        public RoundEvent(DesktopRemoteAdmin plugin) => this.plugin = plugin;

        public void OnRoundEnd(RoundEndEvent ev) => Variables.canGetPlayers = false;

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev) {

            if (!serverStarted)
            {
                serverStarted = true;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    tcpServer.plugin = plugin;
                    tcpServer.StartServer();
                }).Start();
            }

            Variables.canGetPlayers = true;

        }
    }
}
