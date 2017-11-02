using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BannerNameChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.bannername.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new BannerNameChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/bannername");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        bool foundBanner = false;
        int minDist = 0;
        String ownerName = null;
        Vector3Int bannerPosition = Vector3Int.invalidPos;
        var banners = BannerTracker.GetBanners ();
        for (int c = 0; c < banners.Count; c++) {
          ITrackableBlock banner = banners.GetValueAtIndex (c);
          int dist = Pipliz.Math.ManhattanDistance (banner.KeyLocation, causedBy.VoxelPosition);
          if (dist < minDist || !foundBanner) {
            foundBanner = true;
            minDist = dist;
            ownerName = banner.Owner.Name;
            bannerPosition = banner.KeyLocation;
          }
        }
        if (ownerName != null) {
          Chat.Send (causedBy, string.Format ("Closest banner at {0} is owned by {1}", bannerPosition, ownerName));
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
