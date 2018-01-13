using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class ActivityTracker
  {
    private static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "playeractivity.json"));
      }
    }

    private static Dictionary<string, StatsDataEntry> PlayerStats = new Dictionary<string, StatsDataEntry> ();

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.activitytracker.starttimers")]
    public static void AfterWorldLoad ()
    {
      Load ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, "scarabol.commands.activitytracker.onplayerconnectedlate")]
    public static void OnPlayerConnectedLate (Players.Player player)
    {
      String now = DateTime.Now.ToString ();
      StatsDataEntry stats = GetOrCreateStats (player.IDString);
      stats.lastSeen = now;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerDisconnected, "scarabol.commands.activitytracker.onplayerdisconnected")]
    public static void OnPlayerDisconnected (Players.Player player)
    {
      DateTime now = DateTime.Now;
      StatsDataEntry stats = GetOrCreateStats (player.IDString);
      stats.secondsPlayed += (long)now.Subtract (DateTime.Parse (stats.lastSeen)).TotalSeconds;
      stats.lastSeen = now.ToString ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAutoSaveWorld, "scarabol.commands.activitytracker.onautosaveworld")]
    public static void OnAutoSaveWorld ()
    {
      DateTime now = DateTime.Now;
      for (int c = 0; c < Players.CountConnected; c++) {
        Players.Player player = Players.GetConnectedByIndex (c);
        StatsDataEntry stats = GetOrCreateStats (player.IDString);
        stats.secondsPlayed += (long)now.Subtract (DateTime.Parse (stats.lastSeen)).TotalSeconds;
        stats.lastSeen = now.ToString ();
      }
      Save ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnQuitLate, "scarabol.commands.activitytracker.onquitlate")]
    public static void OnQuitLate ()
    {
      Save ();
    }

    public static void Load ()
    {
      try {
        JSONNode json;
        if (JSON.Deserialize (ConfigFilepath, out json, false)) {
          JSONNode PlayerActivity = json;
          JSONNode jsonStats;
          if (!PlayerActivity.TryGetAs ("stats", out jsonStats) || jsonStats.NodeType != NodeType.Object) {
            Pipliz.Log.WriteError ($"No player 'stats' defined in {ConfigFilepath}");
          } else {
            PlayerStats.Clear ();
            foreach (KeyValuePair<string, JSONNode> jsonPlayerStats in jsonStats.LoopObject()) {
              string playerId = jsonPlayerStats.Key;
              StatsDataEntry stats = (StatsDataEntry)jsonPlayerStats.Value;
              if (stats != null) {
                PlayerStats.Add (playerId, stats);
              }
            }
            Pipliz.Log.Write (string.Format ("Loaded {0} player stats from file", PlayerStats.Count));
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while loading player activity; {0}", exception.Message));
      }
    }

    private static void Save ()
    {
      try {
        JSONNode JsonPlayerStats = new JSONNode ();
        foreach (KeyValuePair<string, StatsDataEntry> playerStats in PlayerStats) {
          JsonPlayerStats.SetAs (playerStats.Key, (JSONNode)playerStats.Value);
        }
        JSONNode JsonActivity = new JSONNode ();
        JsonActivity.SetAs ("stats", JsonPlayerStats);
        JSON.Serialize (ConfigFilepath, JsonActivity, 3);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while saving player activity; {0}", exception.Message));
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
      } else {
        return stats.lastSeen;
      }
    }

    public class StatsDataEntry
    {
      public string lastSeen = "";
      public long secondsPlayed = 0;

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