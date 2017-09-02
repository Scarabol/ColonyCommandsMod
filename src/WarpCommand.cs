using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  public class WarpChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warp") || chat.StartsWith ("/warp ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/warp (?<playername>.+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /warp [playername]");
          return true;
        }
        string targetPlayerName = m.Groups ["playername"].Value;
        if (targetPlayerName.StartsWith ("\"")) {
          if (targetPlayerName.EndsWith ("\"")) {
            targetPlayerName = targetPlayerName.Substring (1, targetPlayerName.Length - 2);
          } else {
            Chat.Send (causedBy, "Command didn't match, missing \" after playername");
            return true;
          }
        }
        if (targetPlayerName.Length < 1) {
          Chat.Send (causedBy, "Command didn't match, no playername given");
          return true;
        }
        Players.Player targetPlayer = null;
        for (int c = 0; c < Players.CountConnected; c++) {
          Players.Player player = Players.GetConnectedByIndex (c);
          if (player.Name != null && player.Name.ToLower ().Equals (targetPlayerName.ToLower ())) {
            if (targetPlayer == null) {
              targetPlayer = player;
            } else {
              Chat.Send (causedBy, "Duplicate target player name, pls use SteamID");
              return true;
            }
          }
        }
        if (targetPlayer == null) {
          Chat.Send (causedBy, string.Format ("Could not find player '{0}'", targetPlayerName));
          return true;
        }
        ChatCommands.Implementations.Teleport.TeleportTo (causedBy, targetPlayer.Position);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
