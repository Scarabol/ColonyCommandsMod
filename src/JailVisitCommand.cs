using Chatting;
using Chatting.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColonyCommands
{

  public class JailVisitCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jailvisit") || chat.StartsWith("/jailvisit "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {

      if (!JailManager.validVisitorPos) {
        Chat.Send(causedBy, "Found no valid jail visitor position");
        return true;
      }

      JailManager.VisitJail(causedBy);

      return true;
    }
  }
}

