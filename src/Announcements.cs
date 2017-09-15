using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class Announcements
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.announcements.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new AnnouncementsChatCommand ());
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.announcements.starttimers")]
    public static void AfterWorldLoad ()
    {
      Load ();
      SendNextAnnouncement ();
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerConnectedLate, "scarabol.commands.announcements.onplayerconnectedlate")]
    public static void OnPlayerConnectedLate (Players.Player player)
    {
      if (welcomeMessage.Length > 0) {
        Chat.Send (player, welcomeMessage);
      }
    }

    public static int MIN_INTERVAL = 10;
    public static int CurrentIndex = 0;
    private static JSONNode JsonAnnouncements = new JSONNode ();
    private static string welcomeMessage = "";
    private static int IntervalSeconds = MIN_INTERVAL;
    private static List<ServerMessage> Messages = new List<ServerMessage> ();
    private static int IntervalCounter;

    public static void SendNextAnnouncement ()
    {
      try {
        IntervalCounter += MIN_INTERVAL;
        if (IntervalCounter >= IntervalSeconds) {
          IntervalCounter = 0;
          if (Messages.Count > 0) {
            for (int c = CurrentIndex; c < CurrentIndex + Messages.Count; c++) {
              int Index = c % Messages.Count;
              ServerMessage Message = Messages [Index];
              if (Message.Enabled && Message.Text.Length > 0) {
                CurrentIndex = Index;
                Chat.SendToAll (Message.Text);
                break;
              }
            }
            CurrentIndex = (CurrentIndex + 1) % Messages.Count;
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while sending announcement; {0}", exception.Message));
      }
      ThreadManager.InvokeOnMainThread (delegate () {
        Announcements.SendNextAnnouncement ();
      }, MIN_INTERVAL);
    }

    public static void Load ()
    {
      try {
        JSONNode json;
        if (Pipliz.JSON.JSON.Deserialize (Path.Combine (CommandsModEntries.ModDirectory, "announcements.json"), out json, false)) {
          CurrentIndex = 0;
          JsonAnnouncements = json;
          if (!JsonAnnouncements.TryGetAs ("welcomeMessage", out welcomeMessage)) {
            welcomeMessage = "";
          }
          int intervalSeconds;
          if (JsonAnnouncements.TryGetAs ("intervalSeconds", out intervalSeconds)) {
            IntervalSeconds = System.Math.Max (intervalSeconds, MIN_INTERVAL);
          }
          Pipliz.Log.Write (string.Format ("Using announcements interval {0} seconds", IntervalSeconds));
          JSONNode messages;
          if (!JsonAnnouncements.TryGetAs ("messages", out messages) || messages.NodeType != NodeType.Array) {
            Pipliz.Log.WriteError (string.Format ("No 'messages' array defined in announcements.json"));
          } else {
            Messages.Clear ();
            foreach (JSONNode jsonMsg in messages.LoopArray()) {
              ServerMessage msg = (ServerMessage)jsonMsg;
              if (msg != null) {
                Messages.Add (msg);
              }
            }
            Pipliz.Log.Write (string.Format ("Loaded {0} announcments from file", Messages.Count));
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while loading announcements; {0}", exception.Message));
      }
    }

    private static void Save ()
    {
      try {
        JsonAnnouncements.SetAs ("intervalSeconds", Announcements.IntervalSeconds);
        JSONNode JsonMessages = new JSONNode (NodeType.Array);
        foreach (ServerMessage msg in Messages) {
          JsonMessages.AddToArray ((JSONNode)msg);
        }
        JsonAnnouncements.SetAs ("messages", JsonMessages);
        Pipliz.JSON.JSON.Serialize (Path.Combine (CommandsModEntries.ModDirectory, "announcements.json"), JsonAnnouncements, 3);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while saving announcements; {0}", exception.Message));
      }
    }

    public static void SetIntervalSeconds (int IntervalSeconds)
    {
      Announcements.IntervalSeconds = System.Math.Max (IntervalSeconds, MIN_INTERVAL);
      Save ();
    }

    public static string ListAllAnnouncements ()
    {
      string result = "";
      for (int c = 0; c < Announcements.Messages.Count; c++) {
        Announcements.ServerMessage Message = Announcements.Messages [c];
        result += string.Format ("A{0} ({1}): {2}\n", c, Message.Enabled ? "Enabled" : "Disabled", Message.Text);
      }
      return result;
    }

    public static string ListEnabledAnnouncements ()
    {
      string result = "";
      for (int c = 0; c < Announcements.Messages.Count; c++) {
        Announcements.ServerMessage Message = Announcements.Messages [c];
        if (Message.Enabled) {
          result += string.Format ("A{0} ({1}): {2}\n", c, Message.Enabled ? "Enabled" : "Disabled", Message.Text);
        }
      }
      return result;
    }

    public static void AddAnnouncement (string text)
    {
      Messages.Insert (Announcements.CurrentIndex, new ServerMessage (text));
      Save ();
    }

    public static void RemoveAnnouncement (int index)
    {
      Messages.RemoveAt (index);
      Save ();
    }

    public static void ChangeAnnouncement (int index, string text)
    {
      Messages [index].Text = text;
      Save ();
    }

    public static void MoveAnnouncement (int index, int newIndex)
    {
      ServerMessage msg = Messages [index];
      Messages.RemoveAt (index);
      Messages.Insert (newIndex, msg);
      Save ();
    }

    public static void EnableAnnouncement (int index)
    {
      Messages [index].Enabled = true;
      Save ();
    }

    public static void EnableAllAnnouncements ()
    {
      foreach (ServerMessage msg in Messages) {
        msg.Enabled = true;
      }
      Save ();
    }

    public static void DisableAnnouncement (int index)
    {
      Messages [index].Enabled = false;
      Save ();
    }

    public static void DisableAllAnnouncements ()
    {
      foreach (ServerMessage msg in Messages) {
        msg.Enabled = false;
      }
      Save ();
    }

    public class ServerMessage
    {
      public string Text = "";
      public bool Enabled = true;

      public ServerMessage (string text)
        : this (text, true)
      {
      }

      public ServerMessage (string text, bool enabled)
      {
        this.Text = text;
        this.Enabled = enabled;
      }

      public static explicit operator JSONNode (ServerMessage msg)
      {
        JSONNode json = new JSONNode ();
        json.SetAs ("text", msg.Text);
        json.SetAs ("enabled", msg.Enabled);
        return json;
      }

      public static explicit operator ServerMessage (JSONNode json)
      {
        string text;
        json.TryGetAs ("text", out text);
        if (text == null || text.Length < 1) {
          return null;
        }
        bool enabled;
        if (!json.TryGetAs ("enabled", out enabled)) {
          enabled = true;
        }
        return new ServerMessage (text, enabled);
      }
    }
  }

  public class AnnouncementsChatCommand : ChatCommands.IChatCommand
  {
    private static string ADD_PREFIX = "/announcements add";
    private static string EDIT_PREFIX = "/announcements edit";
    private static string REMOVE_PREFIX = "/announcements remove";
    private static string MOVE_PREFIX = "/announcements move";
    private static string ENABLE_PREFIX = "/announcements enable";
    private static string DISABLE_PREFIX = "/announcements disable";
    private static string INTERVAL_PREFIX = "/announcements interval";

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/announcements") || chat.StartsWith ("/announcements ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
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
        } else {
          Chat.Send (causedBy, "Command didn't match, use /announcements [add|remove|edit|move|enable|disable|interval] [params...]");
        }
        return true;
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }

    public void ListCommand (Players.Player causedBy)
    {
      string msg;
      if (Permissions.PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.list")) {
        msg = Announcements.ListAllAnnouncements ();
      } else {
        msg = Announcements.ListEnabledAnnouncements ();
      }
      if (msg.Length > 0) {
        Chat.Send (causedBy, string.Format ("Server Announcements:\n{0}", msg));
      } else {
        Chat.Send (causedBy, "No announcements");
      }
    }

    public void AddCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.add")) {
        return;
      }
      Announcements.AddAnnouncement (param);
      Chat.Send (causedBy, "New announcement placed next");
    }

    public void RemoveCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.remove")) {
        return;
      }
      int Index;
      if (!TryGetIndex (causedBy, param, REMOVE_PREFIX, out Index)) {
        return;
      }
      Announcements.RemoveAnnouncement (Index);
      Chat.Send (causedBy, string.Format ("Removed announcement {0} from queue", Index));
    }

    public void EditCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.edit")) {
        return;
      }
      var m = Regex.Match (param, @"A?(?<index>\d+) (?<text>.+)");
      if (!m.Success) {
        Chat.Send (causedBy, string.Format ("Params didn't match, use {0} A[index] [text]", EDIT_PREFIX));
        return;
      }
      string StrIndex = m.Groups ["index"].Value;
      int Index;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, string.Format ("Could not parse given parameter '{0}' as index number", StrIndex));
        return;
      }
      string Text = m.Groups ["text"].Value;
      Announcements.ChangeAnnouncement (Index, Text);
      Chat.Send (causedBy, string.Format ("Changed announcement {0}", Index));
    }

    public void MoveCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.move")) {
        return;
      }
      var m = Regex.Match (param, @"A?(?<index>\d+) A?(?<newindex>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, string.Format ("Params didn't match, use {0} A[index] [newindex]", EDIT_PREFIX));
        return;
      }
      string StrIndex = m.Groups ["index"].Value;
      int Index;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, string.Format ("Could not parse given parameter '{0}' as index number", StrIndex));
        return;
      }
      string StrNewIndex = m.Groups ["newindex"].Value;
      int NewIndex;
      if (!int.TryParse (StrNewIndex, out NewIndex)) {
        Chat.Send (causedBy, string.Format ("Could not parse given parameter '{0}' as index number", StrNewIndex));
        return;
      }
      Announcements.MoveAnnouncement (Index, NewIndex);
      Chat.Send (causedBy, string.Format ("Moved announcement {0} to index {1}", Index, NewIndex));
    }

    public void EnableCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.enable")) {
        return;
      }
      if (param.Length > 0) {
        int Index;
        if (!TryGetIndex (causedBy, param, ENABLE_PREFIX, out Index)) {
          return;
        }
        Announcements.EnableAnnouncement (Index);
        Chat.Send (causedBy, string.Format ("Enabled announcement {0}", Index));
      } else {
        Announcements.EnableAllAnnouncements ();
        Chat.Send (causedBy, "Enabled all announcements");
      }
    }

    public void DisableCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.disable")) {
        return;
      }
      if (param.Length > 0) {
        int Index;
        if (!TryGetIndex (causedBy, param, DISABLE_PREFIX, out Index)) {
          return;
        }
        Announcements.DisableAnnouncement (Index);
        Chat.Send (causedBy, string.Format ("Disabled announcement {0}", Index));
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
        Chat.Send (causedBy, string.Format ("Params didn't match, use {0} A[index]", prefix));
        return false;
      }
      string StrIndex = m.Groups ["index"].Value;
      if (!int.TryParse (StrIndex, out Index)) {
        Chat.Send (causedBy, string.Format ("Could not parse given parameter '{0}' as index number", StrIndex));
        return false;
      }
      return true;
    }

    public void IntervalCommand (Players.Player causedBy, string param)
    {
      if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "announcements.interval")) {
        return;
      }
      var m = Regex.Match (param, @"(?<intervalSeconds>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, string.Format ("Params didn't match, use {0} [intervalSeconds]", INTERVAL_PREFIX));
        return;
      }
      int Interval;
      string StrInterval = m.Groups ["intervalSeconds"].Value;
      if (!int.TryParse (StrInterval, out Interval)) {
        Chat.Send (causedBy, string.Format ("Could not parse given parameter '{0}' as index number", StrInterval));
        return;
      }
      Announcements.SetIntervalSeconds (Interval);
      Chat.Send (causedBy, string.Format ("Set announcement interval to {0} seconds", Interval));
    }
  }
}

