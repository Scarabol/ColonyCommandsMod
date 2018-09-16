using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{
  [ModLoader.ModManager]
  public static class MuteList
  {
    public static readonly Dictionary<Players.Player, long> MutedMinutes = new Dictionary<Players.Player, long> ();

    public static void Update ()
    {
      var tmp = new Dictionary<Players.Player, long> (MutedMinutes);
      foreach (var entry in tmp) {
        if (entry.Value < DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) {
          MutedMinutes.Remove (entry.Key);
          Log.Write ($"Unmuted {entry.Key.Name}");
        }
      }
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.mute.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new MuteChatCommand ());
      CommandManager.RegisterCommand (new UnmuteChatCommand ());
      CommandManager.RegisterCommand (new SilenceChatCommand ());
    }
  }

  public class MuteChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/mute") || chat.StartsWith ("/mute ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      MuteList.Update ();
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "mute")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/mute (?<targetplayername>['].+[']|[^ ]+)( (?<minutes>\d+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /mute [targetplayername] [minutes]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      var minutesStr = m.Groups ["minutes"].Value;
      long minutes;
      if (minutesStr == null || minutesStr.Length < 1 || !long.TryParse (minutesStr, out minutes)) {
        Chat.Send (causedBy, "Could not read minutes value");
        return true;
      }
      MuteList.MutedMinutes.Remove (targetPlayer);
      MuteList.MutedMinutes.Add (targetPlayer, DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + minutes * 60 * 1000);
      Chat.Send (targetPlayer, $"You're muted for {minutes} Minute(s)");
      Log.Write ($"{targetPlayer.Name} muted for {minutes} Minute(s)");
      Chat.Send (causedBy, $"{targetPlayer.Name} muted for {minutes} Minute(s)");
      return true;
    }
  }

  public class UnmuteChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/unmute") || chat.StartsWith ("/unmute ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      MuteList.Update ();
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "mute")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/unmute (?<targetplayername>['].+[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /unmute [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      MuteList.MutedMinutes.Remove (targetPlayer);
      Log.Write ($"Unmuted {targetPlayer.Name}");
      return true;
    }
  }

  public class SilenceChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return true;
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      MuteList.Update ();
      if (MuteList.MutedMinutes.ContainsKey (causedBy)) {
        Chat.Send (causedBy, "[muted]");
        return true;
      }
      return false;
    }
  }
}
