using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;
using Pipliz.JSON;
using Pipliz.Threading;

namespace ColonyCommands
{

  public class AnnouncementsChatCommand : IChatCommand
  {
    static string ADD_PREFIX = "/announcements add";
    static string EDIT_PREFIX = "/announcements edit";
    static string REMOVE_PREFIX = "/announcements remove";
    static string MOVE_PREFIX = "/announcements move";
    static string ENABLE_PREFIX = "/announcements enable";
    static string DISABLE_PREFIX = "/announcements disable";
    static string INTERVAL_PREFIX = "/announcements interval";
    static string WELCOME_PREFIX = "/announcements welcome";

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!splits[0].Equals ("/announcements")) {
		return false;
	 }
      if (chattext.Equals ("/announcements") || chattext.Equals ("/announcements list")) {
        ListCommand (causedBy);
      } else if (chattext.Equals (ADD_PREFIX) || chattext.StartsWith (ADD_PREFIX + " ")) {
        AddCommand (causedBy, chattext.Substring (ADD_PREFIX.Length).Trim ());
      } else if (chattext.Equals (REMOVE_PREFIX) || chattext.StartsWith (REMOVE_PREFIX + " ")) {
        RemoveCommand (causedBy, chattext.Substring (REMOVE_PREFIX.Length).Trim ());
      } else if (chattext.Equals (EDIT_PREFIX) || chattext.StartsWith (EDIT_PREFIX)) {
        EditCommand (causedBy, chattext.Substring (EDIT_PREFIX.Length).Trim ());
      } else if (chattext.Equals (MOVE_PREFIX) || chattext.StartsWith (MOVE_PREFIX)) {
        MoveCommand (causedBy, chattext.Substring (MOVE_PREFIX.Length).Trim ());
      } else if (chattext.Equals (ENABLE_PREFIX) || chattext.StartsWith (ENABLE_PREFIX + " ")) {
        EnableCommand (causedBy, chattext.Substring (ENABLE_PREFIX.Length).Trim ());
      } else if (chattext.Equals (DISABLE_PREFIX) || chattext.StartsWith (DISABLE_PREFIX + " ")) {
        DisableCommand (causedBy, chattext.Substring (DISABLE_PREFIX.Length).Trim ());
      } else if (chattext.Equals (INTERVAL_PREFIX) || chattext.StartsWith (INTERVAL_PREFIX + " ")) {
        IntervalCommand (causedBy, chattext.Substring (INTERVAL_PREFIX.Length).Trim ());
      } else if (chattext.Equals (WELCOME_PREFIX) || chattext.StartsWith (WELCOME_PREFIX + " ")) {
        WelcomeCommand (causedBy, chattext.Substring (WELCOME_PREFIX.Length).Trim ());
      } else {
        Chat.Send (causedBy, "Command didn't match, use /announcements [add|remove|edit|move|enable|disable|interval] [params...]");
      }
      return true;
    }

    public void ListCommand (Players.Player causedBy)
    {
      string msg;
      if (PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.list")) {
        msg = Announcements.ListAllAnnouncements ();
      } else {
        msg = Announcements.ListEnabledAnnouncements ();
      }
      if (msg.Length > 0) {
        Chat.Send (causedBy, $"Server Announcements:\n{msg}");
      } else {
        Chat.Send (causedBy, "No announcements");
      }
    }

    public void AddCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.add")) {
        return;
      }
      Announcements.AddAnnouncement (param);
      Chat.Send (causedBy, "New announcement placed next");
    }

    public void RemoveCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.remove")) {
        return;
      }
      int Index;
      if (!TryGetIndex (causedBy, param, REMOVE_PREFIX, out Index)) {
        return;
      }
      Announcements.RemoveAnnouncement (Index);
      Chat.Send (causedBy, $"Removed announcement {Index} from queue");
    }

    public void EditCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.edit")) {
        return;
      }
      var m = Regex.Match (param, @"A?(?<index>\d+) (?<text>.+)");
      if (!m.Success) {
        Chat.Send (causedBy, $"Params didn't match, use {EDIT_PREFIX} A[index] [text]");
        return;
      }
      string StrIndex = m.Groups ["index"].Value;
      int Index;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, $"Could not parse given parameter '{StrIndex}' as index number");
        return;
      }
      string Text = m.Groups ["text"].Value;
      Announcements.ChangeAnnouncement (Index, Text);
      Chat.Send (causedBy, $"Changed announcement {Index}");
    }

    public void MoveCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.move")) {
        return;
      }
      var m = Regex.Match (param, @"A?(?<index>\d+) A?(?<newindex>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, $"Params didn't match, use {EDIT_PREFIX} A[index] [newindex]");
        return;
      }
      string StrIndex = m.Groups ["index"].Value;
      int Index;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, $"Could not parse given parameter '{StrIndex}' as index number");
        return;
      }
      string StrNewIndex = m.Groups ["newindex"].Value;
      int NewIndex;
      if (!int.TryParse (StrNewIndex, out NewIndex)) {
        Chat.Send (causedBy, $"Could not parse given parameter '{StrNewIndex}' as index number");
        return;
      }
      Announcements.MoveAnnouncement (Index, NewIndex);
      Chat.Send (causedBy, $"Moved announcement {Index} to index {NewIndex}");
    }

    public void EnableCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.enable")) {
        return;
      }
      if (param.Length > 0) {
        int Index;
        if (!TryGetIndex (causedBy, param, ENABLE_PREFIX, out Index)) {
          return;
        }
        Announcements.EnableAnnouncement (Index);
        Chat.Send (causedBy, $"Enabled announcement {Index}");
      } else {
        Announcements.EnableAllAnnouncements ();
        Chat.Send (causedBy, "Enabled all announcements");
      }
    }

    public void DisableCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.disable")) {
        return;
      }
      if (param.Length > 0) {
        int Index;
        if (!TryGetIndex (causedBy, param, DISABLE_PREFIX, out Index)) {
          return;
        }
        Announcements.DisableAnnouncement (Index);
        Chat.Send (causedBy, $"Disabled announcement {Index}");
      } else {
        Announcements.DisableAllAnnouncements ();
        Chat.Send (causedBy, "Disabled all announcements");
      }
    }

    protected bool TryGetIndex (Players.Player causedBy, string param, string prefix, out int Index)
    {
      Index = -1;
      var m = Regex.Match (param, @"A?(?<index>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, $"Params didn't match, use {prefix} A[index]");
        return false;
      }
      string StrIndex = m.Groups ["index"].Value;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, $"Could not parse given parameter '{StrIndex}' as index number");
        return false;
      }
      return true;
    }

    public void IntervalCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.interval")) {
        return;
      }
      var m = Regex.Match (param, @"(?<intervalSeconds>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, $"Params didn't match, use {INTERVAL_PREFIX} [intervalSeconds]");
        return;
      }
      int Interval;
      string StrInterval = m.Groups ["intervalSeconds"].Value;
      if (!int.TryParse (StrInterval, out Interval)) {
        Chat.Send (causedBy, $"Could not parse given parameter '{StrInterval}' as index number");
        return;
      }
      Announcements.SetIntervalSeconds (Interval);
      Chat.Send (causedBy, $"Set announcement interval to {Interval} seconds");
    }

    public void WelcomeCommand (Players.Player causedBy, string param)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "announcements.welcome")) {
        return;
      }
      Announcements.SetWelcomeMessage (param);
      if (param.Length > 0) {
        Chat.Send (causedBy, $"Changed welcome message, see below\n{param}");
      } else {
        Chat.Send (causedBy, "Disabled welcome message");
      }
    }
  }
}
