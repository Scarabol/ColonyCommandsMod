using System;
using System.Collections.Generic;
using System.IO;
using Pipliz;
using Pipliz.JSON;
using Chatting;
using TerrainGeneration;
using UnityEngine;

namespace ColonyCommands {

  [ModLoader.ModManager]
  public static class JailManager
  {
    static Vector3 jailPosition;
    static Vector3 jailVisitorPosition;
    static uint jailRange;
    static Dictionary<Players.Player, JailRecord> jailedPersons = new Dictionary<Players.Player, JailRecord>();
    public static Dictionary<Players.Player, List<JailLogRecord>> jailLog = new Dictionary<Players.Player, List<JailLogRecord>>();
    public static Dictionary<Players.Player, Vector3> visitorPreviousPos = new Dictionary<Players.Player, Vector3>();
    const string CONFIG_FILE = "jail-config.json";
    const string LOG_FILE = "jail-log.json";
    public static bool validJail = false;
    public static bool validVisitorPos = false;
    public static uint DEFAULT_JAIL_TIME = 3;
    public static uint GRACE_ESCAPE_ATTEMPTS = 3;
    public const uint DEFAULT_RANGE = 5;

    // Jail record per player
    private class JailRecord {
      public int gracePeriod { get; set; }
      public int escapeAttempts { get; set; }
      public long jailTimestamp { get; set; }
      public long jailDuration { get; set; }
      public Players.Player jailedBy { get; set; }
      public string jailReason { get; set; }
      public List<string> revokedPermissions { get; set; }

      public JailRecord(long time, long duration, Players.Player causedBy, string reason, List<string> permissions)
      {
        this.gracePeriod = 2;
        this.escapeAttempts = 0;
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
    public static void jailPlayer(Players.Player target, Players.Player causedBy, string reason, long jailtime)
    {
      if (!validJail) {
        if (causedBy == null) {
          Log.Write($"Cannot Jail {target.Name}: no valid jail found");
        } else {
          Chat.Send(causedBy, "<color=yellow>No valid jail found. Unable to complete jailing</color>");
        }
        return;
      }

		Helper.TeleportPlayer(target, jailPosition);

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

      string sender;
      if (causedBy == null) {
        sender = "Server";
      } else {
        sender = causedBy.Name;
      }
      Chat.Send(target, $"<color=red>{sender} threw you into jail! Reason: {reason}</color>");
      Chat.Send(target, $"Remaining Jail Time: {jailtime} minutes, type /jailtime to check");
      Chat.SendToConnectedBut(target, $"<color=red>{sender} threw {target.Name} into jail! Reason: {reason}</color>");
      Log.Write($"{sender} threw {target.Name} into jail! Reason: {reason}");
      return;
    }

    // visit the jail - no harm is done
    public static void VisitJail(Players.Player causedBy)
    {
      if (validJail && validVisitorPos) {
        visitorPreviousPos[causedBy] = causedBy.Position;
        Helper.TeleportPlayer(causedBy, jailVisitorPosition);
        Chat.Send(causedBy, "Welcome Visitor! You're free to leave anytime, /jailleave will bring you back to your previous location");
      }
      return;
    }

    // update/set the jail position in the world
    public static void setJailPosition(Players.Player causedBy, uint range = DEFAULT_RANGE)
    {
      // if an old jail position existed remove its protection area
      if (validJail) {
        Pipliz.Vector3Int oldPos = new Pipliz.Vector3Int(jailPosition);
        CustomProtectionArea oldJail = null;
        foreach (CustomProtectionArea area in AntiGrief.CustomAreas) {
          if (area.Equals(oldPos, range)) {
            oldJail = area;
          }
        }
        if (oldJail != null) {
          AntiGrief.RemoveCustomArea(oldJail);
          Chat.Send(causedBy, String.Format("Removed old jail protection area at {0} {1}", (int)jailPosition.x, (int)jailPosition.z));
        }
      }

      jailPosition.x = causedBy.Position.x;
      jailPosition.y = causedBy.Position.y + 1;  // one block higher to prevent clipping
      jailPosition.z = causedBy.Position.z;
      jailRange = range;
      validJail = true;
      Save();

      Pipliz.Vector3Int playerPos = new Pipliz.Vector3Int(causedBy.Position);
      AntiGrief.AddCustomArea(new CustomProtectionArea(playerPos, (int)range, (int)range));
      Chat.Send(causedBy, "Created new custom protection area");

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

        uint defaultJailTime;
        if (jsonConfig.TryGetAs("defaultJailTime", out defaultJailTime)) {
          DEFAULT_JAIL_TIME = defaultJailTime;
        }

        uint graceEscapeAttempts;
        if (jsonConfig.TryGetAs("graceEscapeAttempts", out graceEscapeAttempts)) {
          GRACE_ESCAPE_ATTEMPTS = graceEscapeAttempts;
        }

        JSONNode players;
        jsonConfig.TryGetAs("players", out players);
        foreach (JSONNode node in players.LoopArray()) {
          string PlayerName = node.GetAs<string>("target");
          long jailTimestamp = node.GetAs<long>("time");
          long jailDuration = node.GetAs<long>("duration");
          string causedByName = node.GetAs<string>("jailedBy");
          string reason = node.GetAs<string>("jailReason");

          List<string> permissions = node.GetAs<List<string>>("permissions");  // TODO
          // List<string> permissions = new List<string>;
          // JSONNode jsonPerm;
          // node.TryGetAs("permissions", out jsonPerm);
          // foreach (string perm in jsonPerm.LoopArray()) {
          //   permissions.Add(perm);
          // }

          Players.Player target;
          Players.Player causedBy;
          string error;

          // causedBy can be null in case of server actions, but target has to be a valid player
          PlayerHelper.TryGetPlayer(causedByName, out causedBy, out error, true);
          if (PlayerHelper.TryGetPlayer(PlayerName, out target, out error, true)) {
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

      jsonConfig.SetAs("defaultJailTime", DEFAULT_JAIL_TIME);
      jsonConfig.SetAs("graceEscapeAttempts", GRACE_ESCAPE_ATTEMPTS);

      JSONNode jsonPlayers = new JSONNode(NodeType.Array);
      foreach (KeyValuePair<Players.Player, JailRecord> kvp in jailedPersons) {
        Players.Player target = kvp.Key;
        JailRecord record = kvp.Value;
        JSONNode jsonRecord = new JSONNode();
        jsonRecord.SetAs("target", target.ID.steamID);
        jsonRecord.SetAs("time", record.jailTimestamp);
        jsonRecord.SetAs("duration", record.jailDuration);
        jsonRecord.SetAs("jailReason", record.jailReason);
        if (record.jailedBy != null) {
          jsonRecord.SetAs("jailedBy", record.jailedBy.Name);
        } else {
          jsonRecord.SetAs("jailedBy", "Server");
        }

        // JSONNode permissions = new JSONNode(NodeType.Array);
        // foreach (string perm in record.revokedPermissions) {
        //   permissions.AddToArray(perm);
        // }
        //jsonRecord.SetAs("permissions", record.revokedPermissions.ToString());  // TODO
        jsonRecord.SetAs("permissions", "none");
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
      JailRecord record;
      jailedPersons.TryGetValue(target, out record);
      foreach (string permission in record.revokedPermissions) {
        // PermissionsManager.AddPermissionToUser(causedBy, target, permission);
      }
      jailedPersons.Remove(target);
      Save();

      Helper.TeleportPlayer(target, ServerManager.TerrainGenerator.GetDefaultSpawnLocation().Vector);
      Chat.Send(target, "<color=yellow>You did your time and are released from Jail</color>");
      if (causedBy != null) {
        Log.Write($"{causedBy.Name} released {target.Name} from jail");
      } else {
        Log.Write($"Released {target.Name} from jail");
      }
      return;
    }

    // track jailed players movement
    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerMoved, AntiGrief.NAMESPACE + ".OnPlayerMoved")]
    public static void OnPlayerMoved(Players.Player causedBy, Vector3 pos)
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
        Helper.TeleportPlayer(causedBy, jailPosition);

        ++record.escapeAttempts;
        if (GRACE_ESCAPE_ATTEMPTS == 0 || record.escapeAttempts < GRACE_ESCAPE_ATTEMPTS) {
          Chat.Send(causedBy, "<color=red>A Guard spots your escape attempt and pushes you back</color>");
        } else {
          record.jailDuration += 1 * 60;
          Chat.Send(causedBy, "<color=red>The Guards get angry at you and increase your jailtime by 1 minute</color>");
        }
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

    // calculate remaining jailtime
    public static long getRemainingTime(Players.Player causedBy)
    {
      JailRecord record = jailedPersons[causedBy];
      long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
      long remaining = record.jailTimestamp + record.jailDuration - now;

      return remaining;
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
          jsonRecord.SetAs("reason", record.reason);
          if (record.jailedBy != null) {
            jsonRecord.SetAs("jailedBy", record.jailedBy.ID.steamID);
          } else {
            jsonRecord.SetAs("jailedBy", 0);
          }
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

