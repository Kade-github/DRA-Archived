using DRA_PLUGIN.Game;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace DRA_PLUGIN.Handlers 
{
    class RoundEvent : IEventHandlerWaitingForPlayers, IEventHandlerRoundEnd
    {
        private readonly DesktopRemoteAdmin plugin;
        public RoundEvent(DesktopRemoteAdmin plugin) => this.plugin = plugin;

        public void OnRoundEnd(RoundEndEvent ev) => Variables.canGetPlayers = false;

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev) => Variables.canGetPlayers = true;
    }
}
