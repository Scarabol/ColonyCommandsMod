﻿using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class SetJailCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/setjail") || chat.StartsWith("/setjail "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "setjailposition")) {
        return true;
      }

      if (chattext.Equals("/setjail visitor")) {
        JailManager.setJailVisitorPosition(causedBy.Position);
        Chat.Send(causedBy, "Jail visiting position set");
      }

      var m = Regex.Match(chattext, @"/setjail (?<range>[0-9]+)");
      if (m.Success) {
        uint range = 0;
        if (!uint.TryParse(m.Groups["range"].Value, out range)) {
          Chat.Send(causedBy, "Could not parse range value");
        }
        JailManager.setJailPosition(causedBy.Position, range);
        Chat.Send(causedBy, $"Jail set to your current position with range {range}");
      } else {
        JailManager.setJailPosition(causedBy.Position);
        Chat.Send(causedBy, $"Jail set to your current position and default range {JailManager.DEFAULT_RANGE}");
      }
      return true;
    }
  }
}
