using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class KickChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.kick.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new KickChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/kick") || chat.StartsWith ("/kick ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "kick")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/kick (?<targetplayername>['].+?[']|[^ ]+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /kick [targetplayername]");
          return true;
        }
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        Chat.Send (targetPlayer, "<color=red>You were kicked from the server</color>");
        Chat.SendToAll (string.Format ("{0} is kicked by {1}", targetPlayer.Name, causedBy.Name));
        Players.Disconnect (targetPlayer);
      } catch (Exception exception) {
        Log.WriteError (string.Format ("Exception while parsing command; {0} - {1}", exception.Message, exception.StackTrace));
      }
      return true;
    }
  }
}
