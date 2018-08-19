using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using ChatCommands;
using Permissions;
using Server.TerrainGeneration;

namespace ColonyCommands {

  [ModLoader.ModManager]
  public static class AntiGrief
  {
    public const string MOD_PREFIX = "mods.scarabol.commands.";
    public const string NAMESPACE = "AntiGrief";
    public static string MOD_DIRECTORY;
    public static string PERMISSION_SUPER = "mods.scarabol.antigrief";
    public static string PERMISSION_SPAWN_CHANGE = PERMISSION_SUPER + ".spawnchange";
    public static string PERMISSION_BANNER_PREFIX = PERMISSION_SUPER + ".banner.";
    static int SpawnProtectionRangeXPos;
    static int SpawnProtectionRangeXNeg;
    static int SpawnProtectionRangeZPos;
    static int SpawnProtectionRangeZNeg;
    static int BannerProtectionRangeX;
    static int BannerProtectionRangeZ;
    public static List<CustomProtectionArea> CustomAreas = new List<CustomProtectionArea> ();
    static int NpcKillsJailThreshold;
    static int NpcKillsKickThreshold;
    static int NpcKillsBanThreshold;
    static Dictionary<Players.Player, int> KillCounter = new Dictionary<Players.Player, int> ();

    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "protection-ranges.json"));
      }
    }

    // used only by the /top command to hide players from the scoring
    public static List<Players.Player> UnscoredPlayers = new List<Players.Player>();

    [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
    public static void OnAssemblyLoaded(string path)
    {
      MOD_DIRECTORY = Path.GetDirectoryName(path);
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, NAMESPACE + ".RegisterTypes")]
    public static void AfterItemTypesDefined ()
    {
      Log.Write("Registering commands (Anti-Grief)");
      CommandManager.RegisterCommand(new AnnouncementsChatCommand());
      CommandManager.RegisterCommand(new AntiGriefChatCommand());
      CommandManager.RegisterCommand(new BanChatCommand());
      CommandManager.RegisterCommand(new BannerNameChatCommand());
      CommandManager.RegisterCommand(new BetterChatCommand());
      CommandManager.RegisterCommand(new CleanBannersChatCommand());
      CommandManager.RegisterCommand(new ColonyCap());
      CommandManager.RegisterCommand(new DrainChatCommand());
      CommandManager.RegisterCommand(new GiveAllChatCommand());
      CommandManager.RegisterCommand(new GodChatCommand());
      CommandManager.RegisterCommand(new InactiveChatCommand());
      CommandManager.RegisterCommand(new ItemIdChatCommand());
      CommandManager.RegisterCommand(new KickChatCommand());
      CommandManager.RegisterCommand(new KillNPCsChatCommand());
      CommandManager.RegisterCommand(new KillPlayerChatCommand());
      CommandManager.RegisterCommand(new LastSeenChatCommand());
      CommandManager.RegisterCommand(new NoFlightChatCommand());
      CommandManager.RegisterCommand(new OnlineChatCommand());
      CommandManager.RegisterCommand(new PurgeAllChatCommand());
      CommandManager.RegisterCommand(new ServerPopCommand());
      CommandManager.RegisterCommand(new StuckChatCommand());
      CommandManager.RegisterCommand(new TopChatCommand());
      CommandManager.RegisterCommand(new TradeChatCommand());
      CommandManager.RegisterCommand(new TrashChatCommand());
      CommandManager.RegisterCommand(new TrashPlayerChatCommand());
      CommandManager.RegisterCommand(new TravelChatCommand());
      CommandManager.RegisterCommand(new TravelHereChatCommand());
      CommandManager.RegisterCommand(new TravelThereChatCommand());
      CommandManager.RegisterCommand(new TravelRemoveChatCommand());
      CommandManager.RegisterCommand(new WarpBannerChatCommand());
      CommandManager.RegisterCommand(new WarpChatCommand());
      CommandManager.RegisterCommand(new WarpPlaceChatCommand());
      CommandManager.RegisterCommand(new WarpSpawnChatCommand());
      CommandManager.RegisterCommand(new WhisperChatCommand());
      CommandManager.RegisterCommand(new SetJailCommand());
      CommandManager.RegisterCommand(new JailCommand());
      CommandManager.RegisterCommand(new JailReleaseCommand());
      CommandManager.RegisterCommand(new JailVisitCommand());
      CommandManager.RegisterCommand(new JailLeaveCommand());
      CommandManager.RegisterCommand(new JailRecCommand());
      CommandManager.RegisterCommand(new JailTimeCommand());
      CommandManager.RegisterCommand(new AreaShowCommand());
      CommandManager.RegisterCommand(new HelpCommand());
      CommandManager.RegisterCommand(new DeleteJobsCommand());
      CommandManager.RegisterCommand(new DeleteJobSpeedCommand());
      CommandManager.RegisterCommand(new ProductionCommand());
      CommandManager.RegisterCommand(new ColorTestCommand());
      return;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnTryChangeBlock, NAMESPACE + ".OnTryChangeBlock")]
    public static void OnTryChangeBlock (ModLoader.OnTryChangeBlockData userData)
    {
      var requestedBy = userData.RequestedByPlayer;
      if (requestedBy == null) {
        return;
      }
      var position = userData.Position;
      var spawn = TerrainGenerator.UsedGenerator.GetSpawnLocation (requestedBy);
      var ox = position.x - (int)spawn.x;
      var oz = position.z - (int)spawn.z;
      if (((ox >= 0 && ox <= SpawnProtectionRangeXPos) || (ox < 0 && ox >= -SpawnProtectionRangeXNeg)) && ((oz >= 0 && oz <= SpawnProtectionRangeZPos) || (oz < 0 && oz >= -SpawnProtectionRangeZNeg))) {
        if (!PermissionsManager.HasPermission (requestedBy, PERMISSION_SPAWN_CHANGE)) {
          if (requestedBy.IsConnected) {
            Chat.Send (requestedBy, "<color=red>You don't have permission to change the spawn area!</color>");
          }
          BlockCallback (userData);
          return;
        }
      } else {
        var homeBanner = BannerTracker.Get (requestedBy);
        if (homeBanner != null) {
          Vector3Int homeBannerLocation = homeBanner.KeyLocation;
          if (System.Math.Abs (homeBannerLocation.x - position.x) <= BannerProtectionRangeX && System.Math.Abs (homeBannerLocation.z - position.z) <= BannerProtectionRangeZ) {
            return;
          }
        }
        var checkRangeX = BannerProtectionRangeX;
        var checkRangeZ = BannerProtectionRangeZ;
        if (userData.TypeNew == BlockTypes.Builtin.BuiltinBlocks.Banner) {
          checkRangeX *= 2;
          checkRangeZ *= 2;
        }
        for (var c = 0; c < BannerTracker.GetCount (); c++) {
          Banner banner;
          if (BannerTracker.TryGetAtIndex (c, out banner)) {
            Vector3Int bannerLocation = banner.KeyLocation;
            if (System.Math.Abs (bannerLocation.x - position.x) <= checkRangeX && System.Math.Abs (bannerLocation.z - position.z) <= checkRangeZ) {
              if (banner.Owner != requestedBy && !PermissionsManager.HasPermission (requestedBy, PERMISSION_BANNER_PREFIX + banner.Owner.ID.steamID)) {
                if (requestedBy.IsConnected) {
                  if(userData.TypeNew == BlockTypes.Builtin.BuiltinBlocks.Banner)
                     {
                       int distanceX = checkRangeX - System.Math.Abs(bannerLocation.x - position.x);
                       int distanceZ = checkRangeZ - System.Math.Abs(bannerLocation.z - position.z);

                        Chat.Send(requestedBy, string.Format("<color=red>You can't place your banner here because there is another banner too close. You need to walk {0} blocks in X OR {1} blocks in Z to place your banner.</color>", distanceX, distanceZ));
                     }
                  else
                   Chat.Send (requestedBy, "<color=red>You don't have permission to change blocks near this banner!</color>");
                }
                BlockCallback (userData);
                return;
              }
              break;
            }
          }
        }
        foreach (var area in CustomAreas) {
          if (area.Contains (position) && !PermissionsManager.HasPermission (requestedBy, PERMISSION_SPAWN_CHANGE)) {
            if (requestedBy.IsConnected) {
              Chat.Send (requestedBy, "<color=red>You don't have permission to change this protected area!</color>");
            }
            BlockCallback (userData);
            return;
          }
        }
      }
    }

    static void BlockCallback (ModLoader.OnTryChangeBlockData userData)
    {
      userData.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
      userData.InventoryItemResults.Clear ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, NAMESPACE + ".AfterWorldLoaded")]
    public static void AfterWorldLoad ()
    {
      Load ();
      JailManager.Load();
	  StatisticManager.Load();
	  StatisticManager.TrackItems();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, NAMESPACE + ".OnPlayerConnected")]
    public static void OnPlayerConnectedLate (Players.Player player)
    {
      Chat.Send (player, "<color=yellow>Anti-Grief protection enabled</color>");
    }

    public static void Load ()
    {
      SpawnProtectionRangeXPos = 50;
      SpawnProtectionRangeXNeg = 50;
      SpawnProtectionRangeZPos = 50;
      SpawnProtectionRangeZNeg = 50;
      BannerProtectionRangeX = 50;
      BannerProtectionRangeZ = 50;
      CustomAreas.Clear ();
      JSONNode jsonConfig;
      if (JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        int rx;
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeX+", out rx)) {
          SpawnProtectionRangeXPos = rx;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
          SpawnProtectionRangeXPos = rx;
        } else {
          Log.Write ($"Could not get SpawnProtectionRangeX+ or SpawnProtectionRangeX from json config, using default value {SpawnProtectionRangeXPos}");
        }
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeX-", out rx)) {
          SpawnProtectionRangeXNeg = rx;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
          SpawnProtectionRangeXNeg = rx;
        } else {
          Log.Write ($"Could not get SpawnProtectionRangeX- or SpawnProtectionRangeX from json config, using default value {SpawnProtectionRangeXNeg}");
        }
        int rz;
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ+", out rz)) {
          SpawnProtectionRangeZPos = rz;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
          SpawnProtectionRangeZPos = rz;
        } else {
          Log.Write ($"Could not get SpawnProtectionRangeZ+ or SpawnProtectionRangeZ from json config, using default value {SpawnProtectionRangeZPos}");
        }
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ-", out rz)) {
          SpawnProtectionRangeZNeg = rz;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
          SpawnProtectionRangeZNeg = rz;
        } else {
          Log.Write ($"Could not get SpawnProtectionRangeZ- or SpawnProtectionRangeZ from json config, using default value {SpawnProtectionRangeZNeg}");
        }
        if (!jsonConfig.TryGetAs ("BannerProtectionRangeX", out BannerProtectionRangeX)) {
          Log.Write ($"Could not get banner protection x-range from json config, using default value {BannerProtectionRangeX}");
        }
        if (!jsonConfig.TryGetAs ("BannerProtectionRangeZ", out BannerProtectionRangeZ)) {
          Log.Write ($"Could not get banner protection z-range from json config, using default value {BannerProtectionRangeZ}");
        }
        JSONNode jsonCustomAreas;
        if (jsonConfig.TryGetAs ("CustomAreas", out jsonCustomAreas) && jsonCustomAreas.NodeType == NodeType.Array) {
          foreach (var jsonCustomArea in jsonCustomAreas.LoopArray ()) {
            try {
              CustomAreas.Add (new CustomProtectionArea (jsonCustomArea));
            } catch (Exception exception) {
              Log.WriteError ($"Exception loading custom area; {exception.Message}");
            }
          }
          Log.Write ($"Loaded {CustomAreas.Count} from file");
        }
        jsonConfig.TryGetAsOrDefault ("NpcKillsJailThreshold", out NpcKillsJailThreshold, 2);
        jsonConfig.TryGetAsOrDefault ("NpcKillsKickThreshold", out NpcKillsKickThreshold, 5);
        jsonConfig.TryGetAsOrDefault ("NpcKillsBanThreshold", out NpcKillsBanThreshold, 6);

        JSONNode jsonNameList;
        if (jsonConfig.TryGetAs("UnscoredPlayers", out jsonNameList) && jsonNameList.NodeType == NodeType.Array) {
          foreach (JSONNode jsonName in jsonNameList.LoopArray()) {
            Players.Player player;
            string error;
            string playerName = jsonName.GetAs<string>();
            if (PlayerHelper.TryGetPlayer(playerName, out player, out error, true)) {
              UnscoredPlayers.Add(player);
            } else {
              Log.Write($"Error loading unscored players {playerName}: {error}");
            }
          }
        }

        int speed = 0;
        jsonConfig.TryGetAsOrDefault("DeleteJobSpeed", out speed, 4);
        DeleteJobsManager.SetDeleteJobSpeed(speed, false);

      } else {
        Save ();
        Log.Write ($"Could not find {ConfigFilepath} file, created default one");
      }
      Log.Write ($"Using spawn protection with x+ range {SpawnProtectionRangeXPos}");
      Log.Write ($"Using spawn protection with x- range {SpawnProtectionRangeXNeg}");
      Log.Write ($"Using spawn protection with z+ range {SpawnProtectionRangeZPos}");
      Log.Write ($"Using spawn protection with z- range {SpawnProtectionRangeZNeg}");
      Log.Write ($"Using banner protection with x-range {BannerProtectionRangeX}");
      Log.Write ($"Using banner protection with z-range {BannerProtectionRangeZ}");
    }

    public static void AddCustomArea (CustomProtectionArea area)
    {
      CustomAreas.Add (area);
      Save ();
    }

    public static void RemoveCustomArea(CustomProtectionArea area)
    {
      CustomAreas.Remove(area);
      Save();
    }

    public static void Save ()
    {
      JSONNode jsonConfig;
      if (!JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        jsonConfig = new JSONNode ();
      }
      jsonConfig.SetAs ("SpawnProtectionRangeX+", SpawnProtectionRangeXPos);
      jsonConfig.SetAs ("SpawnProtectionRangeX-", SpawnProtectionRangeXNeg);
      jsonConfig.SetAs ("SpawnProtectionRangeZ+", SpawnProtectionRangeZPos);
      jsonConfig.SetAs ("SpawnProtectionRangeZ-", SpawnProtectionRangeZNeg);
      jsonConfig.SetAs ("BannerProtectionRangeX", BannerProtectionRangeX);
      jsonConfig.SetAs ("BannerProtectionRangeZ", BannerProtectionRangeZ);
      jsonConfig.SetAs ("NpcKillsKickThreshold", NpcKillsKickThreshold);
      jsonConfig.SetAs ("NpcKillsBanThreshold", NpcKillsBanThreshold);
      jsonConfig.SetAs ("NpcKillsJailThreshold", NpcKillsJailThreshold);
      var jsonCustomAreas = new JSONNode (NodeType.Array);
      foreach (var customArea in CustomAreas) {
        jsonCustomAreas.AddToArray (customArea.ToJSON ());
      }
      jsonConfig.SetAs ("CustomAreas", jsonCustomAreas);

      JSONNode jsonUnscoredPlayers = new JSONNode(NodeType.Array);
      foreach (Players.Player player in UnscoredPlayers) {
        JSONNode jsonName = new JSONNode();
        jsonName.SetAs(player.Name);
        jsonUnscoredPlayers.AddToArray(jsonName);
      }
      jsonConfig.SetAs("UnscoredPlayers", jsonUnscoredPlayers);
      jsonConfig.SetAs("DeleteJobSpeed", DeleteJobsManager.GetDeleteJobSpeed());

      JSON.Serialize (ConfigFilepath, jsonConfig, 2);
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnNPCHit, NAMESPACE + ".OnNPCHit")]
    public static void OnNPCHit (NPC.NPCBase npc, ModLoader.OnHitData data)
    {
      if (IsKilled (npc, data) && IsHitByPlayer (data.HitSourceType) && data.HitSourceObject is Players.Player) {
        var killer = (Players.Player)data.HitSourceObject;
        if (npc.Colony.Owner != killer) {
          int kills;
          if (!KillCounter.TryGetValue (killer, out kills)) {
            kills = 0;
          }
          KillCounter[killer] = ++kills;
          if (NpcKillsBanThreshold > 0 && kills >= NpcKillsBanThreshold) {
            Chat.SendToAll ($"{killer.Name} banned for killing too many colonists");
            BlackAndWhitelisting.AddBlackList (killer.ID.steamID.m_SteamID);
            Players.Disconnect (killer);
          } else if (NpcKillsKickThreshold > 0 && kills >= NpcKillsKickThreshold) {
            Chat.SendToAll ($"{killer.Name} kicked for killing too many colonists");
            Players.Disconnect (killer);
          } else if (NpcKillsJailThreshold > 0 && kills >= NpcKillsJailThreshold) {
            Chat.SendToAll ($"{killer.Name} put in Jail for killing too many colonists");
            JailManager.jailPlayer(killer, null, "Killing Colonists", JailManager.DEFAULT_JAIL_TIME);
          }
          Log.Write ($"{killer.Name} killed a colonist of {npc.Colony.Owner.Name} at {npc.Position}");
          int remainingJail = NpcKillsJailThreshold - kills;
          int remainingKick = NpcKillsKickThreshold - kills;
          Chat.Send (killer, $"You killed [{npc.Colony.Owner.Name}]'s colonist, remaining until jail: {remainingJail}, remaining until kick: {remainingKick}");
        }
      }
    }

    static bool IsKilled (NPC.NPCBase npc, ModLoader.OnHitData data)
    {
      return npc.health - data.ResultDamage <= 0;
    }

    static bool IsHitByPlayer (ModLoader.OnHitData.EHitSourceType hitSourceType)
    {
      return hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
             hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile ||
             hitSourceType == ModLoader.OnHitData.EHitSourceType.Misc;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAutoSaveWorld, NAMESPACE + ".OnAutoSaveWorld")]
    public static void OnAutoSaveWorld()
    {
      Save();
	  StatisticManager.Save();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnQuit, NAMESPACE + ".OnQuit")]
	public static void OnQuit()
	{
	  StatisticManager.Save();
    }

  }

}

