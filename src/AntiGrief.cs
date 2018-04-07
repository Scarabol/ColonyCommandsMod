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

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class AntiGriefModEntries
  {
    public static string PERMISSION_SUPER = "mods.scarabol.antigrief";
    public static string PERMISSION_SPAWN_CHANGE = PERMISSION_SUPER + ".spawnchange";
    public static string PERMISSION_BANNER_PREFIX = PERMISSION_SUPER + ".banner.";
    static int SpawnProtectionRangeXPos;
    static int SpawnProtectionRangeXNeg;
    static int SpawnProtectionRangeZPos;
    static int SpawnProtectionRangeZNeg;
    static int BannerProtectionRangeX;
    static int BannerProtectionRangeZ;
    static List<CustomProtectionArea> CustomAreas = new List<CustomProtectionArea> ();
    static int NpcKillsKickThreshold;
    static int NpcKillsBanThreshold;
    static Dictionary<Players.Player, int> KillCounter = new Dictionary<Players.Player, int> ();

    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "protection-ranges.json"));
      }
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAssemblyLoaded, "scarabol.antigrief.assemblyload")]
    public static void OnAssemblyLoaded (string path)
    {
      Log.Write ("Loaded AntiGrief by Scarabol");
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnTryChangeBlock, "scarabol.antigrief.trychangeblock")]
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

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.antigrief.registertypes")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new AntiGriefChatCommand ());
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.antigrief.loadranges")]
    public static void AfterWorldLoad ()
    {
      Load ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, "scarabol.antigrief.onplayerconnected")]
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
        jsonConfig.TryGetAsOrDefault ("NpcKillsKickThreshold", out NpcKillsKickThreshold, 10);
        jsonConfig.TryGetAsOrDefault ("NpcKillsBanThreshold", out NpcKillsBanThreshold, 50);
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
      var jsonCustomAreas = new JSONNode (NodeType.Array);
      foreach (var customArea in CustomAreas) {
        jsonCustomAreas.AddToArray (customArea.ToJSON ());
      }
      jsonConfig.SetAs ("CustomAreas", jsonCustomAreas);
      JSON.Serialize (ConfigFilepath, jsonConfig, 2);
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnNPCHit, "scarabol.antigrief.onnpchit")]
    public static void OnNPCHit (NPC.NPCBase npc, ModLoader.OnHitData data)
    {
      if (IsKilled (npc, data) && IsHitByPlayer (data.HitSourceType) && data.HitSourceObject is Players.Player) {
        var killer = (Players.Player)data.HitSourceObject;
        if (npc.Colony.Owner != killer) {
          int kills;
          if (!KillCounter.TryGetValue (killer, out kills)) {
            kills = 0;
          }
          kills++;
          KillCounter [killer] = kills;
          if (kills >= NpcKillsKickThreshold) {
            Chat.SendToAll ($"{killer.Name} kicked for killing too many colonists");
            Players.Disconnect (killer);
          } else if (kills >= NpcKillsBanThreshold) {
            Chat.SendToAll ($"{killer.Name} banned for killing too many colonists");
            BlackAndWhitelisting.AddBlackList (killer.ID.steamID.m_SteamID);
            Players.Disconnect (killer);
          }
          Log.Write ($"{killer.Name} killed a colonist of {npc.Colony.Owner.Name} at {npc.Position}");
        }
      }
    }

    static bool IsKilled (NPC.NPCBase npc, ModLoader.OnHitData data)
    {
      return npc.health - data.ResultDamage <= 0;
    }

    static bool IsHitByPlayer (ModLoader.OnHitData.EHitSourceType hitSourceType)
    {
      return hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick || hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile;
    }
  }

  public class AntiGriefChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/antigrief") || chat.StartsWith ("/antigrief ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var matched = Regex.Match (chattext, @"/antigrief (?<accesslevel>[^ ]+) ((?<playername>['].+?[']|[^ ]+)|((?<rangex>\d+) (?<rangez>\d+))|((?<rangexn>\d+) (?<rangexp>\d+) (?<rangezn>\d+) (?<rangezp>\d+)))$");
      if (!matched.Success) {
        Chat.Send (causedBy, "Command didn't match, use /antigrief [spawn|nospawn|banner|deny] [playername] or /antigrief area [rangex rangez|rangexn rangexp rangezn rangezp]");
        return true;
      }
      var accesslevel = matched.Groups ["accesslevel"].Value;
      var targetPlayerName = matched.Groups ["playername"].Value;
      if (accesslevel.Equals ("area")) {
        if (causedBy == null || causedBy.ID == NetworkID.Server) {
          Log.WriteError ("You can't define custom protection areas as server (missing center)");
          return true;
        } else if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
          return true;
        }
        var rangex = matched.Groups ["rangex"].Value;
        var rangez = matched.Groups ["rangez"].Value;
        int rx, rz;
        var rangexn = matched.Groups ["rangexn"].Value;
        var rangexp = matched.Groups ["rangexp"].Value;
        var rangezn = matched.Groups ["rangezn"].Value;
        var rangezp = matched.Groups ["rangezp"].Value;
        int rxn, rxp, rzn, rzp;
        if (rangex.Length > 0 && int.TryParse (rangex, out rx) && rangez.Length > 0 && int.TryParse (rangez, out rz)) {
          AntiGriefModEntries.AddCustomArea (new CustomProtectionArea (causedBy.VoxelPosition, rx, rz));
          Chat.Send (causedBy, $"Added anti grief area at {causedBy.VoxelPosition} with x-range {rx} and z-range {rz}");
        } else if (rangexn.Length > 0 && int.TryParse (rangexn, out rxn) && rangexp.Length > 0 && int.TryParse (rangexp, out rxp) && rangezn.Length > 0 && int.TryParse (rangezn, out rzn) && rangezp.Length > 0 && int.TryParse (rangezp, out rzp)) {
          AntiGriefModEntries.AddCustomArea (new CustomProtectionArea (causedBy.VoxelPosition, rxn, rxp, rzn, rzp));
          Chat.Send (causedBy, $"Added anti grief area at {causedBy.VoxelPosition} from x- {rxn} to x+ {rxp} and from z- {rzn} to z+ {rzp}");
        } else {
          Chat.Send (causedBy, $"Could not parse protection area ranges {rangex} {rangez} {rangexn} {rangexp} {rangezn} {rangezp}");
        }
      } else {
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
          Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
          return true;
        }
        if (accesslevel.Equals ("spawn")) {
          if (PermissionsManager.CheckAndWarnPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
            PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, $"You granted [{targetPlayer.Name}] permission to change the spawn area");
            Chat.Send (targetPlayer, "You got permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("nospawn")) {
          if (PermissionsManager.HasPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
            PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, $"You revoked permission for [{targetPlayer.Name}] to change the spawn area");
            Chat.Send (targetPlayer, "You lost permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("banner")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You already have this permission");
            return true;
          }
          PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, $"You granted [{targetPlayer.Name}] permission to change your banner area");
          Chat.Send (targetPlayer, $"You got permission to change banner area of [{causedBy.Name}]");
        } else if (accesslevel.Equals ("deny")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You can't revoke the permission for yourself");
            return true;
          }
          PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, $"You revoked permission for [{targetPlayer.Name}] to change your banner area");
          Chat.Send (targetPlayer, $"You lost permission to change banner area of [{causedBy.Name}]");
        } else {
          Chat.Send (causedBy, "Unknown access level, use /antigrief [spawn|nospawn|banner|deny] steamid");
        }
      }
      return true;
    }
  }

  public class CustomProtectionArea
  {
    readonly int StartX;
    readonly int EndX;
    readonly int StartZ;
    readonly int EndZ;

    public CustomProtectionArea (int startX, int endX, int startZ, int endZ)
    {
      StartX = startX;
      EndX = endX;
      StartZ = startZ;
      EndZ = endZ;
    }

    public CustomProtectionArea (Vector3Int center, int rangeX, int rangeZ)
      : this (center, rangeX, rangeX, rangeZ, rangeZ)
    {
    }

    public CustomProtectionArea (Vector3Int center, int rangeXN, int rangeXP, int rangeZN, int rangeZP)
      : this (center.x - rangeXN, center.x + rangeXP, center.z - rangeZN, center.z + rangeZP)
    {
    }

    public CustomProtectionArea (JSONNode jsonNode)
      : this (jsonNode.GetAs<int> ("startX"), jsonNode.GetAs<int> ("endX"), jsonNode.GetAs<int> ("startZ"), jsonNode.GetAs<int> ("endZ"))
    {
    }

    public JSONNode ToJSON ()
    {
      return new JSONNode ()
        .SetAs ("startX", StartX)
        .SetAs ("endX", EndX)
        .SetAs ("startZ", StartZ)
        .SetAs ("endZ", EndZ);
    }

    public bool Contains (Vector3Int point)
    {
      return StartX <= point.x && EndX >= point.x && StartZ <= point.z && EndZ >= point.z;
    }
  }
}