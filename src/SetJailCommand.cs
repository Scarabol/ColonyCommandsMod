using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
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

      var m = Regex.Match(chattext, @"/setjail ?<range>[0-9]+");
      if (m.Success) {
        uint range = 0;
        try {
          range = System.Convert.ToUInt32(m.Groups["range"].Value, 10);
        } catch (Exception e) {
          Chat.Send(causedBy, $"Syntax error, use /setjail [range]: {e}");
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

