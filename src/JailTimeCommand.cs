using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class JailTimeCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return chat.Equals("/jailtime");
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {

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

