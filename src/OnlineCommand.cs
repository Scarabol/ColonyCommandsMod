using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  public class OnlineChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/online");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        String msg = "";
        for (int c = 0; c < Players.CountConnected; c++) {
          Players.Player player = Players.GetConnectedByIndex (c);
          msg += player.Name + "\n";
        }
        msg += string.Format ("Total {0} players online", Players.CountConnected);
        Chat.Send (causedBy, msg);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
