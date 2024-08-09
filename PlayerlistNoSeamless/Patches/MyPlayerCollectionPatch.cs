using System;
using System.Linq;
using System.Reflection;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.Managers.PatchManager;

namespace PlayerlistNoSeamless.Patches
{
    [PatchShim]
    public static class MyPlayerCollectionPatch
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        public static void Patch(PatchContext ctx)
        {

            if (!TorchBase.Instance.Plugins.Plugins.ContainsKey(Guid.Parse("28a12184-0422-43ba-a6e6-2e228611cca5")))
            {
                _log.Info("Nexus not installed, skipping patching MyPlayerCollection.");
                return;
            }

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nexusAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "Nexus");
            
            if (nexusAssembly == null)
            {
                throw new Exception("Nexus assembly not found.");
            }
            
            var nexusAPIType = nexusAssembly.GetType("Nexus.API.NexusAPI");
            var GetAllOnlinePlayersMethod = nexusAPIType.GetMethod("GetAllOnlinePlayers", BindingFlags.Public | BindingFlags.Static);
            var GetAllServersMethod = nexusAPIType.GetMethod("GetAllServers", BindingFlags.Public | BindingFlags.Static);
            
            if (GetAllOnlinePlayersMethod == null)
            {
                _log.Error("Nexus API not found, skipping patching MyPlayerCollection.");
                return;
            }
            
            if (GetAllServersMethod == null)
            {
                _log.Error("Nexus API not found, skipping patching MyPlayerCollection.");
                return;
            }
            
            
            var OnlinePlayers = GetAllOnlinePlayersMethod.Invoke(null, null);
            var ConnectedServers = GetAllServersMethod.Invoke(null, null);
            
            _log.Info($"OnlinePlayers: {OnlinePlayers}");
            
            
            //Filter out players currently connected to the server we are running on
            //with the other players, Grab their info and store in a list to use in our prefix method
            
            MethodInfo target = typeof(MyPlayerCollection).GetMethod("AddPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
            ctx.GetPattern(target).Prefixes.Add(typeof(MyPlayerCollectionPatch).GetMethod(nameof(AddPlayerPrefix), BindingFlags.Static | BindingFlags.Public));
            
            
            _log.Info("Patched MyPlayerCollection");
        }
        
        public static bool AddPlayerPrefix(MyPlayerCollection __instance, MyPlayer.PlayerId playerId, MyPlayer newPlayer)
        {
            _log.Error($"Player joined. {newPlayer.DisplayName} {newPlayer.Id.SteamId} {newPlayer.Id.SerialId} {newPlayer.Id.ToString()} {newPlayer.Id.ToString()}");
            var m_players = typeof(MyPlayerCollection).GetField("m_players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            
             if (Sync.IsServer && MyVisualScriptLogicProvider.PlayerConnected != null)
                MyVisualScriptLogicProvider.PlayerConnected(newPlayer.Identity.IdentityId);
            newPlayer.Identity.LastLoginTime = DateTime.Now;
            newPlayer.Identity.BlockLimits.SetAllDirty();
            
            
            /*if (!m_players.TryAdd(playerId, newPlayer))
            {
                VRage.MyDebug.Assert(false, "Player already in collection.");
            }*/
            
            
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
        }
    }
}