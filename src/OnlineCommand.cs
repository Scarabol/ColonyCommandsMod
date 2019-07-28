using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class OnlineChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals("/online") || chat.Equals("/online id");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      string msg = "";
      bool idMode = false;
      if (chattext.Equals("/online id")) {
        idMode = true;
      }

      for (var c = 0; c < Players.CountConnected; c++) {
        Players.Player player = Players.GetConnectedByIndex (c);
        msg += player.Name;
        if (idMode) {
          msg += ": #" + player.ID.steamID.GetHashCode();
        }
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
