using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.Managers.PatchManager;
using Nexus.API;

namespace PlayerlistNoSeamless.Patches
{
    [PatchShim]
    public static class MyPlayerCollectionPatch
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private static List<NexusAPI.Player> onlinePlayers = new List<NexusAPI.Player>();
        private static List<NexusAPI.Server> onlineServers = new List<NexusAPI.Server>();
        
        public static void Patch(PatchContext ctx)
        {
            //Filter out players currently connected to the server we are running on
            //with the other players, Grab their info and store in a list to use in our prefix method
            
            MethodInfo target = typeof(MyPlayerCollection).GetMethod("AddPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
            ctx.GetPattern(target).Prefixes.Add(typeof(MyPlayerCollectionPatch).GetMethod(nameof(GetOnlinePlayersPrefix), BindingFlags.Static | BindingFlags.Public));
            
            
            _log.Info("Patched MyPlayerCollection");
        }
        
        public static bool GetOnlinePlayersPrefix(MyPlayerCollection __instance, IEnumerable<MyPlayer> __result)
        {
            var m_players = typeof(MyPlayerCollection).GetField("m_players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

            var m_playersValues = m_players.GetType().GetMethod("Values")?.Invoke(m_players, null);
            
            __result = (IEnumerable<MyPlayer>) m_playersValues;
            return false;
        }
        
        /*public static bool AddPlayerPrefix(MyPlayerCollection __instance, MyPlayer.PlayerId playerId, MyPlayer newPlayer)
        {
            _log.Error($"Player joined. {newPlayer.DisplayName} {newPlayer.Id.SteamId} {newPlayer.Id.SerialId} {newPlayer.Id.ToString()} {newPlayer.Id.ToString()}");
            
            onlinePlayers = NexusAPI.GetAllOnlinePlayers();
            onlineServers = NexusAPI.GetAllServers();

            var currentServer = NexusAPI.GetThisServer();
            
            //Filter out players currently connected to the server we are running on
            onlinePlayers = onlinePlayers.Where(x => x.OnServer != currentServer.ServerID).ToList();
            
            
            var m_players = typeof(MyPlayerCollection).GetField("m_players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            
             if (Sync.IsServer && MyVisualScriptLogicProvider.PlayerConnected != null)
                MyVisualScriptLogicProvider.PlayerConnected(newPlayer.Identity.IdentityId);
            newPlayer.Identity.LastLoginTime = DateTime.Now;
            newPlayer.Identity.BlockLimits.SetAllDirty();
            
        
            
            
            //Loop through our created list and add player info
            m_players.GetType().GetMethod("TryAdd")?.Invoke(m_players, new object[] { playerId, newPlayer });

            //OnPlayersChanged(true, playerId);
            var OnPlayersChangedMethod = typeof(MyPlayerCollection).GetMethod("OnPlayersChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            OnPlayersChangedMethod?.Invoke(__instance, new object[] { true, playerId });

            if (MySession.Static.LocalHumanPlayer != null && newPlayer.IsRealPlayer && !newPlayer.IsWildlifeAgent && newPlayer.PlatformIcon?.Length > 0 && newPlayer.Id != MySession.Static.LocalHumanPlayer.Id)
            {
                switch (newPlayer.PlatformIcon.ToCharArray()[0])
                {
                    case VRage.GameServices.PlatformIcon.XBOX:
                        {
                            if (!MySession.Static.LocalHumanPlayer.EncounteredXboxPlayersIDs.Contains(newPlayer.Id.SteamId))
                            {
                                MySession.Static.LocalHumanPlayer.EncounteredXboxPlayersIDs.Add(newPlayer.Id.SteamId);
                            }
                        }
                        break;
                    case VRage.GameServices.PlatformIcon.PS:
                        {
                            if (!MySession.Static.LocalHumanPlayer.EncounteredPSPlayersIDs.Contains(newPlayer.Id.SteamId))
                            {
                                MySession.Static.LocalHumanPlayer.EncounteredPSPlayersIDs.Add(newPlayer.Id.SteamId);
                            }
                        }
                        break;
                    case VRage.GameServices.PlatformIcon.PC:
                        {
                            if (!MySession.Static.LocalHumanPlayer.EncounteredPCPlayersIDs.Contains(newPlayer.Id.SteamId))
                            {
                                MySession.Static.LocalHumanPlayer.EncounteredPCPlayersIDs.Add(newPlayer.Id.SteamId);
                            }
                        }
                        break;
                }
            }
            return false;
        }*/
    }
}