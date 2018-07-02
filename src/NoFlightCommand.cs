using Pipliz;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ColonyCommands
{

  public class NoFlightChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/noflight");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "noflight")) {
        return true;
      }
      foreach (var player in Players.PlayerDatabase.ValuesAsList) {
        var flightState = player.GetTempValues(false).GetOrDefault("pipliz.setflight", false);
        if (!PermissionsManager.HasPermission (player, "setflight") && flightState) {
          player.GetTempValues(true).Set("pipliz.setflight", false);
          player.ShouldSave = true;
          if (player.IsConnected) {
            Chat.Send (player, "Please don't fly");
            Players.Disconnect (player);
          } else {
            Log.Write ($"Removed flight state from offline player {player.IDString}");
          }
        }
      }
      return true;
    }

    public static void SendFlightState (Players.Player player, bool state)
    {
      using (ByteBuilder byteBuilder = ByteBuilder.Get ()) {
        byteBuilder.Write (30);
        byteBuilder.Write (state);
        NetworkWrapper.Send (byteBuilder.ToArray (), player, NetworkMessageReliability.ReliableWithBuffering);
      }
    }
  }
}
