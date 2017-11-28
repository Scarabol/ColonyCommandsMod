using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  public static class Events
  {
    public static Vector3Int currentLocation = Vector3Int.invalidPos;
    public static Dictionary<Players.Player, UnityEngine.Vector3> originPositions = new Dictionary<Players.Player, UnityEngine.Vector3> ();
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
        Chat.SendToAll ($"{causedBy.Name} started an event! Use /eventjoin to participate");
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
        Chat.Send (causedBy, "You've joined the event");
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
          Chat.Send (causedBy, "You left the event");
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
        foreach (KeyValuePair<Players.Player, UnityEngine.Vector3> participantEntry in Events.originPositions) {
          ChatCommands.Implementations.Teleport.TeleportTo (participantEntry.Key, participantEntry.Value);
          Chat.Send (participantEntry.Key, $"{causedBy.Name} stopped the event! You've been warped back");
        }
        Events.currentLocation = Vector3Int.invalidPos;
        Events.originPositions.Clear ();
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
