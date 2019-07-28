using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class LastSeenChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals("/lastseen")) {
		return false;
		}
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
      var lastSeen = ActivityTracker.GetLastSeen (targetPlayer.ID.ToStringReadable());
      Chat.Send (causedBy, $"Player {targetPlayer.ID.ToStringReadable()} last seen {lastSeen}");
      return true;
    }
  }
}
