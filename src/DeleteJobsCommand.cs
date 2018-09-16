﻿using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class DeleteJobsCommand : IChatCommand
  {
    private bool includeBeds = false;

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/deletejobs") || chat.StartsWith("/deletejobs "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {

      var m = Regex.Match(chattext, @"/deletejobs (?<beds>includebeds)? ?(?<player>['].+[']|[^ ]+)$");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax error, use /deletejobs <player>");
        return true;
      }

      Players.Player target;
      string targetName = m.Groups["player"].Value;
      string error;
      if (!PlayerHelper.TryGetPlayer(targetName, out target, out error, true)) {
        Chat.Send(causedBy, $"Could not find player {targetName}: {error}");
        return true;
      }

      if (m.Groups["beds"].Value.Equals("includebeds")) {
        includeBeds = true;
      }

      string permission = AntiGrief.MOD_PREFIX + "deletejobs";
      if (target == causedBy) {
        permission += ".self";
      }
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
        return true;
      }

      int amount = DeleteJobsManager.DeleteAreaJobs(causedBy, target);
      amount += DeleteJobsManager.DeleteBlockJobs(causedBy, target);
      string beds = "";
      if (includeBeds) {
        amount += DeleteJobsManager.DeleteBeds(causedBy, target);
        beds = "/Beds";
      }
      Chat.Send(causedBy, $"{amount} Jobs{beds} of player {target.Name} will get deleted in the background");

      return true;
    }

  }

}

