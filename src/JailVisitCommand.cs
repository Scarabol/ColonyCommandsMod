using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;

namespace ScarabolMods
{

  public class JailVisitCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/jailvisit") || chat.StartsWith("/jailvisit "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
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

