using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class GodChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.god.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new GodChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/god");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "god")) {
        return true;
      }
      if (PermissionsManager.HasPermission (causedBy, "")) {
        PermissionsManager.RemovePermissionOfUser (causedBy, causedBy, "");
        Chat.SendToAll ($"{causedBy.Name} is a cockroach now!");
      } else {
        PermissionsManager.AddPermissionToUser (causedBy, causedBy, "");
        Chat.SendToAll ($"{causedBy.Name} is now godlike!");
      }
      return true;
    }
  }
}
