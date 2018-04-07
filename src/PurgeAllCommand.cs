using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using BlockTypes.Builtin;
using NPC;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class PurgeAllChatCommand : IChatCommand
  {
    public static int MIN_DAYS_TO_PURGE = 7;

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.purgeall.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new PurgeAllChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/purgeall") || chat.StartsWith ("/purgeall ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "purgeall")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/purgeall (?<days>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /purgeall [days]");
        return true;
      }
      var days = Int32.Parse (m.Groups ["days"].Value);
      if (days < MIN_DAYS_TO_PURGE) {
        Chat.Send (causedBy, $"Command didn't match, days too low. Min is {MIN_DAYS_TO_PURGE}");
        return true;
      }
      var resultMsg = "";
      foreach (var entry in ActivityTracker.GetInactivePlayers (days)) {
        var player = entry.Key;
        var inactiveDays = entry.Value;
        var banner = BannerTracker.Get (player);
        if (banner != null) {
          var cachedFollowers = new List<NPCBase> (Colony.Get (player).Followers);
          foreach (var npc in cachedFollowers) {
            npc.OnDeath ();
          }
          ServerManager.TryChangeBlock (banner.KeyLocation, BuiltinBlocks.Air);
          BannerTracker.Remove (banner.KeyLocation, BuiltinBlocks.Banner, banner.Owner);
          if (resultMsg.Length > 0) {
            resultMsg += ", ";
          }
          resultMsg += $"{player.IDString} ({inactiveDays})";
        }
      }
      if (resultMsg.Length < 1) {
        resultMsg = "No inactive players found";
      } else {
        resultMsg = "Purged: " + resultMsg;
      }
      Chat.Send (causedBy, resultMsg);
      return true;
    }
  }
}
