using Pipliz;
using Chatting;
using Chatting.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColonyCommands
{
  public class HelpCommand : IChatCommand
  {

    private static string[] basicList = {"/antigrief", "/bannername", "/eventjoin", "/eventleave", "/itemid", "/lastseen", "/online", "/serverpop", "/stuck", "/top", "/trade", "/travel", "/trash", "/jailvisit", "/jailleave", "/jailtime", "/whisper (/w)", "/production"};
    private static string[] adminList = {"/announcements", "/ban", "/cleanbanners", "/colonycap", "/drain", "/giveall", "/inactive", "/kick", "/killnpc", "/killplayer", "/noflight", "/purgeall", "/trashplayer", "/travelhere", "/travelthere", "/travelremove", "/warpbanner", "/warp", "/warpplace", "/warpspawn", "/jail", "/jailrelease", "/jailrec", "/setjail", "/areashow", "/eventstart", "/eventend", "/mute", "/unmute", "/deletejobs", "/spawnnpc", "/beds" };

	public HelpCommand() : base()
	{
		Array.Sort(HelpCommand.basicList, StringComparer.InvariantCulture);
		Array.Sort(HelpCommand.adminList, StringComparer.InvariantCulture);
	}

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/help")) {
		return false;
		}
      var m = Regex.Match(chattext, @"/help (?<section>.+)");
      string cmdList;

      // without filter basic command list
      if (!m.Success) {
        cmdList = string.Join(", ", basicList);
        Chat.Send(causedBy, "Type /help warp|event|jail... to filter.");
        Chat.Send(causedBy, $"Commands: {cmdList}");
        return true;
      }

      // all admin commands - only if permission
      string filter = m.Groups["section"].Value;
      if (filter.Equals("admin")) {
        if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "adminhelp")) {
          return true;
        }
        cmdList = string.Join(", ", adminList);
        Chat.Send(causedBy, $"Admin commands: {cmdList}");
        return true;
      }

      // filter by matching
      List<string> matching = new List<string>();
      foreach (string elem in basicList) {
        if (elem.Contains(filter)) {
          matching.Add(elem);
        }
      }
      // search admin command only if permission ok
      if (PermissionsManager.HasPermission(causedBy, AntiGrief.MOD_PREFIX + "adminhelp")) {
        foreach (string elem in adminList) {
          if (elem.Contains(filter)) {
            matching.Add(elem);
          }
        }
      }
      cmdList = string.Join(", ", matching.ToArray());
      Chat.Send(causedBy, $"Matching commands: {cmdList}");

      return true;
    }
  }
}
