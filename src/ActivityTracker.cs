using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Pipliz;
using Pipliz.JSON;

namespace ColonyCommands
{
  [ModLoader.ModManager]
  public static class ActivityTracker
  {
    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "playeractivity.json"));
      }
    }

    static Dictionary<string, StatsDataEntry> PlayerStats = new Dictionary<string, StatsDataEntry> ();

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.activitytracker.starttimers")]
    public static void AfterWorldLoad ()
    {
      Load ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, "scarabol.commands.activitytracker.onplayerconnectedlate")]
    public static void OnPlayerConnectedLate (Players.Player player)
    {
      var now = DateTime.Now.ToString ();
      var stats = GetOrCreateStats (player.ID.ToStringReadable());
      stats.lastSeen = now;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerDisconnected, "scarabol.commands.activitytracker.onplayerdisconnected")]
    public static void OnPlayerDisconnected (Players.Player player)
    {
      var now = DateTime.Now;
      var stats = GetOrCreateStats (player.ID.ToStringReadable());
      stats.secondsPlayed += (long)now.Subtract (DateTime.Parse (stats.lastSeen)).TotalSeconds;
      stats.lastSeen = now.ToString ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAutoSaveWorld, "scarabol.commands.activitytracker.onautosaveworld")]
    public static void OnAutoSaveWorld ()
    {
      var now = DateTime.Now;
      for (var c = 0; c < Players.CountConnected; c++) {
        var player = Players.GetConnectedByIndex (c);
        var stats = GetOrCreateStats (player.ID.ToStringReadable());
        stats.secondsPlayed += (long)now.Subtract (DateTime.Parse (stats.lastSeen)).TotalSeconds;
        stats.lastSeen = now.ToString ();
      }
      Save ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnQuit, "scarabol.commands.activitytracker.onquit")]
    public static void OnQuit()
    {
      Save ();
    }

    public static void Load ()
    {
      try {
        JSONNode playerActivity;
        if (JSON.Deserialize (ConfigFilepath, out playerActivity, false)) {
          JSONNode jsonStats;
          if (!playerActivity.TryGetAs ("stats", out jsonStats) || jsonStats.NodeType != NodeType.Object) {
            Log.WriteError ($"No player 'stats' defined in {ConfigFilepath}");
          } else {
            PlayerStats.Clear ();
            foreach (var jsonPlayerStats in jsonStats.LoopObject ()) {
              var playerId = jsonPlayerStats.Key;
              var stats = (StatsDataEntry)jsonPlayerStats.Value;
              if (stats != null) {
                PlayerStats.Add (playerId, stats);
              }
            }
            Log.Write ($"Loaded {PlayerStats.Count} player stats from file");
          }
        }
      } catch (Exception exception) {
        Log.WriteError ($"Exception while loading player activity; {exception.Message}");
      }
    }

    static void Save ()
    {
      try {
        JSONNode JsonPlayerStats = new JSONNode ();
        foreach (var playerStats in PlayerStats) {
          JsonPlayerStats.SetAs (playerStats.Key, (JSONNode)playerStats.Value);
        }
        JSONNode JsonActivity = new JSONNode ();
        JsonActivity.SetAs ("stats", JsonPlayerStats);
        JSON.Serialize (ConfigFilepath, JsonActivity, 3);
      } catch (Exception exception) {
        Log.WriteError ($"Exception while saving player activity; {exception.Message}");
      }
    }

    public static StatsDataEntry GetOrCreateStats (string playerId)
    {
      StatsDataEntry stats;
      if (!PlayerStats.TryGetValue (playerId, out stats)) {
        stats = new StatsDataEntry ();
        PlayerStats.Add (playerId, stats);
      }
      return stats;
    }

    public static string GetLastSeen (string playerId)
    {
      StatsDataEntry stats;
      if (!PlayerStats.TryGetValue (playerId, out stats)) {
        return "never";
      }
      return stats.lastSeen;
    }

    public static Dictionary<Players.Player, int> GetInactivePlayers(int days, int max = 0)
    {
      var result = new Dictionary<Players.Player, int>();
      foreach (var player in Players.PlayerDatabase.Values) {
        StatsDataEntry stats = GetOrCreateStats(player.ID.ToStringReadable());
        double inactiveDays = DateTime.Now.Subtract(DateTime.Parse(stats.lastSeen)).TotalDays;
        if (inactiveDays >= days && (max == 0 || inactiveDays <= max)) {
          result.Add(player, (int)inactiveDays);
        }
      }
      return result;
    }

    public class StatsDataEntry
    {
      public string lastSeen = "";
      public long secondsPlayed;

      public StatsDataEntry ()
        : this (DateTime.Now.ToString (), 0)
      {
      }

      public StatsDataEntry (String lastSeen, long secondsPlayed)
      {
        this.lastSeen = lastSeen;
        this.secondsPlayed = secondsPlayed;
      }

      public static explicit operator JSONNode (StatsDataEntry stats)
      {
        JSONNode json = new JSONNode ();
        json.SetAs ("lastSeen", stats.lastSeen);
        json.SetAs ("secondsPlayed", stats.secondsPlayed);
        return json;
      }

      public static explicit operator StatsDataEntry (JSONNode json)
      {
        string lastSeen;
        json.TryGetAs ("lastSeen", out lastSeen);
        if (lastSeen == null || lastSeen.Length < 1) {
          lastSeen = DateTime.Now.ToString ();
        }
        long secondsPlayed;
        if (!json.TryGetAs ("secondsPlayed", out secondsPlayed)) {
          secondsPlayed = 0;
        }
        return new StatsDataEntry (lastSeen, secondsPlayed);
      }
    }
  }
}
