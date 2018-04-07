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
      var closestBanner = BannerTracker.GetClosest (causedBy, causedBy.VoxelPosition);
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
