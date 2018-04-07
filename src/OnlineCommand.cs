using Pipliz.Chatting;
using ChatCommands;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class OnlineChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.online.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new OnlineChatCommand ());
    }

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
