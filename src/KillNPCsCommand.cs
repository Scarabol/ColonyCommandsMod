﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using NPC;

namespace ColonyCommands
{

  public class KillNPCsChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/killnpcs") || chat.StartsWith ("/killnpcs ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "killnpcs")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/killnpcs (?<targetplayername>['].+[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /killnpcs [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      var cachedFollowers = new List<NPCBase> (Colony.Get (targetPlayer).Followers);
      foreach (var npc in cachedFollowers) {
        npc.OnDeath ();
      }
      return true;
    }
  }
}
