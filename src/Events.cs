using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class Events
  {
    public static Vector3Int currentLocation = Vector3Int.invalidPos;
    public static Dictionary<Players.Player, UnityEngine.Vector3> originPositions = new Dictionary<Players.Player, UnityEngine.Vector3> ();
    public static string msgAllStarted = "{startername} started an event! Use /eventjoin to participate";
    public static string msgPrivJoined = "You've joined the event";
    public static string msgAllJoined = "{playername} joined the event";
    public static string msgPrivLeft = "You left the event";
    public static string msgAllLeft = "{playername} left the event";
    public static string msgPrivStopped = "{stoppername} stopped the event! You've been warped back";
    public static string msgAllStopped = "{stoppername} stopped the event! Thanks for participating";

    private static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "events.json"));
      }
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.events.load")]
    public static void Load ()
    {
      JSONNode jsonConfig;
      if (JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        msgAllStarted = jsonConfig.GetAsOrDefault ("msgAllStarted", "");
        msgPrivJoined = jsonConfig.GetAsOrDefault ("msgPrivJoined", "");
        msgAllJoined = jsonConfig.GetAsOrDefault ("msgAllJoined", "");
        msgPrivLeft = jsonConfig.GetAsOrDefault ("msgPrivLeft", "");
        msgAllLeft = jsonConfig.GetAsOrDefault ("msgAllLeft", "");
        msgPrivStopped = jsonConfig.GetAsOrDefault ("msgPrivStopped", "");
        msgAllStopped = jsonConfig.GetAsOrDefault ("msgAllStopped", "");
      } else {
        Save ();
        Pipliz.Log.Write ($"Could not find {ConfigFilepath} file, created default one");
      }
    }

    public static void Save ()
    {
      JSONNode jsonConfig;
      if (!JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
        jsonConfig = new JSONNode ();
      }
      jsonConfig.SetAs ("msgAllStarted", msgAllStarted);
      jsonConfig.SetAs ("msgPrivJoined", msgPrivJoined);
      jsonConfig.SetAs ("msgAllJoined", msgAllJoined);
      jsonConfig.SetAs ("msgPrivLeft", msgPrivLeft);
      jsonConfig.SetAs ("msgAllLeft", msgAllLeft);
      jsonConfig.SetAs ("msgPrivStopped", msgPrivStopped);
      jsonConfig.SetAs ("msgAllStopped", msgAllStopped);
      JSON.Serialize (ConfigFilepath, jsonConfig, 2);
    }
  }

  [ModLoader.ModManager]
  public class EventStartChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.eventstart.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new EventStartChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/eventstart");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "events")) {
          return true;
        }
        if (Events.currentLocation != Vector3Int.invalidPos) {
          Chat.Send (causedBy, "There is already an ongoing event");
          return true;
        }
        Events.currentLocation = causedBy.VoxelPosition;
        Events.originPositions.Clear ();
        if (Events.msgAllStarted.Length > 0) {
          Chat.SendToAll (Events.msgAllStarted.Replace ("{startername}", causedBy.Name));
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class EventJoinChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.eventjoin.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new EventJoinChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/eventjoin");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (Events.currentLocation == Vector3Int.invalidPos) {
          Chat.Send (causedBy, "There is currently no event ongoing");
          return true;
        }
        Events.originPositions.Add (causedBy, causedBy.Position);
        ChatCommands.Implementations.Teleport.TeleportTo (causedBy, Events.currentLocation.Vector);
        if (!string.IsNullOrEmpty (Events.msgPrivJoined)) {
          Chat.Send (causedBy, Events.msgPrivJoined);
          if (!string.IsNullOrEmpty (Events.msgAllJoined)) {
            Chat.SendToAllBut (causedBy, Events.msgAllJoined.Replace ("{playername}", causedBy.Name));
          }
        } else if (!string.IsNullOrEmpty (Events.msgAllJoined)) {
          Chat.SendToAll (Events.msgAllJoined.Replace ("{playername}", causedBy.Name));
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class EventLeaveChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.eventleave.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new EventLeaveChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/eventleave");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        UnityEngine.Vector3 originPosition;
        if (Events.originPositions.TryGetValue (causedBy, out originPosition) && Events.originPositions.Remove (causedBy)) {
          ChatCommands.Implementations.Teleport.TeleportTo (causedBy, originPosition);
          if (!string.IsNullOrEmpty (Events.msgPrivLeft)) {
            Chat.Send (causedBy, Events.msgPrivLeft);
            if (!string.IsNullOrEmpty (Events.msgAllLeft)) {
              Chat.SendToAllBut (causedBy, Events.msgAllLeft.Replace ("{playername}", causedBy.Name));
            }
          } else if (!string.IsNullOrEmpty (Events.msgAllLeft)) {
            Chat.SendToAll (Events.msgAllLeft.Replace ("{playername}", causedBy.Name));
          }
        } else {
          Chat.Send (causedBy, "You're not participating in an event");
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class EventEndChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.eventend.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new EventEndChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/eventend");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "events")) {
          return true;
        }
        if (Events.currentLocation == Vector3Int.invalidPos) {
          Chat.Send (causedBy, "There is currently no event ongoing");
          return true;
        }
        Events.currentLocation = Vector3Int.invalidPos;
        foreach (KeyValuePair<Players.Player, UnityEngine.Vector3> participantEntry in Events.originPositions) {
          ChatCommands.Implementations.Teleport.TeleportTo (participantEntry.Key, participantEntry.Value);
          if (!string.IsNullOrEmpty (Events.msgPrivStopped)) {
            Chat.Send (causedBy, Events.msgPrivStopped.Replace ("{stoppername}", causedBy.Name));
          }
        }
        Events.originPositions.Clear ();
        if (!string.IsNullOrEmpty (Events.msgAllStopped)) {
          Chat.SendToAll (Events.msgAllStopped.Replace ("{stoppername}", causedBy.Name));
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
