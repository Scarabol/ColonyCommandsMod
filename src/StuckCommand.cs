using System.Collections.Generic;
using Pipliz;
using Pipliz.Threading;
using TerrainGeneration;
using Chatting;
using Chatting.Commands;
using UnityEngine;

namespace ColonyCommands
{
  [ModLoader.ModManager]
  public class StuckChatCommand : IChatCommand
  {
    static Dictionary<Players.Player, long> RescueOperations = new Dictionary<Players.Player, long> ();
    static Dictionary<Players.Player, Pipliz.Vector3Int> StuckPositions = new Dictionary<Players.Player, Pipliz.Vector3Int> ();

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerMoved, "scarabol.commands.stuck.onplayermoved")]
    public static void OnPlayerMoved (Players.Player player, Vector3 pos)
    {
      Pipliz.Vector3Int stuckPos;
      if (StuckPositions.TryGetValue (player, out stuckPos) && Pipliz.Math.ManhattanDistance (player.VoxelPosition, stuckPos) > 0) {
        RescueOperations.Remove (player);
        StuckPositions.Remove (player);
        Chat.Send (player, "Oh you got free! Rescue mission aborted.");
      }
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/stuck")) {
		return false;
		}
      if (causedBy == null || causedBy.ID == NetworkID.Server) {
        return true;
      }
      RescueOperations.Remove (causedBy);
      StuckPositions.Remove (causedBy);
      var rescueId = Pipliz.Random.NextLong ();
      RescueOperations.Add (causedBy, rescueId);
      StuckPositions.Add (causedBy, causedBy.VoxelPosition);
      Chat.Send (causedBy, "Please don't move for 1 Minute. Help is on the way!");
      ThreadManager.InvokeOnMainThread (delegate () {
        long actualId;
        if (RescueOperations.TryGetValue (causedBy, out actualId) && actualId == rescueId) {
          Teleport.TeleportTo (causedBy, ServerManager.TerrainGenerator.GetDefaultSpawnLocation().Vector);
          Chat.Send (causedBy, "Thank you for your patience. Have a nice day!");
        }
      }, 60.0f);
      return true;
    }
  }
}
