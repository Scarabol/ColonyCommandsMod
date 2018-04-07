using System.Collections.Generic;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class CleanBannersChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.cleanbanners.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new CleanBannersChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/cleanbanners");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "cleanbanners")) {
        return true;
      }
      var toClean = new List<Banner> ();
      for (var c = 0; c < BannerTracker.GetCount (); c++) {
        Banner banner;
        if (BannerTracker.TryGetAtIndex (c, out banner)) {
          var bannerPosition = banner.KeyLocation;
          ushort actualType;
          if (World.TryGetTypeAt (bannerPosition, out actualType)) {
            if (actualType != BuiltinBlocks.Banner) {
              toClean.Add (banner);
            }
          } else {
            Chat.Send (causedBy, $"Could not check block type for banner at {bannerPosition}");
          }
        }
      }
      foreach (var banner in toClean) {
        var bannerPosition = banner.KeyLocation;
        BannerTracker.Remove (bannerPosition, BuiltinBlocks.Banner, banner.Owner);
        Chat.Send (causedBy, $"Block at {bannerPosition} is not a banner block, removed it");
      }
      return true;
    }
  }
}
