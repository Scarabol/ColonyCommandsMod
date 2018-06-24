using System;
using System.Collections.Generic;
using System.IO;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Chatting;
using ChatCommands.Implementations;
using Server.TerrainGeneration;
using UnityEngine;
using Permissions;

namespace ScarabolMods {

  [ModLoader.ModManager]
  public static class JailManager
  {
    static Vector3 jailPosition;
    static Vector3 jailVisitorPosition;
    static uint jailRange;
    static Dictionary<Players.Player, JailRecord> jailedPersons = new Dictionary<Players.Player, JailRecord>();
    static string CONFIG_FILE = "jail-config.json";
    public static bool validJail = false;
    public const uint DEFAULT_RANGE = 5;
    public const uint DEFAULT_JAIL_TIME = 3;

    // Jail record per player
    public class JailRecord {
      public long jailTimestamp { get; set; }
      public long jailDuration { get; set; }
      public Players.Player jailedBy { get; set; }
      public string jailReason { get; set; }
      public List<string> revokedPermissions { get; set; }

      public JailRecord(long time, long duration, Players.Player causedBy, string reason, List<string> permissions)
      {
        jailTimestamp = time;
        jailDuration = duration;
        jailedBy = causedBy;
        jailReason = reason;
        revokedPermissions = permissions;
      }
    }

    // Permission list to revoke during jail time
    static string[] permissionList = {
      AntiGrief.MOD_PREFIX + "warp.banner",
      AntiGrief.MOD_PREFIX + "warp.player",
      AntiGrief.MOD_PREFIX + "warp.self",
      AntiGrief.MOD_PREFIX + "warp.place",
      AntiGrief.MOD_PREFIX + "warp.spawn",
      AntiGrief.MOD_PREFIX + "warp.spawn.self",
      "pipliz.setflight",
      "cheats.enable",
    };

    static string ConfigfilePath {
      get {
        return Path.Combine(Path.Combine("gamedata", "savegames"), Path.Combine(ServerManager.WorldName, CONFIG_FILE));
      }
    }

    // send a player into the jail
    public static void jailPlayer(Players.Player target, Players.Player causedBy, string reason, long jailtime = DEFAULT_JAIL_TIME)
    {
      if (!validJail) {
        if (causedBy == null) {
          Log.Write($"Cannot Jail {target.Name}: no valid jail found");
        } else {
          Chat.Send(causedBy, "No valid jail found. Unable to complete jailing");
        }
        return;
      }

      List<string> permissions = new List<string>();
      foreach (string permission in permissionList) {
        if (PermissionsManager.HasPermission(target, permission)) {
          permissions.Add(permission);
          PermissionsManager.RemovePermissionOfUser(causedBy, target, permission);
        }
      }
      long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
      JailRecord record = new JailRecord(now, jailtime * 60, causedBy, reason, permissions);
      jailedPersons.Add(target, record);

      Teleport.TeleportTo(target, jailPosition);
      Chat.Send(target, $"<color=red>{causedBy.Name} threw you into jail!</color> Reason: {reason}");
      Chat.Send(target, $"Remaining Jail Time: {jailtime} minutes");
      Log.Write($"{causedBy.Name} threw {target.Name} into jail! Reason: {reason}");
      return;
    }

    // update/set the jail position in the world
    public static void setJailPosition(Vector3 newPosition, uint range = DEFAULT_RANGE)
    {
      jailPosition.x = newPosition.x;
      jailPosition.y = newPosition.y + 1;  // one block higher to prevent clipping
      jailPosition.z = newPosition.z;

      jailRange = range;
      return;
    }

    // update/set the jail visitor position in the world
    public static void setJailVisitorPosition(Vector3 newPosition)
    {
      jailVisitorPosition.x = newPosition.x;
      jailVisitorPosition.y = newPosition.y + 1;
      jailVisitorPosition.z = newPosition.z;
      return;
    }

    // load from config file
    public static void Load()
    {
      JSONNode jsonConfig;
      if (!JSON.Deserialize(ConfigfilePath, out jsonConfig, false)) {
        Log.Write("No {0} found inside world directory, creating default config", CONFIG_FILE);
        return;
      }

      Log.Write("Loading jail config from {0}", CONFIG_FILE);
      try {
        JSONNode position;
        jsonConfig.TryGetAs("position", out position);
        jailPosition.x = position.GetAs<float>("x");
        jailPosition.y = position.GetAs<float>("y");
        jailPosition.z = position.GetAs<float>("z");
        jailRange = position.GetAs<uint>("range");
        validJail = true;

        JSONNode visitorPos;
        jsonConfig.TryGetAs("visitorPosition", out visitorPos);
        jailVisitorPosition.x = visitorPos.GetAs<float>("x");
        jailVisitorPosition.y = visitorPos.GetAs<float>("y");
        jailVisitorPosition.z = visitorPos.GetAs<float>("z");

        JSONNode players;
        jsonConfig.TryGetAs("players", out players);
        foreach (JSONNode node in players.LoopArray()) {
          string PlayerName = node.GetAs<string>("name");
          long jailTimestamp = node.GetAs<long>("time");
          long jailDuration = node.GetAs<long>("duration");
          string causedByName = node.GetAs<string>("jailedBy");
          string reason = node.GetAs<string>("jailReason");

          List<string> permissions = node.GetAs<List<string>>("permissions");
          // List<string> permissions = new List<string>;
          // JSONNode jsonPerm;
          // node.TryGetAs("permissions", out jsonPerm);
          // foreach (string perm in jsonPerm.LoopArray()) {
          //   permissions.Add(perm);
          // }

          Players.Player target;
          Players.Player causedBy;
          string error;

          if (PlayerHelper.TryGetPlayer(PlayerName, out target, out error, true) &&
            PlayerHelper.TryGetPlayer(causedByName, out causedBy, out error, true)) {
            JailRecord record = new JailRecord(jailTimestamp, jailDuration, causedBy, reason, permissions);
            jailedPersons.Add(target, record);
          }
        }

      } catch (Exception e) {
        Log.Write("Error parsing {0}: {1}", CONFIG_FILE, e.Message);
      }
      return;
    }

    // save to config file
    public static void Save()
    {
      Log.Write("Saving jail config to {0}", CONFIG_FILE);

      JSONNode jsonConfig = new JSONNode();
      JSONNode jsonPosition = new JSONNode();
      jsonPosition.SetAs("x", jailPosition.x);
      jsonPosition.SetAs("y", jailPosition.y);
      jsonPosition.SetAs("z", jailPosition.z);
      jsonPosition.SetAs("range", jailRange);
      jsonConfig.SetAs("position", jsonPosition);

      JSONNode jsonVisitorPos = new JSONNode();
      jsonVisitorPos.SetAs("x", jailVisitorPosition.x);
      jsonVisitorPos.SetAs("y", jailVisitorPosition.y);
      jsonVisitorPos.SetAs("z", jailVisitorPosition.z);
      jsonConfig.SetAs("visitorPosition", jsonVisitorPos);

      JSONNode jsonPlayers = new JSONNode(NodeType.Array);
      foreach (KeyValuePair<Players.Player, JailRecord> kvp in jailedPersons) {
        Players.Player target = kvp.Key;
        JailRecord record = kvp.Value;
        JSONNode jsonRecord = new JSONNode();
        jsonRecord.SetAs("target", target.ID.steamID);
        jsonRecord.SetAs("time", record.jailTimestamp);
        jsonRecord.SetAs("duration", record.jailDuration);
        jsonRecord.SetAs("jailedBy", record.jailedBy.Name);
        jsonRecord.SetAs("jailReason", record.jailReason);

        // JSONNode permissions = new JSONNode(NodeType.Array);
        // foreach (string perm in record.revokedPermissions) {
        //   permissions.AddToArray(perm);
        // }
        jsonRecord.SetAs("permissions", record.revokedPermissions.ToString());
        jsonPlayers.AddToArray(jsonRecord);
      }
      jsonConfig.SetAs("players", jsonPlayers);

      try {
        JSON.Serialize(ConfigfilePath, jsonConfig);
      } catch (Exception e) {
        Log.Write("Could not save {0}: {1}", CONFIG_FILE, e.Message);
      }
      return;
    }

    // check if jailed
    public static bool IsPlayerJailed(Players.Player player)
    {
      return jailedPersons.ContainsKey(player);
    }

    // release a player from jail
    public static void releasePlayer(Players.Player target, Players.Player causedBy)
    {
      if (causedBy == null) {
        causedBy = target;
      }
      JailRecord record;
      jailedPersons.TryGetValue(target, out record);
      foreach (string permission in record.revokedPermissions) {
        PermissionsManager.AddPermissionToUser(causedBy, target, permission);
      }
      jailedPersons.Remove(target);
      Teleport.TeleportTo(target, TerrainGenerator.UsedGenerator.GetSpawnLocation(target));
      Chat.Send(target, "<color=yellow>You did your time and are released from Jail</color>");
      Log.Write($"{causedBy.Name} released {target.Name} from jail");
      return;
    }

    // track jailed players movement
    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerMoved, "scarabol.antigrief.onplayermoved")]
    public static void OnPlayerMoved(Players.Player causedBy)
    {
      checkJailTimeLeft();
      if (!jailedPersons.ContainsKey(causedBy)) {
        return;
      }

      uint distance = (uint) Vector3.Distance(causedBy.Position, jailPosition);
      if (distance > jailRange) {
        Teleport.TeleportTo(causedBy, jailPosition);
        Chat.Send(causedBy, "<color=red>A guard spotted your escape attempt</color>");
      }
    }

    // check time and release Players again
    public static void checkJailTimeLeft()
    {
      long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
      foreach (KeyValuePair<Players.Player, JailRecord> kvp in jailedPersons) {
        Players.Player target = kvp.Key;
        JailRecord record = kvp.Value;
        if (record.jailTimestamp + record.jailDuration <= now) {
          releasePlayer(target, null);
        }
      }
      return;
    }

  }
}

