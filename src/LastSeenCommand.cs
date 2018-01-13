using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class LastSeenChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.lastseen.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new LastSeenChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/lastseen") || chat.StartsWith ("/lastseen ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        var m = Regex.Match (chattext, @"/lastseen (?<playername>['].+?[']|[^ ]+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /lastseen [playername]");
          return true;
        }
        string targetPlayerName = m.Groups ["playername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        string lastSeen = ActivityTracker.GetLastSeen (targetPlayer.IDString);
        Chat.Send (causedBy, string.Format ("Player {0} last seen {1}", targetPlayer.IDString, lastSeen));
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
