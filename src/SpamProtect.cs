using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz.Chatting;

namespace ScarabolMods
{
  public static class MuteList
  {
    public static readonly Dictionary<Players.Player, long> MutedMinutes = new Dictionary<Players.Player, long> ();

    public static void Update ()
    {
      var tmp = new Dictionary<Players.Player, long> (MutedMinutes);
      foreach (var entry in tmp) {
        if (entry.Value < DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) {
          MutedMinutes.Remove (entry.Key);
          Pipliz.Log.Write ($"Unmuted {entry.Key.Name}");
        }
      }
    }
  }

  [ModLoader.ModManager]
  public class MuteChatCommand : ChatCommands.IChatCommand
  {

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.mute.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new MuteChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/mute") || chat.StartsWith ("/mute ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        MuteList.Update ();
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "mute")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/mute (?<targetplayername>['].+?[']|[^ ]+)( (?<minutes>\d+))?");
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
        Pipliz.Log.Write ($"{targetPlayer.Name} muted for {minutes} Minute(s)");
      } catch (Exception exception) {
        Pipliz.Log.WriteError ($"Exception while parsing command; {exception.Message}");
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class UnmuteChatCommand : ChatCommands.IChatCommand
  {

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.unmute.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new UnmuteChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/unmute") || chat.StartsWith ("/unmute ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        MuteList.Update ();
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "mute")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/unmute (?<targetplayername>['].+?[']|[^ ]+)");
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
        Pipliz.Log.Write ($"Unmuted {targetPlayer.Name}");
      } catch (Exception exception) {
        Pipliz.Log.WriteError ($"Exception while parsing command; {exception.Message}");
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class SilenceChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.mute.silence.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new SilenceChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return true;
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        MuteList.Update ();
        if (MuteList.MutedMinutes.ContainsKey (causedBy)) {
          Chat.Send (causedBy, "[muted]");
          return true;
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError ($"Exception while parsing command; {exception.Message}");
      }
      return false;
    }
  }
}
