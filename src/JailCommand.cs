using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class JailCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jail") || chat.StartsWith("/jail "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "jail")) {
        return true;
      }

      var m = Regex.Match(chattext, @"/jail (?<player>[^ ]+) (?<jailtime>[0-9]+)? ?(?<reason>.+)$");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax error, use /jail <player> [time] <reason>");
        return true;
      }

      Players.Player criminal;
      string criminalName = m.Groups["player"].Value;
      string error;
      if (!PlayerHelper.TryGetPlayer(criminalName, out criminal, out error, true)) {
        Chat.Send(causedBy, $"Could not find player {criminalName}: {error}");
        return true;
      }

      uint jailtime = 0;
      var timeval = m.Groups["jailtime"].Value;
      if (timeval.Equals("")) {
        jailtime = JailManager.DEFAULT_JAIL_TIME;
      } else {
        if (!uint.TryParse(timeval, out jailtime)) {
          Chat.Send(causedBy, $"Could not identify time value {timeval}");
        }
      }
      string reason = m.Groups["reason"].Value;

      if (JailManager.IsPlayerJailed(criminal)) {
        Chat.Send(causedBy, $"{criminal.Name} is already in jail. Use /jail_extend in case");
        return true;
      }

      JailManager.jailPlayer(criminal, causedBy, reason, jailtime);
      Chat.Send(causedBy, $"Threw {criminal.Name} into the jail for {jailtime} minutes");

      return true;
    }
  }
}

