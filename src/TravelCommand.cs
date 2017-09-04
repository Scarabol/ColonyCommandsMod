﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  public class TravelChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travel");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        foreach (KeyValuePair<Vector3Int, Vector3Int> TravelPath in  WaypointManager.TravelPaths) {
          if (Pipliz.Math.ManhattanDistance (causedBy.VoxelPosition, TravelPath.Key) < 3) {
            ChatCommands.Implementations.Teleport.TeleportTo (causedBy, TravelPath.Value.Vector);
            return true;
          }
        }
        Chat.Send (causedBy, "You must be close to a waypoint to travel");
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  public class TravelHereChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travelhere");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "travelpaths")) {
          return true;
        }
        WaypointManager.StartWaypoints.Add (causedBy, causedBy.VoxelPosition);
        Chat.Send (causedBy, string.Format ("Added start waypoint at {0}, use /travelthere to set the endpoint", causedBy.VoxelPosition));
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  public class TravelThereChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travelthere");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "travelpaths")) {
          return true;
        }
        Vector3Int StartWaypoint;
        if (WaypointManager.StartWaypoints.TryGetValue (causedBy, out StartWaypoint)) {
          WaypointManager.TravelPaths.Add (StartWaypoint, causedBy.VoxelPosition);
          WaypointManager.StartWaypoints.Remove (causedBy);
          WaypointManager.Save ();
          Chat.Send (causedBy, string.Format ("Saved travel path from {0} to {1}", StartWaypoint, causedBy.VoxelPosition));
        } else {
          Chat.Send (causedBy, "You have no start waypoint set, use /travelhere at start point");
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  public static class WaypointManager
  {
    public static Dictionary<Vector3Int, Vector3Int> TravelPaths = new Dictionary<Vector3Int, Vector3Int> ();
    public static Dictionary<Players.Player, Vector3Int> StartWaypoints = new Dictionary<Players.Player, Vector3Int> ();

    public static void Load ()
    {
      JSONNode JsonWaypoints;
      if (Pipliz.JSON.JSON.Deserialize (Path.Combine (CommandsModEntries.ModDirectory, "travelpaths.json"), out JsonWaypoints, false)) {
        TravelPaths.Clear ();
        foreach (JSONNode JsonWaypoint in JsonWaypoints.LoopArray()) {
          TravelPaths.Add ((Vector3Int)JsonWaypoint ["source"], (Vector3Int)JsonWaypoint ["target"]);
        }
        Pipliz.Log.Write (string.Format ("Loaded {0} travel paths from file", WaypointManager.TravelPaths.Count));
      } else {
        Pipliz.Log.Write ("No travel paths loaded. File travelpaths.json not found in mod directory");
      }
    }

    public static void Save ()
    {
      JSONNode JsonWaypoints = new JSONNode (NodeType.Array);
      foreach (KeyValuePair<Vector3Int, Vector3Int> Waypoint in TravelPaths) {
        JsonWaypoints.AddToArray (new JSONNode ().SetAs ("source", (JSONNode)Waypoint.Key).SetAs ("target", (JSONNode)Waypoint.Value));
      }
      Pipliz.JSON.JSON.Serialize (Path.Combine (CommandsModEntries.ModDirectory, "travelpaths.json"), JsonWaypoints, 1);
    }
  }
}