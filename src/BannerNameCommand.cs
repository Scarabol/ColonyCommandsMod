using Pipliz.Chatting;
using ChatCommands;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BannerNameChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.bannername.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new BannerNameChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/bannername");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var position = causedBy.Position;
      var closestBanner = null;
      var shortestDistance = -1;
      for (var c = 0; c < BannerTracker.GetCount(); c++) {
        Banner banner;
        if (BannerTracker.TryGetAtIndex(c, out banner)) {
          if (banner.Owner == causedBy) {
            continue;
          }
          Vector3Int bannerLocation = banner.KeyLocation;
          var distX = position.x - bannerLocation.x;
          var distZ = position.z - bannerLocation.z;
          var distance = (int)System.Math.Sqrt(System.Math.Pow(distX, 2) + System.Math.Pow(distZ, 2));
          if (shortestDistance == -1 || distance < shortestDistance) {
            shortestDistance = distance;
            closestBanner = banner;
          }
        }
      }

      if (closestBanner != null) {
        var ownerName = closestBanner.Owner.Name;
        if (ownerName != null) {
          Chat.Send (causedBy, $"Closest banner at {closestBanner.KeyLocation} is owned by {ownerName}");
        }
      }
      return true;
    }
  }
}
