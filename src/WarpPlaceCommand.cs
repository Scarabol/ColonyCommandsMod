using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;

namespace ColonyCommands
{

  public class WarpPlaceChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warpplace") || chat.StartsWith ("/warpplace ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "warp.place")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/warpplace (?<px>-?\d+) (?<py>-?\d+)( (?<pz>-?\d+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /warpplace [x] [y] [z] or /warpplace [x] [z]");
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
        vz = vy;
        // TODO vy = TerrainGenerator.UsedGenerator.GetHeight (vx, vz);
		vy = 65;
      }
      causedBy.Position = new UnityEngine.Vector3(vx, vy, vz);
      return true;
    }
  }
}
