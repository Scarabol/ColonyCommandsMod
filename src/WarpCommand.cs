using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class WarpChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/warp")) {
		return false;
	}
      if (!PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.player") &&
          !PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.self")) {
        Chat.Send (causedBy, "<color=red>You don't have permission to warp</color>");
        return true;
      }
      var m = Regex.Match (chattext, @"/warp (?<targetplayername>['].+[']|[^ ]+)( (?<teleportplayername>['].+[']|[^ ]+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /warp [targetplayername] or /warp [targetplayername] [teleportplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      var teleportPlayer = causedBy;
      var teleportPlayerName = m.Groups ["teleportplayername"].Value;
      if (teleportPlayerName.Length > 0) {
        if (PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.player")) {
          if (!PlayerHelper.TryGetPlayer (teleportPlayerName, out teleportPlayer, out error)) {
            Chat.Send (causedBy, $"Could not find teleport player '{teleportPlayerName}'; {error}");
            return true;
          }
        } else {
          Chat.Send (causedBy, "<color=red>You don't have permission to warp other players</color>");
          return true;
        }
      }
      Teleport.TeleportTo (teleportPlayer, targetPlayer.Position);
      return true;
    }
  }
}
