using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class GodChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/god");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "god")) {
        return true;
      }
      if (PermissionsManager.HasPermission (causedBy, "")) {
        PermissionsManager.RemovePermissionOfUser (causedBy, causedBy, "");
        Chat.SendToConnected ($"{causedBy.Name} is a cockroach now!");
      } else {
        PermissionsManager.AddPermissionToUser (causedBy, causedBy, "");
        Chat.SendToConnected ($"{causedBy.Name} is now godlike!");
      }
      return true;
    }
  }
}
