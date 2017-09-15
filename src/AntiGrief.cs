using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Permissions;
using NPC;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class AntiGriefModEntries
  {
    public static string PERMISSION_SUPER = "mods.scarabol.antigrief";
    public static string PERMISSION_SPAWN_CHANGE = PERMISSION_SUPER + ".spawnchange";
    public static string PERMISSION_BANNER_PREFIX = PERMISSION_SUPER + ".banner.";
    private static string ConfigFilepath;
    private static int SpawnProtectionRangeXPos = 50;
    private static int SpawnProtectionRangeXNeg = 50;
    private static int SpawnProtectionRangeZPos = 50;
    private static int SpawnProtectionRangeZNeg = 50;
    private static int BannerProtectionRangeX = 50;
    private static int BannerProtectionRangeZ = 50;
    private static List<CustomProtectionArea> customAreas = new List<CustomProtectionArea> ();

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAssemblyLoaded, "scarabol.antigrief.assemblyload")]
    public static void OnAssemblyLoaded (string path)
    {
      Pipliz.Log.Write ("Loaded AntiGrief by Scarabol");
      ConfigFilepath = Path.Combine (Path.GetDirectoryName (path), "protection-ranges.json");
      LoadRangesFromJSON ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnTryChangeBlockUser, "scarabol.antigrief.trychangeblock")]
    public static bool OnTryChangeBlockUser (ModLoader.OnTryChangeBlockUserData userData)
    {
      Players.Player requestedBy = userData.requestedBy;
      Vector3Int position = userData.VoxelToChange;
      Vector3Int spawn = TerrainGenerator.GetSpawnLocation ();
      int ox = position.x - spawn.x;
      int oz = position.z - spawn.z;
      if (((ox >= 0 && ox <= SpawnProtectionRangeXPos) || (ox < 0 && ox >= -SpawnProtectionRangeXNeg)) && ((oz >= 0 && oz <= SpawnProtectionRangeZPos) || (oz < 0 && oz >= -SpawnProtectionRangeZNeg))) {
        if (!PermissionsManager.HasPermission (requestedBy, PERMISSION_SPAWN_CHANGE)) {
          Chat.Send (requestedBy, "<color=red>You don't have permission to change the spawn area!</color>");
          return false;
        }
      } else {
        Banner homeBanner = BannerTracker.Get (requestedBy);
        if (homeBanner != null) {
          Vector3Int homeBannerLocation = homeBanner.KeyLocation;
          if (System.Math.Abs (homeBannerLocation.x - position.x) <= BannerProtectionRangeX && System.Math.Abs (homeBannerLocation.z - position.z) <= BannerProtectionRangeZ) {
            return true;
          }
        }
        int checkRangeX = BannerProtectionRangeX;
        int checkRangeZ = BannerProtectionRangeZ;
        if (userData.typeToBuild == BlockTypes.Builtin.BuiltinBlocks.Banner) {
          checkRangeX *= 2;
          checkRangeZ *= 2;
        }
        foreach (Banner b in BannerTracker.GetBanners()) {
          Vector3Int bannerLocation = b.KeyLocation;
          if (System.Math.Abs (bannerLocation.x - position.x) <= checkRangeX && System.Math.Abs (bannerLocation.z - position.z) <= checkRangeZ) {
            if (b.Owner != requestedBy && !PermissionsManager.HasPermission (requestedBy, PERMISSION_BANNER_PREFIX + b.Owner.ID.steamID)) {
              Chat.Send (requestedBy, "<color=red>You don't have permission to change blocks near this banner!</color>");
              return false;
            }
            break;
          }
        }
        foreach (CustomProtectionArea area in customAreas) {
          if (area.Contains (position) && !PermissionsManager.HasPermission (requestedBy, PERMISSION_SPAWN_CHANGE)) {
            Chat.Send (requestedBy, "<color=red>You don't have permission to change this protected area!</color>");
            return false;
          }
        }
      }
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.antigrief.registertypes")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new AntiGriefChatCommand ());
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, "scarabol.antigrief.onplayerconnected")]
    public static void OnPlayerConnectedLate (Players.Player player)
    {
      Chat.Send (player, "<color=yellow>Anti-Grief protection enabled</color>");
    }

    public static void LoadRangesFromJSON ()
    {
      JSONNode jsonConfig;
      if (JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        int rx;
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeX+", out rx)) {
          SpawnProtectionRangeXPos = rx;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
          SpawnProtectionRangeXPos = rx;
        } else {
          Pipliz.Log.Write (string.Format ("Could not get SpawnProtectionRangeX+ or SpawnProtectionRangeX from json config, using default value {0}", SpawnProtectionRangeXPos));
        }
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeX-", out rx)) {
          SpawnProtectionRangeXNeg = rx;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
          SpawnProtectionRangeXNeg = rx;
        } else {
          Pipliz.Log.Write (string.Format ("Could not get SpawnProtectionRangeX- or SpawnProtectionRangeX from json config, using default value {0}", SpawnProtectionRangeXNeg));
        }
        int rz;
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ+", out rz)) {
          SpawnProtectionRangeZPos = rz;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
          SpawnProtectionRangeZPos = rz;
        } else {
          Pipliz.Log.Write (string.Format ("Could not get SpawnProtectionRangeZ+ or SpawnProtectionRangeZ from json config, using default value {0}", SpawnProtectionRangeZPos));
        }
        if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ-", out rz)) {
          SpawnProtectionRangeZNeg = rz;
        } else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
          SpawnProtectionRangeZNeg = rz;
        } else {
          Pipliz.Log.Write (string.Format ("Could not get SpawnProtectionRangeZ- or SpawnProtectionRangeZ from json config, using default value {0}", SpawnProtectionRangeZNeg));
        }
        if (!jsonConfig.TryGetAs ("BannerProtectionRangeX", out BannerProtectionRangeX)) {
          Pipliz.Log.Write (string.Format ("Could not get banner protection x-range from json config, using default value {0}", BannerProtectionRangeX));
        }
        if (!jsonConfig.TryGetAs ("BannerProtectionRangeZ", out BannerProtectionRangeZ)) {
          Pipliz.Log.Write (string.Format ("Could not get banner protection z-range from json config, using default value {0}", BannerProtectionRangeZ));
        }
        JSONNode jsonCustomAreas;
        if (jsonConfig.TryGetAs ("CustomAreas", out jsonCustomAreas) && jsonCustomAreas.NodeType == NodeType.Array) {
          foreach (JSONNode jsonCustomArea in jsonCustomAreas.LoopArray()) {
            try {
              customAreas.Add (new CustomProtectionArea (jsonCustomArea));
            } catch (Exception exception) {
              Pipliz.Log.WriteError ($"Exception loading custom area; {exception.Message}");
            }
          }
          Pipliz.Log.Write ($"Loaded {customAreas.Count} from file");
        }
      } else {
        Save ();
        Pipliz.Log.Write ("Could not find protection-ranges.json file, created default one");
      }
      Pipliz.Log.Write (string.Format ("Using spawn protection with x+ range {0}", SpawnProtectionRangeXPos));
      Pipliz.Log.Write (string.Format ("Using spawn protection with x- range {0}", SpawnProtectionRangeXNeg));
      Pipliz.Log.Write (string.Format ("Using spawn protection with z+ range {0}", SpawnProtectionRangeZPos));
      Pipliz.Log.Write (string.Format ("Using spawn protection with z- range {0}", SpawnProtectionRangeZNeg));
      Pipliz.Log.Write (string.Format ("Using banner protection with x-range {0}", BannerProtectionRangeX));
      Pipliz.Log.Write (string.Format ("Using banner protection with z-range {0}", BannerProtectionRangeZ));
    }

    public static void AddCustomArea (CustomProtectionArea area)
    {
      customAreas.Add (area);
      Save ();
    }

    public static void Save ()
    {
      JSONNode jsonConfig;
      if (!JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        jsonConfig = new JSONNode ();
      }
      JSONNode jsonCustomAreas = new JSONNode (NodeType.Array);
      foreach (CustomProtectionArea customArea in customAreas) {
        jsonCustomAreas.AddToArray (customArea.ToJSON ());
      }
      jsonConfig.SetAs ("CustomAreas", jsonCustomAreas);
      JSON.Serialize (ConfigFilepath, jsonConfig, 2);
    }
  }

  public class AntiGriefChatCommand : ChatCommands.IChatCommand
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
      string accesslevel = matched.Groups ["accesslevel"].Value;
      string targetPlayerName = matched.Groups ["playername"].Value;
      if (accesslevel.Equals ("area")) {
        if (causedBy == null || causedBy.ID == NetworkID.Server) {
          Pipliz.Log.WriteError ("You can't define custom protection areas as server (missing center)");
          return true;
        } else if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
          return true;
        }
        string rangex = matched.Groups ["rangex"].Value;
        string rangez = matched.Groups ["rangez"].Value;
        int rx;
        int rz;
        string rangexn = matched.Groups ["rangexn"].Value;
        string rangexp = matched.Groups ["rangexp"].Value;
        string rangezn = matched.Groups ["rangezn"].Value;
        string rangezp = matched.Groups ["rangezp"].Value;
        int rxn;
        int rxp;
        int rzn;
        int rzp;
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
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        if (accesslevel.Equals ("spawn")) {
          if (PermissionsManager.CheckAndWarnPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
            PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, string.Format ("You granted [{0}] permission to change the spawn area", targetPlayer.Name));
            Chat.Send (targetPlayer, "You got permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("nospawn")) {
          if (PermissionsManager.HasPermission (causedBy, AntiGriefModEntries.PERMISSION_SUPER)) {
            PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, string.Format ("You revoked permission for [{0}] to change the spawn area", targetPlayer.Name));
            Chat.Send (targetPlayer, "You lost permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("banner")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You already have this permission");
            return true;
          }
          PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, string.Format ("You granted [{0}] permission to change your banner area", targetPlayer.Name));
          Chat.Send (targetPlayer, string.Format ("You got permission to change banner area of [{0}]", causedBy.Name));
        } else if (accesslevel.Equals ("deny")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You can't revoke the permission for yourself");
            return true;
          }
          PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGriefModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, string.Format ("You revoked permission for [{0}] to change your banner area", targetPlayer.Name));
          Chat.Send (targetPlayer, string.Format ("You lost permission to change banner area of [{0}]", causedBy.Name));
        } else {
          Chat.Send (causedBy, "Unknown access level, use /antigrief [spawn|nospawn|banner|deny] steamid");
        }
      }
      return true;
    }
  }

  public class CustomProtectionArea
  {
    private int startX;
    private int endX;
    private int startZ;
    private int endZ;

    public CustomProtectionArea (int startX, int endX, int startZ, int endZ)
    {
      this.startX = startX;
      this.endX = endX;
      this.startZ = startZ;
      this.endZ = endZ;
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
        .SetAs ("startX", startX)
        .SetAs ("endX", endX)
        .SetAs ("startZ", startZ)
        .SetAs ("endZ", endZ);
    }

    public bool Contains (Vector3Int point)
    {
      return this.startX <= point.x && this.endX >= point.x && this.startZ <= point.z && this.endZ >= point.z;
    }
  }
}