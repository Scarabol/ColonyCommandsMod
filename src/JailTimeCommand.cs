using Chatting;
using Chatting.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColonyCommands
{

  public class JailTimeCommand : IChatCommand
  {

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {
		if (!splits[0].Equals("/jailtime")) {
			return false;
		}
      if (!JailManager.IsPlayerJailed(causedBy)) {
        Chat.Send(causedBy, "You are not jailed - free to move!");
        return true;
      }

      long remainTime = JailManager.getRemainingTime(causedBy);
      if (remainTime > 60) {
        Chat.Send(causedBy, String.Format("Remaining Jail Time: {0}:{1:D2} Minutes", remainTime / 60, remainTime % 60));
      } else {
        Chat.Send(causedBy, $"Remaining Jail Time: {remainTime} Seconds");
      }

      return true;
    }
  }
}

