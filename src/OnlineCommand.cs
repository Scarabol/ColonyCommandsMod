using System;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class OnlineChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.online.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new OnlineChatCommand ());
    }

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
          msg += player.Name;
          if (c < Players.CountConnected - 1) {
            msg += ", ";
          }
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
