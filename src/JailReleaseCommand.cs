using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class JailReleaseCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jail_release") || chat.StartsWith("/jail_release "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "jail")) {
        return true;
      }

      var m = Regex.Match(chattext, @"/jail_release (?<player>[^ ]+)");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax error, use /jail_release <player>");
        return true;
      }

      Players.Player criminal;
      string criminalName = m.Groups["player"].Value;
      string error;
      if (!PlayerHelper.TryGetPlayer(criminalName, out criminal, out error, true)) {
        Chat.Send(causedBy, $"Could not find player {criminalName}: {error}");
        return true;
      }

      if (!JailManager.IsPlayerJailed(criminal)) {
        Chat.Send(causedBy, $"{criminal.Name} is currently not in jail");
        return true;
      }

      JailManager.releasePlayer(criminal, causedBy);
      Chat.Send(causedBy, $"Released {criminal.Name} from jail");
      Chat.Send(criminal, $"{causedBy.Name} released you from jail");

      return true;
    }
  }
}

