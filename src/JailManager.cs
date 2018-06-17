using System;
using System.Collections.Generic;
using System.IO;
using Pipliz;
using Pipliz.JSON;
using UnityEngine;

namespace ScarabolMods {

  public static class JailManager
  {
    static Vector3Int jailPosition;
    static uint jailRange;
    static Dictionary<Players.Player, JailRecord> jailedPersons = new Dictionary<Players.Player, JailRecord>();
    static string CONFIG_FILE = "jail-config.json";
    public static bool validJail = false;
    public const uint DEFAULT_RANGE = 5;
    public const uint DEFAULT_JAIL_TIME = 3;

    // Jail record per player
    public class JailRecord {
      public uint timeLeft { get; set; }
      public Players.Player jailedBy { get; set; }
      public string jailReason { get; set; }

      public JailRecord(uint time, Players.Player causedBy, string reason)
      {
        timeLeft = time;
        jailedBy = causedBy;
        jailReason = reason;
      }
    }

    static string ConfigfilePath {
      get {
        return Path.Combine(Path.Combine("gamedata", "savegames"), Path.Combine(ServerManager.WorldName, CONFIG_FILE));
      }
    }

    // send a player into the jail
    public static void jailPlayer(Players.Player criminal, Players.Player causedBy, string reason, uint jailtime = DEFAULT_JAIL_TIME)
    {
      if (!validJail) {
        return;
      }
      JailRecord record = new JailRecord(jailtime, causedBy, reason);
      jailedPersons.Add(criminal, record);
      return;
    }

    // update/set the jail position in the world
    public static void setJailPosition(Vector3 newPosition, uint range = DEFAULT_RANGE)
    {
      Vector3Int position;
      position.x = (int)newPosition.x;
      position.y = (int)newPosition.y + 1;  // one block higher to prevent clipping
      position.z = (int)newPosition.z;

      jailPosition = position;
      jailRange = range;
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

      try {
        JSONNode position;
        jsonConfig.TryGetAs("position", out position);
        jailPosition.x = position.GetAs<int>("x");
        jailPosition.y = position.GetAs<int>("y");
        jailPosition.z = position.GetAs<int>("z");
        jailRange = position.GetAs<uint>("range");
        validJail = true;

        JSONNode players;
        jsonConfig.TryGetAs("players", out players);
        foreach (JSONNode node in players.LoopArray()) {
          string PlayerName = node.GetAs<string>("criminal");
          uint jailTimeLeft = node.GetAs<uint>("time");
          string causedByName = node.GetAs<string>("jailedBy");
          string reason = node.GetAs<string>("jailReason");

          Players.Player criminal;
          Players.Player causedBy;
          string error;

          if (PlayerHelper.TryGetPlayer(PlayerName, out criminal, out error, true) &&
            PlayerHelper.TryGetPlayer(causedByName, out causedBy, out error, true)) {
            JailRecord record = new JailRecord(jailTimeLeft, causedBy, reason);
            jailedPersons.Add(criminal, record);
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
      JSONNode jsonConfig = new JSONNode();
      JSONNode jsonPosition = new JSONNode();
      jsonPosition.SetAs("x", jailPosition.x);
      jsonPosition.SetAs("y", jailPosition.y);
      jsonPosition.SetAs("z", jailPosition.z);
      jsonPosition.SetAs("range", jailRange);
      jsonConfig.SetAs("position", jsonPosition);

      JSONNode jsonPlayers = new JSONNode(NodeType.Array);
      foreach (KeyValuePair<Players.Player, JailRecord> kvp in jailedPersons) {
        Players.Player criminal = kvp.Key;
        JailRecord record = kvp.Value;
        JSONNode jsonRecord = new JSONNode();
        jsonRecord.SetAs("criminal", criminal.ID.steamID);
        jsonRecord.SetAs("time", record.timeLeft);
        jsonRecord.SetAs("jailedBy", record.jailedBy.Name);
        jsonRecord.SetAs("jailReason", record.jailReason);
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

  } // class

} // namespace

