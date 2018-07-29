using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ColonyCommands
{

  public class KillPlayerChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/killplayer") || chat.StartsWith ("/killplayer ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "killplayer")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/killplayer (?<targetplayername>['].+[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /killplayer [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      Players.OnDeath (targetPlayer);
      targetPlayer.SendHealthPacket ();
      if (targetPlayer == causedBy) {
        Chat.SendToAll ($"Player {causedBy.Name} committed suicide");
      } else {
        Chat.SendToAll ($"Player {targetPlayer.Name} was killed by {causedBy.Name}");
      }
      return true;
    }
  }
}
