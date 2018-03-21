using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class CleanBannersChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.cleanbanners.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new CleanBannersChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/cleanbanners");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "cleanbanners")) {
          return true;
        }
        List<Banner> toClean = new List<Banner> ();
        Vector3Int bannerPosition = Vector3Int.invalidPos;
        for (int c = 0; c < BannerTracker.GetCount (); c++) {
          Banner banner;
          if (BannerTracker.TryGetAtIndex (c, out banner)) {
            bannerPosition = banner.KeyLocation;
            ushort actualType;
            if (World.TryGetTypeAt (bannerPosition, out actualType)) {
              if (actualType != BuiltinBlocks.Banner) {
                toClean.Add (banner);
              }
            } else {
              Chat.Send (causedBy, string.Format ("Could not check block type for banner at {0}", bannerPosition));
            }
          }
        }
        foreach (Banner banner in toClean) {
          bannerPosition = banner.KeyLocation;
          BannerTracker.Remove (bannerPosition, BuiltinBlocks.Banner, banner.Owner);
          Chat.Send (causedBy, string.Format ("Block at {0} is not a banner block, removed it", bannerPosition));
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
