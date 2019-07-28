using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class NoFlightChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals("/noflight")) {
		return false;
		}
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "noflight")) {
        return true;
      }
      foreach (var player in Players.PlayerDatabase.Values) {
        var flightState = player.GetTempValues(false).GetOrDefault("pipliz.setflight", false);
        if (!PermissionsManager.HasPermission (player, "setflight") && flightState) {
          player.GetTempValues(true).Set("pipliz.setflight", false);
          // player.ShouldSave = true;
          if (player.ConnectionState == Players.EConnectionState.Connected) {
            Chat.Send (player, "Please don't fly");
            Players.Disconnect (player);
          } else {
            Log.Write ($"Removed flight state from offline player {player.ID.ToStringReadable()}");
          }
        }
      }
      return true;
    }

  }
}

