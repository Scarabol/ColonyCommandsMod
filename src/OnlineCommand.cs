﻿using Pipliz.Chatting;
using ChatCommands;

namespace ColonyCommands
{

  public class OnlineChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/online");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var msg = "";
      for (var c = 0; c < Players.CountConnected; c++) {
        Players.Player player = Players.GetConnectedByIndex (c);
        msg += player.Name;
        if (c < Players.CountConnected - 1) {
          msg += ", ";
        }
      }
      msg += $"\nTotal {Players.CountConnected} players online";
      Chat.Send (causedBy, msg);
      return true;
    }
  }
}
