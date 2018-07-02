using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;

namespace ColonyCommands
{

  public class LastSeenChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/lastseen") || chat.StartsWith ("/lastseen ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var m = Regex.Match (chattext, @"/lastseen (?<playername>['].+[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /lastseen [playername]");
        return true;
      }
      var targetPlayerName = m.Groups ["playername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
        Chat.Send (causedBy, $"Could not find player '{targetPlayerName}'; {error}");
        return true;
      }
      var lastSeen = ActivityTracker.GetLastSeen (targetPlayer.IDString);
      Chat.Send (causedBy, $"Player {targetPlayer.IDString} last seen {lastSeen}");
      return true;
    }
  }
}
