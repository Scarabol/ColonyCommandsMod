using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BannerNameChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.bannername.registercommand")]
    public static void AfterItemTypesServer ()
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
        foreach (ITrackableBlock banner in BannerTracker.GetBanners()) {
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
