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
  [ModLoader.ModManager]
  public static class Announcements
  {
    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "announcements.json"));
      }
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
    public static int CurrentIndex;
    static JSONNode JsonAnnouncements = new JSONNode ();
    static string welcomeMessage = "";
    static int IntervalSeconds = MIN_INTERVAL;
    static List<ServerMessage> Messages = new List<ServerMessage> ();
    static int IntervalCounter;

    public static void SendNextAnnouncement ()
    {
      try {
        IntervalCounter += MIN_INTERVAL;
        if (IntervalCounter >= IntervalSeconds) {
          IntervalCounter = 0;
          if (Messages.Count > 0) {
            for (var c = CurrentIndex; c < CurrentIndex + Messages.Count; c++) {
              int Index = c % Messages.Count;
              ServerMessage Message = Messages [Index];
              if (Message.Enabled && Message.Text.Length > 0) {
                CurrentIndex = Index;
                Chat.SendToConnected (Message.Text);
                break;
              }
            }
            CurrentIndex = (CurrentIndex + 1) % Messages.Count;
          }
        }
      } catch (Exception exception) {
        Log.WriteError ($"Exception while sending announcement; {exception.Message}");
      }
      ThreadManager.InvokeOnMainThread (delegate () {
        SendNextAnnouncement ();
      }, MIN_INTERVAL);
    }

    public static void Load ()
    {
      try {
        JSONNode json;
        if (JSON.Deserialize (ConfigFilepath, out json, false)) {
          CurrentIndex = 0;
          JsonAnnouncements = json;
          if (!JsonAnnouncements.TryGetAs ("welcomeMessage", out welcomeMessage)) {
            welcomeMessage = "";
          }
          int intervalSeconds;
          if (JsonAnnouncements.TryGetAs ("intervalSeconds", out intervalSeconds)) {
            IntervalSeconds = System.Math.Max (intervalSeconds, MIN_INTERVAL);
          }
          Log.Write ($"Using announcements interval {IntervalSeconds} seconds");
          JSONNode messages;
          if (!JsonAnnouncements.TryGetAs ("messages", out messages) || messages.NodeType != NodeType.Array) {
            Log.WriteError ($"No 'messages' array defined in {ConfigFilepath}");
          } else {
            Messages.Clear ();
            foreach (var jsonMsg in messages.LoopArray ()) {
              ServerMessage msg = (ServerMessage)jsonMsg;
              if (msg != null) {
                Messages.Add (msg);
              }
            }
            Log.Write ($"Loaded {Messages.Count} announcments from file");
          }
        }
      } catch (Exception exception) {
        Log.WriteError ($"Exception while loading announcements; {exception.Message}");
      }
    }

    static void Save ()
    {
      try {
        JsonAnnouncements.SetAs ("welcomeMessage", welcomeMessage);
        JsonAnnouncements.SetAs ("intervalSeconds", IntervalSeconds);
        JSONNode JsonMessages = new JSONNode (NodeType.Array);
        foreach (var msg in Messages) {
          JsonMessages.AddToArray ((JSONNode)msg);
        }
        JsonAnnouncements.SetAs ("messages", JsonMessages);
        JSON.Serialize (ConfigFilepath, JsonAnnouncements, 3);
      } catch (Exception exception) {
        Log.WriteError ($"Exception while saving announcements; {exception.Message}");
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
      for (var c = 0; c < Messages.Count; c++) {
        ServerMessage Message = Messages [c];
        result += string.Format ("A{0} ({1}): {2}\n", c, Message.Enabled ? "Enabled" : "Disabled", Message.Text);
      }
      return result;
    }

    public static string ListEnabledAnnouncements ()
    {
      string result = "";
      for (var c = 0; c < Messages.Count; c++) {
        ServerMessage Message = Messages [c];
        if (Message.Enabled) {
          result += string.Format ("A{0} ({1}): {2}\n", c, Message.Enabled ? "Enabled" : "Disabled", Message.Text);
        }
      }
      return result;
    }

    public static void AddAnnouncement (string text)
    {
      Messages.Insert (CurrentIndex, new ServerMessage (text));
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
      Messages.ForEach (message => message.Enabled = true);
      Save ();
    }

    public static void DisableAnnouncement (int index)
    {
      Messages [index].Enabled = false;
      Save ();
    }

    public static void DisableAllAnnouncements ()
    {
      Messages.ForEach (message => message.Enabled = false);
      Save ();
    }

    public static void SetWelcomeMessage (string text)
    {
      welcomeMessage = text;
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
        Text = text;
        Enabled = enabled;
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
}
