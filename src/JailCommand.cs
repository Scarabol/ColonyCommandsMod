using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;

namespace ColonyCommands
{

  public class JailCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jail") || chat.StartsWith("/jail "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + ".jail")) {
        return true;
      }

      var m = Regex.Match(chattext, @"/jail (?<player>['].+[']|[^ ]+) (?<jailtime>[0-9]+)? ?(?<reason>.+)$");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax error, use /jail <player> [time] <reason>");
        return true;
      }

      Players.Player target;
      string targetName = m.Groups["player"].Value;
      string error;
      if (!PlayerHelper.TryGetPlayer(targetName, out target, out error, true)) {
        Chat.Send(causedBy, $"Could not find player {targetName}: {error}");
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

      if (JailManager.IsPlayerJailed(target)) {
        Chat.Send(causedBy, $"{target.Name} is already in jail.");
        return true;
      }

      JailManager.jailPlayer(target, causedBy, reason, jailtime);
      Chat.Send(causedBy, $"Threw {target.Name} into the jail for {jailtime} minutes");

      return true;
    }
  }
}

