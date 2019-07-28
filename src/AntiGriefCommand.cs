using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;
using Pipliz.JSON;
using TerrainGeneration;

namespace ColonyCommands
{

  public class AntiGriefChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/antigrief")) {
		return false;
	}
      var matched = Regex.Match (chattext, @"/antigrief (?<accesslevel>[^ ]+) ((?<playername>['].+[']|[^ ]+)|((?<rangex>\d+) (?<rangez>\d+))|((?<rangexn>\d+) (?<rangexp>\d+) (?<rangezn>\d+) (?<rangezp>\d+)))$");
      if (!matched.Success) {
        Chat.Send (causedBy, "Command didn't match, use /antigrief [spawn|nospawn|banner|deny] [playername] or /antigrief area [rangex rangez|rangexn rangexp rangezn rangezp]");
        return true;
      }
      var accesslevel = matched.Groups ["accesslevel"].Value;
      var targetPlayerName = matched.Groups ["playername"].Value;

      if (accesslevel.Equals ("area")) {
        if (causedBy == null || causedBy.ID == NetworkID.Server) {
          Log.WriteError ("You can't define custom protection areas as server (missing center)");
          return true;
        } else if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.PERMISSION_SUPER)) {
          return true;
        }
        var rangex = matched.Groups ["rangex"].Value;
        var rangez = matched.Groups ["rangez"].Value;
        int rx, rz;
        var rangexn = matched.Groups ["rangexn"].Value;
        var rangexp = matched.Groups ["rangexp"].Value;
        var rangezn = matched.Groups ["rangezn"].Value;
        var rangezp = matched.Groups ["rangezp"].Value;
        int rxn, rxp, rzn, rzp;
        if (rangex.Length > 0 && int.TryParse (rangex, out rx) && rangez.Length > 0 && int.TryParse (rangez, out rz)) {
          AntiGrief.AddCustomArea (new CustomProtectionArea (causedBy.VoxelPosition, rx, rz));
          Chat.Send (causedBy, $"Added anti grief area at {causedBy.VoxelPosition} with x-range {rx} and z-range {rz}");
        } else if (rangexn.Length > 0 && int.TryParse (rangexn, out rxn) && rangexp.Length > 0 && int.TryParse (rangexp, out rxp) && rangezn.Length > 0 && int.TryParse (rangezn, out rzn) && rangezp.Length > 0 && int.TryParse (rangezp, out rzp)) {
          AntiGrief.AddCustomArea (new CustomProtectionArea (causedBy.VoxelPosition, rxn, rxp, rzn, rzp));
          Chat.Send (causedBy, $"Added anti grief area at {causedBy.VoxelPosition} from x- {rxn} to x+ {rxp} and from z- {rzn} to z+ {rzp}");
        } else {
          Chat.Send (causedBy, $"Could not parse protection area ranges {rangex} {rangez} {rangexn} {rangexp} {rangezn} {rangezp}");
        }
      } else {
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
          Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
          return true;
        }
        if (accesslevel.Equals ("spawn")) {
          if (PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.PERMISSION_SUPER)) {
            PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGrief.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, $"You granted [{targetPlayer.Name}] permission to change the spawn area");
            Chat.Send (targetPlayer, "You got permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("nospawn")) {
          if (PermissionsManager.HasPermission (causedBy, AntiGrief.PERMISSION_SUPER)) {
            PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGrief.PERMISSION_SPAWN_CHANGE);
            Chat.Send (causedBy, $"You revoked permission for [{targetPlayer.Name}] to change the spawn area");
            Chat.Send (targetPlayer, "You lost permission to change the spawn area");
          }
        } else if (accesslevel.Equals ("banner")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You already have this permission");
            return true;
          }
          PermissionsManager.AddPermissionToUser (causedBy, targetPlayer, AntiGrief.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, $"You granted [{targetPlayer.Name}] permission to change your banner area");
          Chat.Send (targetPlayer, $"You got permission to change banner area of [{causedBy.Name}]");
        } else if (accesslevel.Equals ("deny")) {
          if (causedBy.Equals (targetPlayer)) {
            Chat.Send (causedBy, "You can't revoke the permission for yourself");
            return true;
          }
          PermissionsManager.RemovePermissionOfUser (causedBy, targetPlayer, AntiGrief.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
          Chat.Send (causedBy, $"You revoked permission for [{targetPlayer.Name}] to change your banner area");
          Chat.Send (targetPlayer, $"You lost permission to change banner area of [{causedBy.Name}]");
        } else {
          Chat.Send (causedBy, "Unknown access level, use /antigrief [spawn|nospawn|banner|deny] steamid");
        }
      }
      return true;
    }
  }
}
