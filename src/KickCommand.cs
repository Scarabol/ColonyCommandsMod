using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ColonyCommands
{

  public class KickChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/kick") || chat.StartsWith ("/kick ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "kick")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/kick (?<targetplayername>['].+[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /kick [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      Chat.Send (targetPlayer, "<color=red>You were kicked from the server</color>");
      Chat.SendToAll ($"{targetPlayer.Name} is kicked by {causedBy.Name}");
      Players.Disconnect (targetPlayer);
      return true;
    }
  }
}
