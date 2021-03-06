﻿using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using ChatCommands.Implementations;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WarpChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.warp.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new WarpChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warp") || chat.StartsWith ("/warp ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp.player") &&
          !PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp.self")) {
        Chat.Send (causedBy, "<color=red>You don't have permission to warp</color>");
        return true;
      }
      var m = Regex.Match (chattext, @"/warp (?<targetplayername>['].+?[']|[^ ]+)( (?<teleportplayername>['].+?[']|[^ ]+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /warp [targetplayername] or /warp [targetplayername] [teleportplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      var teleportPlayer = causedBy;
      var teleportPlayerName = m.Groups ["teleportplayername"].Value;
      if (teleportPlayerName.Length > 0) {
        if (PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp.player")) {
          if (!PlayerHelper.TryGetPlayer (teleportPlayerName, out teleportPlayer, out error)) {
            Chat.Send (causedBy, $"Could not find teleport player '{teleportPlayerName}'; {error}");
            return true;
          }
        } else {
          Chat.Send (causedBy, "<color=red>You don't have permission to warp other players</color>");
          return true;
        }
      }
      Teleport.TeleportTo (teleportPlayer, targetPlayer.Position);
      return true;
    }
  }
}
