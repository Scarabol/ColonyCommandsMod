using System.IO;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{
  [ModLoader.ModManager]
  public class TravelChatCommand : IChatCommand
  {

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.travel.loadwaypoints")]
    public static void AfterWorldLoad ()
    {
      WaypointManager.Load ();
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travel");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      foreach (var TravelPath in WaypointManager.travelPaths) {
        if (Pipliz.Math.ManhattanDistance (causedBy.VoxelPosition, TravelPath.Key) < 3) {
          Teleport.TeleportTo (causedBy, TravelPath.Value.Vector);
          return true;
        }
      }
      Chat.Send (causedBy, "You must be close to a waypoint to travel");
      return true;
    }
  }

  public class TravelHereChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travelhere");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "travelpaths")) {
        return true;
      }
      WaypointManager.startWaypoints.Add (causedBy, causedBy.VoxelPosition);
      Chat.Send (causedBy, $"Added start waypoint at {causedBy.VoxelPosition}, use /travelthere to set the endpoint");
      return true;
    }
  }

  public class TravelThereChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travelthere");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "travelpaths")) {
        return true;
      }
      Vector3Int StartWaypoint;
      if (WaypointManager.startWaypoints.TryGetValue (causedBy, out StartWaypoint)) {
        WaypointManager.travelPaths.Add (StartWaypoint, causedBy.VoxelPosition);
        WaypointManager.startWaypoints.Remove (causedBy);
        WaypointManager.Save ();
        Chat.Send (causedBy, $"Saved travel path from {StartWaypoint} to {causedBy.VoxelPosition}");
      } else {
        Chat.Send (causedBy, "You have no start waypoint set, use /travelhere at start point");
      }
      return true;
    }
  }

  public class TravelRemoveChatCommand : IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/travelremove");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "travelpaths")) {
        return true;
      }
      if (WaypointManager.travelPaths.Remove (causedBy.VoxelPosition)) {
        WaypointManager.Save ();
        Chat.Send (causedBy, "Travel path removed");
      } else {
        Chat.Send (causedBy, "No start waypoint found at your position");
      }
      return true;
    }
  }

  public static class WaypointManager
  {
    public static Dictionary<Vector3Int, Vector3Int> travelPaths = new Dictionary<Vector3Int, Vector3Int> ();
    public static Dictionary<Players.Player, Vector3Int> startWaypoints = new Dictionary<Players.Player, Vector3Int> ();

    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "travelpaths.json"));
      }
    }

    public static void Load ()
    {
      JSONNode JsonWaypoints;
      if (JSON.Deserialize (ConfigFilepath, out JsonWaypoints, false)) {
        travelPaths.Clear ();
        foreach (var JsonWaypoint in JsonWaypoints.LoopArray ()) {
          travelPaths.Add ((Vector3Int)JsonWaypoint ["source"], (Vector3Int)JsonWaypoint ["target"]);
        }
        Log.Write ($"Loaded {WaypointManager.travelPaths.Count} travel paths from file");
      } else {
        Log.Write ($"No travel paths loaded. File {ConfigFilepath} not found");
      }
    }

    public static void Save ()
    {
      JSONNode JsonWaypoints = new JSONNode (NodeType.Array);
      foreach (var Waypoint in travelPaths) {
        JsonWaypoints.AddToArray (new JSONNode ().SetAs ("source", (JSONNode)Waypoint.Key).SetAs ("target", (JSONNode)Waypoint.Value));
      }
      JSON.Serialize (ConfigFilepath, JsonWaypoints, 1);
    }
  }
}
