using Chatting;
using Chatting.Commands;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ColonyCommands
{

  public class JailCommand : IChatCommand
  {

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals("/jail")) {
		return false;
		}
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "jail")) {
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

