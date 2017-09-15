using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class KickChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.kick.registercommand")]
    public static void AfterItemTypesServer ()
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
        Log.Write ("some");
        var m = Regex.Match (chattext, @"/kick (?<targetplayername>['].+?[']|[^ ]+)");
        Log.Write ("thing");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /kick [targetplayername]");
          return true;
        }
        Log.Write ("is");
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Log.Write ("wrong");
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        Log.Write ("here");
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
