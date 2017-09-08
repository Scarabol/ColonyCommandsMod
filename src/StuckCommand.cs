using System;
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
  [ModLoader.ModManager]
  public class StuckChatCommand : ChatCommands.IChatCommand
  {
    private static Dictionary<Players.Player, long> rescueOperations = new Dictionary<Players.Player, long> ();
    private static Dictionary<Players.Player, Vector3Int> stuckPositions = new Dictionary<Players.Player, Vector3Int> ();

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.stuck.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new StuckChatCommand ());
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnPlayerMoved, "scarabol.commands.stuck.onplayermoved")]
    public static void OnPlayerMoved (Players.Player player)
    {
      Vector3Int stuckPos;
      if (stuckPositions.TryGetValue (player, out stuckPos) && Pipliz.Math.ManhattanDistance (player.VoxelPosition, stuckPos) > 0) {
        rescueOperations.Remove (player);
        stuckPositions.Remove (player);
        Chat.Send (player, "Oh you got free! Rescue mission aborted.");
      }
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/stuck");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (causedBy == null || causedBy.ID == NetworkID.Server) {
          return true;
        }
        rescueOperations.Remove (causedBy);
        stuckPositions.Remove (causedBy);
        long rescueId = Pipliz.Random.NextLong ();
        rescueOperations.Add (causedBy, rescueId);
        stuckPositions.Add (causedBy, causedBy.VoxelPosition);
        Chat.Send (causedBy, "Please don't move for 1 Minute. Help is on the way!");
        ThreadManager.InvokeOnMainThread (delegate () {
          long actualId;
          if (rescueOperations.TryGetValue (causedBy, out actualId) && actualId == rescueId) {
            ChatCommands.Implementations.Teleport.TeleportTo (causedBy, TerrainGenerator.GetSpawnLocation ().Vector);
            Chat.Send (causedBy, "Thank you for your patience. Have a nice day!");
          }
        }, 60.0f);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
