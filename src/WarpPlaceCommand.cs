using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;

namespace ColonyCommands
{

  public class WarpPlaceChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/warpplace")) {
		return false;
		}
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.place")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/warpplace (?<px>-?\d+) (?<py>-?\d+)( (?<pz>-?\d+))?");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax: /warpplace [x] [y] [z] or /warpplace [x] [z]");
        return true;
      }
      var xCoord = m.Groups ["px"].Value;
      float vx;
      if (!float.TryParse (xCoord, out vx)) {
        Chat.Send (causedBy, $"Failure parsing first coordinate '{xCoord}'");
        return true;
      }
      var yCoord = m.Groups ["py"].Value;
      float vy;
      if (!float.TryParse (yCoord, out vy)) {
        Chat.Send (causedBy, $"Failure parsing second coordinate '{yCoord}'");
        return true;
      }
      var zCoord = m.Groups ["pz"].Value;
      float vz;
      if (zCoord.Length > 0) {
        if (!float.TryParse (zCoord, out vz)) {
          Chat.Send (causedBy, $"Failure parsing third coordinate '{zCoord}'");
          return true;
        }
      } else {
		TerrainGenerator gen = (TerrainGenerator)ServerManager.TerrainGenerator;
        vz = vy;
		vy = (float)(gen.QueryData((int)vx, (int)vz).Height + 1);
      }
	Helper.TeleportPlayer(causedBy, new UnityEngine.Vector3(vx, vy, vz));
      return true;
    }
  }
}
