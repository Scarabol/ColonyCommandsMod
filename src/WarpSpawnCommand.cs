using System.Text.RegularExpressions;
using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;

namespace ColonyCommands
{

  public class WarpSpawnChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/warpspawn")) {
		return false;
		}
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.spawn.self")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/warpspawn ?(?<playername>.+)?");
      if (!m.Success) {
        Chat.Send(causedBy, "use /warpspawn or /warpspawn <playername>");
        return true;
      }
      var TeleportPlayer = causedBy;
      var TeleportPlayerName = m.Groups ["playername"].Value;
      if (TeleportPlayerName.Length > 0) {
        if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "warp.spawn")) {
          return true;
        }
        string Error;
        if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
          Chat.Send (causedBy, $"Could not find teleport player '{TeleportPlayerName}'; {Error}");
          return true;
        }
      }

      Teleport.TeleportTo (TeleportPlayer, ServerManager.TerrainGenerator.GetDefaultSpawnLocation().Vector);
      return true;
    }
  }
}
