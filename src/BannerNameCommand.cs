using Pipliz;
using Pipliz.Chatting;
using ChatCommands;

namespace ScarabolMods
{
  public class BannerNameChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/bannername");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      UnityEngine.Vector3 position = causedBy.Position;
      Banner closestBanner = null;
      int shortestDistance = -1;
      for (var c = 0; c < BannerTracker.GetCount(); c++) {
        Banner banner;
        if (BannerTracker.TryGetAtIndex(c, out banner)) {
          if (banner.Owner == causedBy) {
            continue;
          }
          Vector3Int bannerLocation = banner.KeyLocation;
          double distX = position.x - bannerLocation.x;
          double distZ = position.z - bannerLocation.z;
          int distance = (int)System.Math.Sqrt(System.Math.Pow(distX, 2) + System.Math.Pow(distZ, 2));
          if (shortestDistance == -1 || distance < shortestDistance) {
            shortestDistance = distance;
            closestBanner = banner;
          }
        }
      }

      if (closestBanner != null) {
        var ownerName = closestBanner.Owner.Name;
        if (ownerName != null) {
          Chat.Send (causedBy, $"Closest banner at {closestBanner.KeyLocation} is owned by {ownerName}. {shortestDistance} blocks away");
        }
      }
      return true;
    }
  }
}
