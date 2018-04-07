using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class InactiveChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.inactive.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new InactiveChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/inactive") || chat.StartsWith ("/inactive ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "inactive")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/inactive (?<days>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /inactive [days]");
        return true;
      }
      int days = Int32.Parse (m.Groups ["days"].Value);
      if (days <= 0) {
        Chat.Send (causedBy, "Command didn't match, days too low");
        return true;
      }
      String resultMsg = "";
      foreach (var entry in ActivityTracker.GetInactivePlayers (days)) {
        var player = entry.Key;
        var inactiveDays = entry.Value;
        if (BannerTracker.Get (player) != null) {
          if (resultMsg.Length > 0) {
            resultMsg += ", ";
          }
          resultMsg += $"{player.IDString} ({inactiveDays})";
        }
      }
      if (resultMsg.Length < 1) {
        resultMsg = "No inactive players found";
      }
      Chat.Send (causedBy, resultMsg);
      return true;
    }
  }
}
