using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class GodChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.god.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new GodChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/god");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "god")) {
          return true;
        }
        if (Permissions.PermissionsManager.HasPermission (causedBy, "")) {
          Permissions.PermissionsManager.RemovePermissionOfUser (causedBy, causedBy, "");
          Chat.SendToAll (string.Format ("{0} is a cockroach now!", causedBy.Name));
        } else {
          Permissions.PermissionsManager.AddPermissionToUser (causedBy, causedBy, "");
          Chat.SendToAll (string.Format ("{0} is now godlike!", causedBy.Name));
        }
      } catch (Exception exception) {
        Log.WriteError (string.Format ("Exception while parsing command; {0} - {1}", exception.Message, exception.StackTrace));
      }
      return true;
    }
  }
}
