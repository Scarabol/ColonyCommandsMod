using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class PurgeAllChatCommand : ChatCommands.IChatCommand
  {
    public static int MIN_DAYS_TO_PURGE = 7;

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.purgeall.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new PurgeAllChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/purgeall") || chat.StartsWith ("/purgeall ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "purgeall")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/purgeall (?<days>\d+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /purgeall [days]");
          return true;
        }
        int days = Int32.Parse (m.Groups ["days"].Value);
        if (days < MIN_DAYS_TO_PURGE) {
          Chat.Send (causedBy, string.Format ("Command didn't match, days too low. Min is {0}", MIN_DAYS_TO_PURGE));
          return true;
        }
        String resultMsg = "";
        foreach (KeyValuePair<Players.Player, long> entry in ActivityTracker.GetInactivePlayers (days)) {
          var player = entry.Key;
          var inactiveDays = entry.Value;
          var banner = BannerTracker.Get (player);
          if (banner != null) {
            List<NPC.NPCBase> cachedFollowers = new List<NPC.NPCBase> (Colony.Get (player).Followers);
            foreach (NPC.NPCBase npc in cachedFollowers) {
              npc.OnDeath ();
            }
            ServerManager.TryChangeBlock (banner.KeyLocation, BuiltinBlocks.Air);
            BannerTracker.Remove (banner.KeyLocation, BuiltinBlocks.Banner, banner.Owner);
            if (resultMsg.Length > 0) {
              resultMsg += ", ";
            }
            resultMsg += string.Format ("{0} ({1})", player.IDString, inactiveDays);
          }
        }
        if (resultMsg.Length < 1) {
          resultMsg = "No inactive players found";
        } else {
          resultMsg = "Purged: " + resultMsg;
        }
        Chat.Send (causedBy, resultMsg);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
