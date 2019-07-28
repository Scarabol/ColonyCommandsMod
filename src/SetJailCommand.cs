using Chatting;
using Chatting.Commands;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColonyCommands
{

  public class SetJailCommand : IChatCommand
  {

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals("/setjail")) {
		return false;
	}
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "setjailposition")) {
        return true;
      }

      if (chattext.Equals("/setjail visitor")) {
        JailManager.setJailVisitorPosition(causedBy.Position);
        Chat.Send(causedBy, "Jail visiting position set");
        return true;
      }

      var m = Regex.Match(chattext, @"/setjail (?<range>[0-9]+)");
      if (m.Success) {
        uint range = 0;
        if (!uint.TryParse(m.Groups["range"].Value, out range)) {
          Chat.Send(causedBy, "Could not parse range value");
        }
        JailManager.setJailPosition(causedBy, range);
        Chat.Send(causedBy, $"Jail set to your current position with range {range}");
      } else {
        JailManager.setJailPosition(causedBy);
        Chat.Send(causedBy, $"Jail set to your current position and default range {JailManager.DEFAULT_RANGE}");
      }
      return true;
    }
  }
}

