using System.Text.RegularExpressions;
using Pipliz.Chatting;
using Permissions;
using ChatCommands;

namespace ScarabolMods
{

  public class BanChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/ban") || chat.StartsWith ("/ban ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "ban")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/ban (?<targetplayername>['].+?[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /ban [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      Chat.Send (targetPlayer, "<color=red>You were banned from the server</color>");
      Chat.SendToAll ($"{targetPlayer.Name} is banned by {causedBy.Name}");
      BlackAndWhitelisting.AddBlackList (targetPlayer.ID.steamID.m_SteamID);
      Players.Disconnect (targetPlayer);
      return true;
    }
  }
}
