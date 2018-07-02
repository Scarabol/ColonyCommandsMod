using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using ChatCommands.Implementations;

namespace ColonyCommands
{

  public class WarpBannerChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warpbanner") || chat.StartsWith ("/warpbanner ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.banner")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/warpbanner (?<targetplayername>['].+[']|[^ ]+)( (?<teleportplayername>['].+[']|[^ ]+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /warpbanner [targetplayername] or /warpbanner [targetplayername] [teleportplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Banner targetBanner = null;
      var closestDist = int.MaxValue;
      Banner closestMatch = null;
      for (var c = 0; c < BannerTracker.GetCount (); c++) {
        Banner banner;
        if (BannerTracker.TryGetAtIndex (c, out banner)) {
          if (banner.Owner.Name.ToLower ().Equals (targetPlayerName.ToLower ())) {
            if (targetBanner == null) {
              targetBanner = banner;
            } else {
              Chat.Send (causedBy, $"Duplicate player name");
            }
          } else {
            var levDist = LevenshteinDistance.Compute (banner.Owner.Name.ToLower (), targetPlayerName.ToLower ());
            if (levDist < closestDist) {
              closestDist = levDist;
              closestMatch = banner;
            } else if (levDist == closestDist) {
              closestMatch = null;
            }
          }
        }
      }
      if (targetBanner == null) {
        if (closestMatch != null) {
          targetBanner = closestMatch;
          Log.Write ($"Name '{targetPlayerName}' did not match, picked closest match '{targetBanner.Owner.Name}' instead");
        } else {
          Chat.Send (causedBy, $"Banner not found for '{targetPlayerName}'");
          return true;
        }
      }
      var TeleportPlayer = causedBy;
      var TeleportPlayerName = m.Groups ["teleportplayername"].Value;
      if (TeleportPlayerName.Length > 0) {
        string Error;
        if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
          Chat.Send (causedBy, $"Could not find teleport player '{TeleportPlayerName}'; {Error}");
          return true;
        }
      }
      Teleport.TeleportTo (TeleportPlayer, targetBanner.KeyLocation.Vector);
      return true;
    }
  }
}
