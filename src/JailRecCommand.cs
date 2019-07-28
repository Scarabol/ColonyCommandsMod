using Chatting;
using Chatting.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColonyCommands
{

  public class JailRecCommand : IChatCommand
  {

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {
		if (!splits[0].Equals("/jailrec")) {
			return false;
		}
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "jail")) {
        return true;
      }

      // get log by player
      var m = Regex.Match(chattext, @"/jailrec (?<target>.+)");
      if (m.Success) {
        string targetName = m.Groups["target"].Value;
        Players.Player target;
        string error;
        if (!PlayerHelper.TryGetPlayer(targetName, out target, out error)) {
          Chat.Send(causedBy, $"Could not find {targetName}: {error}");
          return true;
        }
        List<JailManager.JailLogRecord> PlayerJailLog;
        if (!JailManager.jailLog.TryGetValue(target, out PlayerJailLog)) {
          Chat.Send(causedBy, $"No records found - {targetName} is clean");
          return true;
        }
        foreach (JailManager.JailLogRecord record in PlayerJailLog) {
          DateTime timestamp = new DateTime(record.timestamp * TimeSpan.TicksPerMillisecond * 1000);
          string jailedBy = "Server";
          if (record.jailedBy != null) {
            jailedBy = record.jailedBy.Name;
          }
          Chat.Send(causedBy, String.Format("{0} by: {1} for: {2}", timestamp.ToString(), jailedBy, record.reason));
        }

      // or get the full log
      } else {
        // in full log mode only timestamp and playername are displayed (last 10 jailed).
        // showing more would require a more capable chat window
        List<combinedJailLog> combinedLog = new List<combinedJailLog>();
        foreach (KeyValuePair<Players.Player, List<JailManager.JailLogRecord>> kvp in JailManager.jailLog) {
          Players.Player target = kvp.Key;
          List<JailManager.JailLogRecord> records = kvp.Value;
          foreach (JailManager.JailLogRecord record in records) {
            combinedLog.Add(new combinedJailLog(target, record.jailedBy, record.timestamp));
          }
        }
        combinedLog.Sort(delegate(combinedJailLog a, combinedJailLog b)
        {
          return a.timestamp.CompareTo(b.timestamp);
        });

        int limit = 10;
        if (combinedLog.Count < limit) {
          limit = combinedLog.Count;
        }

        Chat.Send(causedBy, "Jail Records (last 10):");
        for (int i = 0; i < limit; ++i) {
          combinedJailLog record = combinedLog[i];
          DateTime timestamp = new DateTime(record.timestamp * TimeSpan.TicksPerMillisecond * 1000);
          string targetName = record.target.Name;
          string jailerName = "Server";
          if (record.causedBy != null) {
            jailerName = record.causedBy.Name;
          }
          Chat.Send(causedBy, String.Format("{0} {1} by: {2}", timestamp.ToString(), targetName, jailerName));
        }
      }

      return true;
    }

    private class combinedJailLog
    {
      public Players.Player target { get; set; }
      public Players.Player causedBy { get; set; }
      public long timestamp { get; set; }

      public combinedJailLog(Players.Player tgt, Players.Player src, long time)
      {
        target = tgt;
        causedBy = src;
        timestamp = time;
      }
    }

  }
}

