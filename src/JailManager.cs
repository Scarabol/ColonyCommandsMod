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
    public static Dictionary<Players.Player, List<JailLogRecord>> jailLog = new Dictionary<Players.Player, List<JailLogRecord>>();
    const string CONFIG_FILE = "jail-config.json";
    const string LOG_FILE = "jail-log.json";
    public static bool validJail = false;
    public static bool validVisitorPos = false;
    public const uint DEFAULT_RANGE = 5;
    public const uint DEFAULT_JAIL_TIME = 3;

    // Jail record per player
    private class JailRecord {
      public int gracePeriod { get; set; }
      public long jailTimestamp { get; set; }
      public long jailDuration { get; set; }
      public Players.Player jailedBy { get; set; }
      public string jailReason { get; set; }
      public List<string> revokedPermissions { get; set; }

      public JailRecord(long time, long duration, Players.Player causedBy, string reason, List<string> permissions)
      {
        this.gracePeriod = 2;
        this.jailTimestamp = time;
        this.jailDuration = duration;
        this.jailedBy = causedBy;
        this.jailReason = reason;
        this.revokedPermissions = permissions;
      }
    }

    // log file record
    public class JailLogRecord {
      public long timestamp { get; set; }
      public long duration { get; set; }
      public Players.Player jailedBy { get; set; }
      public string reason { get; set; }

      public JailLogRecord(long time, long duration, Players.Player causedBy, string reason)
      {
        this.timestamp = time;
        this.duration = duration;
        this.jailedBy = causedBy;
        this.reason = reason;
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

    static string LogFilePath {
      get {
        return Path.Combine(Path.Combine("gamedata", "savegames"), Path.Combine(ServerManager.WorldName, LOG_FILE));
      }
    }

    // send a player to jail
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
      Teleport.TeleportTo(target, jailPosition);

      List<string> permissions = new List<string>();
      foreach (string permission in permissionList) {
        if (PermissionsManager.HasPermission(target, permission)) {
          permissions.Add(permission);
          // PermissionsManager.RemovePermissionOfUser(causedBy, target, permission);
        }
      }

      long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
      // create/add history record
      JailLogRecord logRecord = new JailLogRecord(now, jailtime * 60, causedBy, reason);
      List<JailLogRecord> playerRecords;
      if (jailLog.TryGetValue(target, out playerRecords)) {
        playerRecords.Add(logRecord);
      } else {
        playerRecords = new List<JailLogRecord>();
        playerRecords.Add(logRecord);
        jailLog.Add(target, playerRecords);
      }
      SaveLogFile();

      // create jail record
      JailRecord record = new JailRecord(now, jailtime * 60, causedBy, reason, permissions);
      jailedPersons.Add(target, record);
      Save();

      Chat.Send(target, $"<color=red>{causedBy.Name} threw you into jail! Reason: {reason}</color>");
      Chat.Send(target, $"Remaining Jail Time: {jailtime} minutes");
      Log.Write($"{causedBy.Name} threw {target.Name} into jail! Reason: {reason}");
      return;
    }

    // visit the jail - no harm is done
    public static void VisitJail(Players.Player causedBy)
    {
      if (validJail && validVisitorPos) {
        Teleport.TeleportTo(causedBy, jailVisitorPosition);
      }
      return;
    }

    // update/set the jail position in the world
    public static void setJailPosition(Vector3 newPosition, uint range = DEFAULT_RANGE)
    {
      jailPosition.x = newPosition.x;
      jailPosition.y = newPosition.y + 1;  // one block higher to prevent clipping
      jailPosition.z = newPosition.z;
      jailRange = range;
      validJail = true;
      Save();
      return;
    }

    // update/set the jail visitor position in the world
    public static void setJailVisitorPosition(Vector3 newPosition)
    {
      jailVisitorPosition.x = newPosition.x;
      jailVisitorPosition.y = newPosition.y + 1;
      jailVisitorPosition.z = newPosition.z;
      validVisitorPos = true;
      Save();
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
        if (jsonConfig.TryGetAs("position", out position)) {
          jailPosition.x = position.GetAs<float>("x");
          jailPosition.y = position.GetAs<float>("y");
          jailPosition.z = position.GetAs<float>("z");
          jailRange = position.GetAs<uint>("range");
          validJail = true;
        } else {
          Log.Write("Did not find a jail position, invalid config");
        }

        JSONNode visitorPos;
        if (jsonConfig.TryGetAs("visitorPosition", out visitorPos)) {
          jailVisitorPosition.x = visitorPos.GetAs<float>("x");
          jailVisitorPosition.y = visitorPos.GetAs<float>("y");
          jailVisitorPosition.z = visitorPos.GetAs<float>("z");
          validVisitorPos = true;
        }

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

      LoadLogFile();
      return;
    }

    // save to config file
    public static void Save()
    {
      Log.Write("Saving jail config to {0}", CONFIG_FILE);

      JSONNode jsonConfig = new JSONNode();

      if (validJail) {
        JSONNode jsonPosition = new JSONNode();
        jsonPosition.SetAs("x", jailPosition.x);
        jsonPosition.SetAs("y", jailPosition.y);
        jsonPosition.SetAs("z", jailPosition.z);
        jsonPosition.SetAs("range", jailRange);
        jsonConfig.SetAs("position", jsonPosition);
      }

      if (validVisitorPos) {
        JSONNode jsonVisitorPos = new JSONNode();
        jsonVisitorPos.SetAs("x", jailVisitorPosition.x);
        jsonVisitorPos.SetAs("y", jailVisitorPosition.y);
        jsonVisitorPos.SetAs("z", jailVisitorPosition.z);
        jsonConfig.SetAs("visitorPosition", jsonVisitorPos);
      }

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
        jsonRecord.SetAs("permissions", record.revokedPermissions.ToString());  // TODO
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
      jailedPersons.Remove(target);
      Save();
      if (causedBy == null) {
        causedBy = target;
      }
      JailRecord record;
      jailedPersons.TryGetValue(target, out record);
      foreach (string permission in record.revokedPermissions) {
        // PermissionsManager.AddPermissionToUser(causedBy, target, permission);
      }
      Teleport.TeleportTo(target, TerrainGenerator.UsedGenerator.GetSpawnLocation(target));
      Chat.Send(target, "<color=yellow>You did your time and are released from Jail</color>");
      Log.Write($"{causedBy.Name} released {target.Name} from jail");
      return;
    }

    // track jailed players movement
    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerMoved, AntiGrief.NAMESPACE + ".OnPlayerMoved")]
    public static void OnPlayerMoved(Players.Player causedBy)
    {
      if (!jailedPersons.ContainsKey(causedBy)) {
        return;
      }

      checkJailTimeLeft();

      // each newly jailed player gets a grace period. This is mostly to avoid guard warnings
      // because OnPlayerMoved triggers too fast and can get the old position before the teleport to jail
      JailRecord record;
      jailedPersons.TryGetValue(causedBy, out record);
      if (record.gracePeriod > 0) {
        --record.gracePeriod;
        return;
      }
      uint distance = (uint) Vector3.Distance(causedBy.Position, jailPosition);
      if (distance > jailRange) {
        Teleport.TeleportTo(causedBy, jailPosition);
        Chat.Send(causedBy, "<color=red>Guards spot your escape attempt and push you back</color>");
      }
    }

    // check time and release Players from jail
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

    // load log file (=past jail records)
    public static void LoadLogFile()
    {
      JSONNode jsonLog;
      if (!JSON.Deserialize(LogFilePath, out jsonLog, false)) {
        Log.Write("No {0} found inside world directory, nothing to load", LOG_FILE);
        return;
      }

      Log.Write("Loading jail history records from {0}", LOG_FILE);
      try {

        JSONNode players;
        jsonLog.TryGetAs("players", out players);
        foreach (JSONNode node in players.LoopArray()) {
          string PlayerId = node.GetAs<string>("steamId");
          Players.Player target;
          string error;
          if (!PlayerHelper.TryGetPlayer(PlayerId, out target, out error, true)) {
            Log.Write($"Could not find player with id {PlayerId} from {LOG_FILE}");
            continue;
          }

          List<JailLogRecord> playerHistory = new List<JailLogRecord>();
          JSONNode jsonPlayerRecords;
          node.TryGetAs("records", out jsonPlayerRecords);
          foreach (JSONNode record in jsonPlayerRecords.LoopArray()) {
            long timestamp = record.GetAs<long>("timestamp");
            long duration = record.GetAs<long>("duration");
            string jailedById = record.GetAs<string>("jailedBy");
            string reason = record.GetAs<string>("reason");

            Players.Player causedBy;
            PlayerHelper.TryGetPlayer(jailedById, out causedBy, out error, true);
            JailLogRecord playerRecord = new JailLogRecord(timestamp, duration, causedBy, reason);
            playerHistory.Add(playerRecord);
          }
          
          jailLog.Add(target, playerHistory);
        }

      } catch (Exception e) {
        Log.Write("Error parsing {0}: {1}", LOG_FILE, e.Message);
      }
      return;
    }

    // save log file (=past jail records)
    public static void SaveLogFile()
    {
      Log.Write("Saving jail history log to {0}", LOG_FILE);

      JSONNode jsonLogfile = new JSONNode();
      JSONNode jsonPlayers = new JSONNode(NodeType.Array);
      foreach (KeyValuePair<Players.Player, List<JailLogRecord>> kvp in jailLog) {
        JSONNode jsonPlayerRecord = new JSONNode();
        Players.Player target = kvp.Key;
        List<JailLogRecord> records = kvp.Value;
        jsonPlayerRecord.SetAs("steamId", target.ID.steamID);

        JSONNode jsonRecords = new JSONNode(NodeType.Array);
        foreach (JailLogRecord record in records) {
          JSONNode jsonRecord = new JSONNode();
          jsonRecord.SetAs("timestamp", record.timestamp);
          jsonRecord.SetAs("duration", record.duration);
          jsonRecord.SetAs("jailedBy", record.jailedBy.ID.steamID);
          jsonRecord.SetAs("reason", record.reason);
          jsonRecords.AddToArray(jsonRecord);
        }
        jsonPlayerRecord.SetAs("records", jsonRecords);

        jsonPlayers.AddToArray(jsonPlayerRecord);
      }
      jsonLogfile.SetAs("players", jsonPlayers);

      try {
        JSON.Serialize(LogFilePath, jsonLogfile);
      } catch (Exception e) {
        Log.Write("Could not save {0}: {1}", LOG_FILE, e.Message);
      }

      return;
    }

  }
}

