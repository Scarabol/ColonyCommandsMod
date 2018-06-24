using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class JailRecCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jailrec") || chat.StartsWith("/jailrec "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {

      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + ".jailrec")) {
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
          Chat.Send(causedBy, String.Format("{0} by {1}: {2}", timestamp.ToString(), record.jailedBy, record.reason));
        }

      // or get the full log
      } else {
        Chat.Send(causedBy, "yet to be implemented - try /jailrec <player> instead");
      }

      return true;
    }
  }
}

